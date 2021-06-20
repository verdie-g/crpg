using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Battles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Battles.Commands
{
    public record RespondToBattleMercenaryApplicationCommand : IMediatorRequest<BattleMercenaryApplicationViewModel>
    {
        public int HeroId { get; init; }
        public int MercenaryApplicationId { get; init; }
        public bool Accept { get; init; }

        internal class Handler : IMediatorRequestHandler<RespondToBattleMercenaryApplicationCommand, BattleMercenaryApplicationViewModel>
        {
            private static readonly ILogger Logger = LoggerFactory.CreateLogger<RespondToBattleMercenaryApplicationCommand>();

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly ICharacterClassModel _characterClassModel;

            public Handler(ICrpgDbContext db, IMapper mapper, ICharacterClassModel characterClassModel)
            {
                _db = db;
                _mapper = mapper;
                _characterClassModel = characterClassModel;
            }

            public async Task<Result<BattleMercenaryApplicationViewModel>> Handle(RespondToBattleMercenaryApplicationCommand req,
                CancellationToken cancellationToken)
            {
                var hero = await _db.Heroes.FirstOrDefaultAsync(h => h.Id == req.HeroId, cancellationToken);
                if (hero == null)
                {
                    return new(CommonErrors.HeroNotFound(req.HeroId));
                }

                var application = await _db.BattleMercenaryApplications
                    .AsSplitQuery()
                    .Include(a => a.Battle!).ThenInclude(b => b.Fighters.Where(f => f.HeroId == req.HeroId))
                    .Include(a => a.Character!).ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(a => a.Id == req.MercenaryApplicationId, cancellationToken);
                if (application == null)
                {
                    return new(CommonErrors.ApplicationNotFound(req.MercenaryApplicationId));
                }

                var heroFighter = application.Battle!.Fighters.FirstOrDefault();
                if (heroFighter == null)
                {
                    return new(CommonErrors.HeroNotAFighter(hero.Id, application.BattleId));
                }

                if (heroFighter.Side != application.Side)
                {
                    return new(CommonErrors.HeroesNotOnTheSameSide(hero.Id, 0,
                        application.BattleId));
                }

                if (application.Battle.Phase != BattlePhase.Hiring)
                {
                    return new(CommonErrors.BattleInvalidPhase(application.BattleId, application.Battle.Phase));
                }

                if (application.Status != BattleMercenaryApplicationStatus.Pending)
                {
                    return new(CommonErrors.ApplicationClosed(application.Id));
                }

                if (req.Accept)
                {
                    application.Status = BattleMercenaryApplicationStatus.Accepted;
                    BattleMercenary newMercenary = new()
                    {
                        Side = application.Side,
                        Character = application.Character,
                        Battle = application.Battle,
                        CaptainFighter = heroFighter,
                        Application = application,
                    };
                    _db.BattleMercenaries.Add(newMercenary);

                    // Delete all other applying hero pending applications for this battle.
                    var otherApplications = await _db.BattleMercenaryApplications
                        .Where(a => a.Id != application.Id
                                    && a.BattleId == application.BattleId
                                    && a.Character!.UserId == application.Character!.UserId
                                    && a.Status == BattleMercenaryApplicationStatus.Pending)
                        .ToArrayAsync(cancellationToken);
                    _db.BattleMercenaryApplications.RemoveRange(otherApplications);
                }
                else
                {
                    application.Status = BattleMercenaryApplicationStatus.Declined;
                }

                await _db.SaveChangesAsync(cancellationToken);
                Logger.LogInformation(
                    "Hero '{0}' {1} application '{2}' from character '{3}' to join battle '{4}' as a mercenary",
                    req.HeroId, req.Accept ? "accepted" : "declined", req.MercenaryApplicationId,
                    application.CharacterId, application.BattleId);
                return new(new BattleMercenaryApplicationViewModel
                {
                    Id = application.Id,
                    User = _mapper.Map<UserPublicViewModel>(application.Character!.User),
                    Character = new CharacterPublicViewModel
                    {
                        Id = application.Character.Id,
                        Level = application.Character.Level,
                        Class = _characterClassModel.ResolveCharacterClass(application.Character.Statistics),
                    },
                    Wage = application.Wage,
                    Note = application.Note,
                    Side = application.Side,
                    Status = application.Status,
                });
            }
        }
    }
}
