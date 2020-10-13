using System.Threading;
using System.Threading.Tasks;
using Crpg.GameMod.Api.Models;

namespace Crpg.GameMod.Api
{
    internal interface ICrpgClient
    {
        Task<CrpgResult<CrpgGameUpdateResponse>> Update(CrpgGameUpdateRequest req, CancellationToken cancellationToken = default);
    }
}
