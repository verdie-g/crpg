using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Strategus;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Strategus.Commands
{
    public class UpdateStrategusHeroTroopsCommand : IMediatorRequest
    {
        public TimeSpan DeltaTime { get; set; }

        internal class Handler : IMediatorRequestHandler<UpdateStrategusHeroTroopsCommand>
        {
            private readonly ICrpgDbContext _db;
            private readonly Constants _constants;

            public Handler(ICrpgDbContext db, Constants constants)
            {
                _db = db;
                _constants = constants;
            }

            public async Task<Result> Handle(UpdateStrategusHeroTroopsCommand req, CancellationToken cancellationToken)
            {
                float deltaTimeHours = (float)req.DeltaTime.TotalHours;
                float recruits = deltaTimeHours * _constants.StrategusTroopRecruitmentPerHour;

                var heroes = _db.StrategusHeroes
                    .Where(h => h.Status == StrategusHeroStatus.RecruitingInSettlement)
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
}
