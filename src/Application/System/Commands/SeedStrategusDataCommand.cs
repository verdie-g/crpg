using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Strategus;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Crpg.Application.System.Commands
{
    public class SeedStrategusDataCommand : IMediatorRequest
    {
        internal class Handler : IMediatorRequestHandler<SeedStrategusDataCommand>
        {
            private readonly ICrpgDbContext _db;
            private readonly IStrategusSettlementsSource _settlementsSource;
            private readonly Constants _constants;

            public Handler(ICrpgDbContext db, IStrategusSettlementsSource settlementsSource, Constants constants)
            {
                _db = db;
                _settlementsSource = settlementsSource;
                _constants = constants;
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
                            Position = TranslatePositionForRegion(settlementCreation.Position, region),
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

            private Point TranslatePositionForRegion(Point pos, Region region)
            {
                // Europe map is duplicated twice for NorthAmerica and Asia and are put together but NorthAmerica is
                // horizontally mirrored.
                return region switch
                {
                    Region.Europe => new Point(pos.X, pos.Y),
                    Region.NorthAmerica => new Point(_constants.StrategusMapWidth * 2 - pos.X, pos.Y),
                    Region.Asia => new Point(_constants.StrategusMapWidth * 2 + pos.X, pos.Y),
                    _ => throw new ArgumentOutOfRangeException(nameof(region), region, null),
                };
            }
        }
    }
}
