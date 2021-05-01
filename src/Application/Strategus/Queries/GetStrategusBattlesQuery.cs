using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Strategus.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Strategus.Battles;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Strategus.Queries
{
    public record GetStrategusBattlesQuery : IMediatorRequest<IList<StrategusBattlePublicViewModel>>
    {
        public Region Region { get; init; }
        public IList<StrategusBattlePhase> Phases { get; init; } = Array.Empty<StrategusBattlePhase>();

        public class Validator : AbstractValidator<GetStrategusBattlesQuery>
        {
            public Validator()
            {
                RuleFor(q => q.Region).IsInEnum();
                RuleFor(q => q.Phases).ForEach(p => p.IsInEnum());
            }
        }

        internal class Handler : IMediatorRequestHandler<GetStrategusBattlesQuery, IList<StrategusBattlePublicViewModel>>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<IList<StrategusBattlePublicViewModel>>> Handle(GetStrategusBattlesQuery req, CancellationToken cancellationToken)
            {
                var battles = await _db.StrategusBattles
                    .AsSplitQuery()
                    .Include(b => b.Fighters).ThenInclude(f => f.Hero!.User)
                    .Include(b => b.AttackedSettlement)
                    .Where(b => b.Region == req.Region && req.Phases.Contains(b.Phase))
                    .ToArrayAsync(cancellationToken);

                var battlesVm = battles.Select(b => new StrategusBattlePublicViewModel
                {
                    Id = b.Id,
                    Region = b.Region,
                    Phase = b.Phase,
                    Attacker = _mapper.Map<StrategusBattleFighterPublicViewModel>(
                        b.Fighters.First(f => f.Side == StrategusBattleSide.Attacker && f.MainFighter)),
                    AttackerTotalTroops = b.Fighters
                        .Where(f => f.Side == StrategusBattleSide.Attacker)
                        .Sum(f => (int)f.Hero!.Troops),
                    Defender = _mapper.Map<StrategusBattleFighterPublicViewModel>(
                        b.Fighters.FirstOrDefault(f => f.Side == StrategusBattleSide.Defender && f.MainFighter)),
                    DefenderTotalTroops = b.Fighters
                            .Where(f => f.Side == StrategusBattleSide.Defender)
                            .Sum(f => (int)f.Hero!.Troops)
                        + (b.AttackedSettlement?.Troops ?? 0),
                    SettlementDefender = _mapper.Map<StrategusSettlementPublicViewModel>(b.AttackedSettlement),
                }).ToArray();

                return new(battlesVm);
            }
        }
    }
}
