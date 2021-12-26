using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Heroes;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Heroes.Commands;

public record UpdateHeroTroopsCommand : IMediatorRequest
{
    public TimeSpan DeltaTime { get; init; }

    internal class Handler : IMediatorRequestHandler<UpdateHeroTroopsCommand>
    {
        private readonly ICrpgDbContext _db;
        private readonly Constants _constants;

        public Handler(ICrpgDbContext db, Constants constants)
        {
            _db = db;
            _constants = constants;
        }

        public async Task<Result> Handle(UpdateHeroTroopsCommand req, CancellationToken cancellationToken)
        {
            float deltaTimeHours = (float)req.DeltaTime.TotalHours;
            float recruits = deltaTimeHours * _constants.StrategusTroopRecruitmentPerHour;

            var heroes = _db.Heroes
                .Where(h => h.Status == HeroStatus.RecruitingInSettlement)
                .AsAsyncEnumerable();

            await foreach (var hero in heroes.WithCancellation(cancellationToken))
            {
                hero.Troops = Math.Min(hero.Troops + recruits, _constants.StrategusMaxHeroTroops);
            }

            await _db.SaveChangesAsync(cancellationToken);
            return Result.NoErrors;
        }
    }
}
