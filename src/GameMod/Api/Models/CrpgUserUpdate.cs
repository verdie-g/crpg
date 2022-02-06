namespace Crpg.GameMod.Api.Models;

// Copy of Crpg.Application.Games.Models.GameUserUpdate
internal class CrpgUserUpdate
{
    public int CharacterId { get; set; }
    public CrpgUserReward? Reward { get; set; }
    public IList<CrpgUserBrokenItem> BrokenItems { get; set; } = Array.Empty<CrpgUserBrokenItem>();
}
