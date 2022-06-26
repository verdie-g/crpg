using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Api.Models.Users;

namespace Crpg.Module.Api;

internal interface ICrpgClient : IDisposable
{
    Task<CrpgResult<CrpgUser>> GetUserAsync(Platform platform, string platformUserId,
        string userName, CancellationToken cancellationToken = default);

    Task<CrpgResult<IList<CrpgItem>>> GetItemsAsync(CancellationToken cancellationToken = default);

    Task<CrpgResult<CrpgUsersUpdateResponse>> UpdateUsersAsync(CrpgGameUsersUpdateRequest req,
        CancellationToken cancellationToken = default);
}
