using Crpg.Module.Api.Models.Users;

namespace Crpg.Module.Api.Models;

/// <summary>
/// Copy of Crpg.Application.Games.Models.UpdateGameUserResult.
/// </summary>
internal class UpdateCrpgUserResult
{
    public CrpgUser User { get; set; } = default!;
    public CrpgUserEffectiveReward EffectiveReward { get; set; } = default!;
    public IList<CrpgRepairedItem> RepairedItems { get; set; } = Array.Empty<CrpgRepairedItem>();
}
