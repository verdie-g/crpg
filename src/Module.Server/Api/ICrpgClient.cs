using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Api.Models.Users;

namespace Crpg.Module.Api;

internal interface ICrpgClient : IDisposable
{
    Task<CrpgResult<CrpgUser>> GetUser(Platform platform, string platformUserId,
        string characterName, CancellationToken cancellationToken = default);

    Task<CrpgResult<IList<CrpgItem>>> GetItems(CancellationToken cancellationToken = default);

    Task<CrpgResult<CrpgUsersUpdateResponse>> Update(CrpgGameUsersUpdateRequest req, CancellationToken cancellationToken = default);
}
