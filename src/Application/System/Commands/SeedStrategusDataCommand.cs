using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Strategus;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.System.Commands
{
    public class SeedStrategusDataCommand : IMediatorRequest
    {
        internal class Handler : IMediatorRequestHandler<SeedStrategusDataCommand>
        {
            private readonly ICrpgDbContext _db;
            private readonly IStrategusSettlementsSource _settlementsSource;
            private readonly IStrategusMap _strategusMap;

            public Handler(ICrpgDbContext db, IStrategusSettlementsSource settlementsSource, IStrategusMap strategusMap)
            {
                _db = db;
                _settlementsSource = settlementsSource;
                _strategusMap = strategusMap;
            }

            public async Task<Result> Handle(SeedStrategusDataCommand request, CancellationToken cancellationToken)
            {
                await SeedSettlements(cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
                return new Result();
            }

            private async Task SeedSettlements(CancellationToken cancellationToken)
            {
                var settlementsByName = (await _settlementsSource.LoadStrategusSettlements())
                    .ToDictionary(i => i.Name);
                var dbSettlementsByNameRegion = await _db.StrategusSettlements
                    .ToDictionaryAsync(di => (di.Name, di.Region), cancellationToken);

                foreach (var settlementCreation in settlementsByName.Values)
                {
                    foreach (var region in GetRegions())
                    {
                        var settlement = new StrategusSettlement
                        {
                            Name = settlementCreation.Name,
                            Type = settlementCreation.Type,
                            Culture = settlementCreation.Culture,
                            Region = region,
                            Position = _strategusMap.TranslatePositionForRegion(settlementCreation.Position, Region.Europe, region),
                            Scene = settlementCreation.Scene,
                        };

                        CreateOrUpdateSettlement(dbSettlementsByNameRegion, settlement);
                    }
                }

                foreach (var dbSettlement in dbSettlementsByNameRegion.Values)
                {
                    if (!settlementsByName.ContainsKey(dbSettlement.Name))
                    {
                        _db.StrategusSettlements.Remove(dbSettlement);
                    }
                }
            }

            private void CreateOrUpdateSettlement(Dictionary<(string name, Region region),
                    StrategusSettlement> dbSettlementsByName, StrategusSettlement settlement)
            {
                if (dbSettlementsByName.TryGetValue((settlement.Name, settlement.Region), out StrategusSettlement? dbSettlement))
                {
                    _db.Entry(dbSettlement).State = EntityState.Detached;

                    settlement.Id = dbSettlement.Id;
                    _db.StrategusSettlements.Update(settlement);
                }
                else
                {
                    _db.StrategusSettlements.Add(settlement);
                }
            }

            private static IEnumerable<Region> GetRegions() => Enum.GetValues(typeof(Region)).Cast<Region>();

        }
    }
}
