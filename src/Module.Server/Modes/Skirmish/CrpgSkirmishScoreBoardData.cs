using System.Globalization;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond.MultiplayerBadges;

namespace Crpg.Module.Modes.Skirmish;

internal class CrpgSkirmishScoreboardData : IScoreboardData
{
    public MissionScoreboardComponent.ScoreboardHeader[] GetScoreboardHeaders()
    {
        return new MissionScoreboardComponent.ScoreboardHeader[]
        {
            new("ping", missionPeer => Math.Round(missionPeer.GetNetworkPeer().AveragePingInMilliseconds).ToString(CultureInfo.InvariantCulture), _ => string.Empty),
            new("avatar", _ => string.Empty, _ => string.Empty),
            new("badge", missionPeer => BadgeManager.GetByIndex(missionPeer.GetPeer().ChosenBadgeIndex)?.StringId, _ => string.Empty),
            new("name", missionPeer => missionPeer.DisplayedName, _ => new TextObject("{=hvQSOi79}Bot").ToString()),
            new("kill", missionPeer => missionPeer.KillCount.ToString(), bot => bot.KillCount.ToString()),
            new("death", missionPeer => missionPeer.DeathCount.ToString(), bot => bot.DeathCount.ToString()),
            new("assist", missionPeer => missionPeer.AssistCount.ToString(), bot => bot.AssistCount.ToString()),
            new("life", missionPeer => (CrpgSkirmishSpawningBehavior.MaxSpawns - missionPeer.SpawnCountThisRound).ToString(), _ => "0"),
            new("score", missionPeer => missionPeer.Score.ToString(), bot => bot.Score.ToString()),
        };
    }
}
