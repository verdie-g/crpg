using System;
using System.Threading;
using System.Threading.Tasks;
using Crpg.GameMod.Api.Models;
using NetworkMessages.FromServer;

namespace Crpg.GameMod.Api
{
    internal interface ICrpgClient : IDisposable
    {
        Task Initialize();
        Task<CrpgResult<CrpgGameUpdateResponse>> Update(CrpgGameUpdateRequest req, CancellationToken cancellationToken = default);
    }
}
