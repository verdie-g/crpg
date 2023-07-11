using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.ActivityLogs;
using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Restrictions;
using Crpg.Module.Api.Models.Users;

namespace Crpg.Module.Api;

internal interface ICrpgClient : IDisposable
{
    Task<CrpgResult<CrpgUser>> GetUserAsync(Platform platform, string platformUserId, CrpgRegion region,
        CancellationToken cancellationToken = default);

    Task<CrpgResult<CrpgUser>> GetTournamentUserAsync(Platform platform, string platformUserId,
        CancellationToken cancellationToken = default);

    Task CreateActivityLogsAsync(IList<CrpgActivityLog> activityLogs, CancellationToken cancellationToken = default);

    Task<CrpgResult<CrpgClan>> GetClanAsync(int clanId, CancellationToken cancellationToken = default);

    Task<CrpgResult<CrpgUsersUpdateResponse>> UpdateUsersAsync(CrpgGameUsersUpdateRequest req,
        CancellationToken cancellationToken = default);

    Task<CrpgResult<CrpgRestriction>> RestrictUserAsync(CrpgRestrictionRequest req,
        CancellationToken cancellationToken = default);
}
