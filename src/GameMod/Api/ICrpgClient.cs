using System;
using System.Threading;
using System.Threading.Tasks;
using Crpg.GameMod.Api.Models;
using Crpg.GameMod.Api.Models.Users;
using NetworkMessages.FromServer;

namespace Crpg.GameMod.Api
{
    internal interface ICrpgClient : IDisposable
    {
        Task Initialize();

        Task<CrpgResult<CrpgUser>> GetUser(Platform platform, string platformUserId,
            string characterName, CancellationToken cancellationToken = default);

        Task<CrpgResult<CrpgUsersUpdateResponse>> Update(CrpgGameUsersUpdateRequest req, CancellationToken cancellationToken = default);
    }
}
