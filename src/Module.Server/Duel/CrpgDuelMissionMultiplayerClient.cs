using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Duel;

internal class CrpgDuelMissionMultiplayerClient : MissionMultiplayerGameModeDuelClient
{
    public override bool IsGameModeUsingAllowCultureChange => false;

    public override bool IsGameModeUsingAllowTroopChange => false;
    public CrpgDuelMissionMultiplayerClient()
        : base()
    {
    }

    public override bool CanRequestCultureChange()
    {
        return false;
    }

    public override bool CanRequestTroopChange()
    {
        return false;
    }
}
