using System.Threading;
using System.Threading.Tasks;
using Crpg.GameMod.Api.Models;

namespace Crpg.GameMod.Api
{
    internal interface ICrpgClient
    {
        Task<CrpgGameUpdateResponse> Update(CrpgGameUpdateRequest req, CancellationToken cancellationToken = default);
    }
}