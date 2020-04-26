using System.Threading;
using System.Threading.Tasks;
using Crpg.GameMod.Api.Requests;
using Crpg.GameMod.Api.Responses;

namespace Crpg.GameMod.Api
{
    internal interface ICrpgClient
    {
        Task<GameUser> GetOrCreateUser(GetUserRequest req, CancellationToken cancellationToken = default);
        Task<TickResponse> Tick(TickRequest req, CancellationToken cancellationToken = default);
    }
}