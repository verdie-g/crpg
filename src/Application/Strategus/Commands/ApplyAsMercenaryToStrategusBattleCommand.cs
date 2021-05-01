using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Strategus.Models;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Strategus.Battles;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Strategus.Commands
{
    public record ApplyAsMercenaryToStrategusBattleCommand : IMediatorRequest<StrategusBattleMercenaryApplicationViewModel>
    {
        public int UserId { get; init; }
        public int CharacterId { get; init; }
        public int BattleId { get; init; }
        public StrategusBattleSide Side { get; init; }

        public class Validator : AbstractValidator<ApplyAsMercenaryToStrategusBattleCommand>
        {
            public Validator()
            {
                RuleFor(a => a.Side).IsInEnum();
            }
        }

        internal class Handler : IMediatorRequestHandler<ApplyAsMercenaryToStrategusBattleCommand, StrategusBattleMercenaryApplicationViewModel>
        {
            private static readonly ILogger Logger = LoggerFactory.CreateLogger<ApplyAsMercenaryToStrategusBattleCommand>();

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly ICharacterClassModel _characterClassModel;

            public Handler(ICrpgDbContext db, IMapper mapper, ICharacterClassModel characterClassModel)
            {
                _db = db;
                _mapper = mapper;
                _characterClassModel = characterClassModel;
            }

            public async Task<Result<StrategusBattleMercenaryApplicationViewModel>> Handle(
                ApplyAsMercenaryToStrategusBattleCommand req,
                CancellationToken cancellationToken)
            {
                var character = await _db.Characters
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == req.CharacterId && c.UserId == req.UserId, cancellationToken);
                if (character == null)
                {
                    return new(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
                }

                var battle = await _db.StrategusBattles
                    .AsSplitQuery()
                    .Include(b => b.Mercenaries)
                    .FirstOrDefaultAsync(b => b.Id == req.BattleId, cancellationToken);
                if (battle == null)
                {
                    return new(CommonErrors.BattleNotFound(req.BattleId));
                }

                if (battle.Phase != StrategusBattlePhase.Hiring)
                {
                    return new(CommonErrors.BattleInvalidPhase(req.BattleId, battle.Phase));
                }

                // Check for existing application.
                var application = await _db.StrategusBattleMercenaryApplications
                    .Where(a => a.CharacterId == req.CharacterId
                                && a.BattleId == req.BattleId
                                && a.Side == req.Side
                                && (a.Status == StrategusBattleMercenaryApplicationStatus.Pending
                                    || a.Status == StrategusBattleMercenaryApplicationStatus.Accepted))
                    .FirstOrDefaultAsync(cancellationToken);
                if (application == null)
                {
                    application = new StrategusBattleMercenaryApplication
                    {
                        Side = req.Side,
                        Status = StrategusBattleMercenaryApplicationStatus.Pending,
                        Battle = battle,
                        Character = character,
                    };
                    _db.StrategusBattleMercenaryApplications.Add(application);
                    await _db.SaveChangesAsync(cancellationToken);
                    Logger.LogInformation("User '{0}' applied as a mercenary to battle '{1}' with character '{2}'",
                        character.UserId, battle.Id, character.Id);
                }

                return new(new StrategusBattleMercenaryApplicationViewModel
                {
                    Id = application.Id,
                    User = _mapper.Map<UserPublicViewModel>(character.User),
                    Character = new CharacterPublicViewModel
                    {
                        Id = character.Id,
                        Level = character.Level,
                        Class = _characterClassModel.ResolveCharacterClass(character.Statistics),
                    },
                    Side = application.Side,
                    Status = application.Status,
                });
            }
        }
    }
}
