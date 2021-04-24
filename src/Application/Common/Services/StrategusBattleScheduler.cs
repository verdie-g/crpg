using System.Threading.Tasks;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Strategus.Battles;

namespace Crpg.Application.Common.Services
{
    internal interface IStrategusBattleScheduler
    {
        Task ScheduleBattle(StrategusBattle battle);
    }

    internal class StrategusBattleScheduler : IStrategusBattleScheduler
    {
        public Task ScheduleBattle(StrategusBattle battle)
        {
            // TODO
            return Task.CompletedTask;
        }
    }
}
