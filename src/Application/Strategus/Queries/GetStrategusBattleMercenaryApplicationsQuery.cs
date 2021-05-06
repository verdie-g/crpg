using System;
using System.Collections.Generic;
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

namespace Crpg.Application.Strategus.Queries
{
    public record GetStrategusBattleMercenaryApplicationsQuery : IMediatorRequest<IList<StrategusBattleMercenaryApplicationViewModel>>
    {
        public int UserId { get; init; }
        public int BattleId { get; init; }
        public IList<StrategusBattleMercenaryApplicationStatus> Statuses { get; init; } = Array.Empty<StrategusBattleMercenaryApplicationStatus>();

        public class Validator : AbstractValidator<GetStrategusBattleMercenaryApplicationsQuery>
        {
            public Validator()
            {
                RuleFor(a => a.Statuses).ForEach(s => s.IsInEnum());
            }
        }

        internal class Handler : IMediatorRequestHandler<GetStrategusBattleMercenaryApplicationsQuery, IList<StrategusBattleMercenaryApplicationViewModel>>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly ICharacterClassModel _characterClassModel;

            public Handler(ICrpgDbContext db, IMapper mapper, ICharacterClassModel characterClassModel)
            {
                _db = db;
                _mapper = mapper;
                _characterClassModel = characterClassModel;
            }

            public async Task<Result<IList<StrategusBattleMercenaryApplicationViewModel>>> Handle(GetStrategusBattleMercenaryApplicationsQuery req, CancellationToken cancellationToken)
            {
                var battle = await _db.StrategusBattles
                    .AsSplitQuery()
                    .Include(b => b.Fighters.Where(f => f.HeroId == req.UserId))
                    .Include(b => b.MercenaryApplications.Where(a => req.Statuses.Contains(a.Status)))
                    .ThenInclude(m => m.Character!.User)
                    .FirstOrDefaultAsync(b => b.Id == req.BattleId, cancellationToken);
                if (battle == null)
                {
                    return new(CommonErrors.BattleNotFound(req.BattleId));
                }

                // Mercenaries can only apply during the Hiring phase so return an error for preceding phases.
                if (battle.Phase == StrategusBattlePhase.Preparation)
                {
                    return new(CommonErrors.BattleInvalidPhase(req.BattleId, battle.Phase));
                }

                StrategusBattleFighter? fighter = battle.Fighters.FirstOrDefault();
                // If the user is not a fighter of that battle, only return its applications, else return the mercenary
                // applications from the same side as the user.
                var applications = battle.MercenaryApplications
                    .Where(a => a.Character!.UserId == req.UserId || (fighter != null && a.Side == fighter.Side))
                    .Select(m => new StrategusBattleMercenaryApplicationViewModel
                {
                    Id = m.Id,
                    User = _mapper.Map<UserPublicViewModel>(m.Character!.User),
                    Character = new CharacterPublicViewModel
                    {
                        Id = m.Character.Id,
                        Level = m.Character.Level,
                        Class = _characterClassModel.ResolveCharacterClass(m.Character.Statistics),
                    },
                    Wage = m.Wage,
                    Note = m.Note,
                    Side = m.Side,
                }).ToArray();

                return new(applications);
            }
        }
    }
}
