using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crpg.GameMod.Api.Models;
using Crpg.GameMod.Api.Models.Items;
using Crpg.GameMod.Api.Models.Users;

namespace Crpg.GameMod.Api
{
    internal interface ICrpgClient : IDisposable
    {
        Task<CrpgResult<CrpgUser>> GetUser(Platform platform, string platformUserId,
            string characterName, CancellationToken cancellationToken = default);

        Task<CrpgResult<IList<CrpgItem>>> GetItems(CancellationToken cancellationToken = default);

        Task<CrpgResult<CrpgUsersUpdateResponse>> Update(CrpgGameUsersUpdateRequest req, CancellationToken cancellationToken = default);
    }
}
