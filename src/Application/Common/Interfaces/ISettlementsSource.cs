using System.Collections.Generic;
using System.Threading.Tasks;
using Crpg.Application.Settlements.Models;

namespace Crpg.Application.Common.Interfaces
{
    internal interface ISettlementsSource
    {
        Task<IEnumerable<SettlementCreation>> LoadStrategusSettlements();
    }
}
