using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Battles;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Battles.Queries;

public record GetBattleFighterApplicationsQuery : IMediatorRequest<IList<BattleFighterApplicationViewModel>>
{
    public int HeroId { get; init; }
    public int BattleId { get; init; }
    public IList<BattleFighterApplicationStatus> Statuses { get; init; } = Array.Empty<BattleFighterApplicationStatus>();

    public class Validator : AbstractValidator<GetBattleFighterApplicationsQuery>
    {
        public Validator()
        {
            RuleFor(a => a.Statuses).ForEach(s => s.IsInEnum());
        }
    }

    internal class Handler : IMediatorRequestHandler<GetBattleFighterApplicationsQuery, IList<BattleFighterApplicationViewModel>>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IList<BattleFighterApplicationViewModel>>> Handle(
            GetBattleFighterApplicationsQuery req, CancellationToken cancellationToken)
        {
            var battle = await _db.Battles
                .AsSplitQuery()
                .Include(b => b.Fighters.Where(f => f.HeroId == req.HeroId))
                .Include(b => b.FighterApplications.Where(a => req.Statuses.Contains(a.Status)))
                .ThenInclude(a => a.Hero!).ThenInclude(h => h.User)
                .FirstOrDefaultAsync(b => b.Id == req.BattleId, cancellationToken);
            if (battle == null)
            {
                return new(CommonErrors.BattleNotFound(req.BattleId));
            }

            BattleFighter? fighter = battle.Fighters.FirstOrDefault();
            // If the fighter is a commander, return all applications of their side else return only the hero applications.
            var applications = battle.FighterApplications
                .Where(a => a.HeroId == req.HeroId || (fighter != null && fighter.Commander && a.Side == fighter.Side));
            return new(_mapper.Map<IList<BattleFighterApplicationViewModel>>(applications));
        }
    }
}
