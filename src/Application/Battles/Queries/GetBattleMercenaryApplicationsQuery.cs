using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Battles;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Battles.Queries;

public record GetBattleMercenaryApplicationsQuery : IMediatorRequest<IList<BattleMercenaryApplicationViewModel>>
{
    public int UserId { get; init; }
    public int BattleId { get; init; }
    public IList<BattleMercenaryApplicationStatus> Statuses { get; init; } = Array.Empty<BattleMercenaryApplicationStatus>();

    public class Validator : AbstractValidator<GetBattleMercenaryApplicationsQuery>
    {
        public Validator()
        {
            RuleFor(a => a.Statuses).ForEach(s => s.IsInEnum());
        }
    }

    internal class Handler : IMediatorRequestHandler<GetBattleMercenaryApplicationsQuery, IList<BattleMercenaryApplicationViewModel>>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly ICharacterClassResolver _characterClassResolver;

        public Handler(ICrpgDbContext db, IMapper mapper, ICharacterClassResolver characterClassResolver)
        {
            _db = db;
            _mapper = mapper;
            _characterClassResolver = characterClassResolver;
        }

        public async Task<Result<IList<BattleMercenaryApplicationViewModel>>> Handle(GetBattleMercenaryApplicationsQuery req, CancellationToken cancellationToken)
        {
            var battle = await _db.Battles
                .AsSplitQuery()
                .Include(b => b.Fighters.Where(f => f.PartyId == req.UserId))
                .Include(b => b.MercenaryApplications.Where(a => req.Statuses.Contains(a.Status)))
                .ThenInclude(m => m.Character!.User)
                .FirstOrDefaultAsync(b => b.Id == req.BattleId, cancellationToken);
            if (battle == null)
            {
                return new(CommonErrors.BattleNotFound(req.BattleId));
            }

            // Mercenaries can only apply during the Hiring phase so return an error for preceding phases.
            if (battle.Phase == BattlePhase.Preparation)
            {
                return new(CommonErrors.BattleInvalidPhase(req.BattleId, battle.Phase));
            }

            BattleFighter? fighter = battle.Fighters.FirstOrDefault();
            // If the user is not a fighter of that battle, only return its applications, else return the mercenary
            // applications from the same side as the user.
            var applications = battle.MercenaryApplications
                .Where(a => a.Character!.UserId == req.UserId || (fighter != null && a.Side == fighter.Side))
                .Select(m => new BattleMercenaryApplicationViewModel
                {
                    Id = m.Id,
                    User = _mapper.Map<UserPublicViewModel>(m.Character!.User),
                    Character = new CharacterPublicViewModel
                    {
                        Id = m.Character.Id,
                        Level = m.Character.Level,
                        Class = m.Character.Class,
                    },
                    Wage = m.Wage,
                    Note = m.Note,
                    Side = m.Side,
                    Status = m.Status,
                }).ToArray();

            return new(applications);
        }
    }
}
