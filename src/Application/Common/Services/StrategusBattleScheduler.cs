using System.Threading.Tasks;
using Crpg.Domain.Entities.Strategus;

namespace Crpg.Application.Common.Services
{
    internal interface IStrategusBattleScheduler
    {
        Task ScheduleBattle(StrategusBattle battle);
    }

    internal class StrategusBattleScheduler
    {
        public Task ScheduleBattle(StrategusBattle battle)
        {
            // TODO
            return Task.CompletedTask;
        }
    }
}
