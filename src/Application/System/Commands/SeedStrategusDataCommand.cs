using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
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

            public Handler(ICrpgDbContext db, IStrategusSettlementsSource settlementsSource)
            {
                _db = db;
                _settlementsSource = settlementsSource;
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
                var dbSettlementsByName = await _db.StrategusSettlements
                    .ToDictionaryAsync(di => di.Name, cancellationToken);

                foreach (var settlementCreation in settlementsByName.Values)
                {
                    var settlement = new StrategusSettlement
                    {
                        Name = settlementCreation.Name,
                        Type = settlementCreation.Type,
                        Culture = settlementCreation.Culture,
                        Position = settlementCreation.Position,
                        Scene = settlementCreation.Scene,
                    };

                    CreateOrUpdateSettlement(dbSettlementsByName, settlement);
                }

                foreach (var dbSettlement in dbSettlementsByName.Values)
                {
                    if (!settlementsByName.ContainsKey(dbSettlement.Name))
                    {
                        _db.StrategusSettlements.Remove(dbSettlement);
                    }
                }
            }

            private void CreateOrUpdateSettlement(Dictionary<string, StrategusSettlement> dbSettlementsByName,
                StrategusSettlement settlement)
            {
                if (dbSettlementsByName.TryGetValue(settlement.Name, out StrategusSettlement? dbSettlement))
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
        }
    }
}
