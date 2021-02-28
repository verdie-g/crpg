using System.Collections.Generic;
using System.Threading.Tasks;
using Crpg.Application.Strategus.Models;

namespace Crpg.Application.Common.Interfaces
{
    internal interface IStrategusSettlementsSource
    {
        Task<IEnumerable<StrategusSettlementCreation>> LoadStrategusSettlements();
    }
}
