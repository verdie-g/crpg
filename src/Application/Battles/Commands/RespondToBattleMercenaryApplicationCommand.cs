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

namespace Crpg.Application.Battles.Commands;

public record RespondToBattleMercenaryApplicationCommand : IMediatorRequest<BattleMercenaryApplicationViewModel>
{
    public int PartyId { get; init; }
    public int MercenaryApplicationId { get; init; }
    public bool Accept { get; init; }

    internal class Handler : IMediatorRequestHandler<RespondToBattleMercenaryApplicationCommand, BattleMercenaryApplicationViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<RespondToBattleMercenaryApplicationCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly ICharacterClassResolver _characterClassResolver;

        public Handler(ICrpgDbContext db, IMapper mapper, ICharacterClassResolver characterClassResolver)
        {
            _db = db;
            _mapper = mapper;
            _characterClassResolver = characterClassResolver;
        }

        public async Task<Result<BattleMercenaryApplicationViewModel>> Handle(RespondToBattleMercenaryApplicationCommand req,
            CancellationToken cancellationToken)
        {
            var party = await _db.Parties.FirstOrDefaultAsync(h => h.Id == req.PartyId, cancellationToken);
            if (party == null)
            {
                return new(CommonErrors.PartyNotFound(req.PartyId));
            }

            var application = await _db.BattleMercenaryApplications
                .AsSplitQuery()
                .Include(a => a.Battle!).ThenInclude(b => b.Fighters.Where(f => f.PartyId == req.PartyId))
                .Include(a => a.Character!).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(a => a.Id == req.MercenaryApplicationId, cancellationToken);
            if (application == null)
            {
                return new(CommonErrors.ApplicationNotFound(req.MercenaryApplicationId));
            }

            var partyFighter = application.Battle!.Fighters.FirstOrDefault();
            if (partyFighter == null)
            {
                return new(CommonErrors.PartyNotAFighter(party.Id, application.BattleId));
            }

            if (partyFighter.Side != application.Side)
            {
                return new(CommonErrors.PartiesNotOnTheSameSide(party.Id, 0,
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
                    CaptainFighter = partyFighter,
                    Application = application,
                };
                _db.BattleMercenaries.Add(newMercenary);

                // Delete all other applying party pending applications for this battle.
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
                "Party '{0}' {1} application '{2}' from character '{3}' to join battle '{4}' as a mercenary",
                req.PartyId, req.Accept ? "accepted" : "declined", req.MercenaryApplicationId,
                application.CharacterId, application.BattleId);
            return new(new BattleMercenaryApplicationViewModel
            {
                Id = application.Id,
                User = _mapper.Map<UserPublicViewModel>(application.Character!.User),
                Character = new CharacterPublicViewModel
                {
                    Id = application.Character.Id,
                    Level = application.Character.Level,
                    Class = application.Character.Class,
                },
                Wage = application.Wage,
                Note = application.Note,
                Side = application.Side,
                Status = application.Status,
            });
        }
    }
}
