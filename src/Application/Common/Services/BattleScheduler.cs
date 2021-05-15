using System.Threading.Tasks;
using Crpg.Domain.Entities.Battles;

namespace Crpg.Application.Common.Services
{
    internal interface IBattleScheduler
    {
        Task ScheduleBattle(Battle battle);
    }

    internal class BattleScheduler : IBattleScheduler
    {
        public Task ScheduleBattle(Battle battle)
        {
            // TODO
            return Task.CompletedTask;
        }
    }
}
