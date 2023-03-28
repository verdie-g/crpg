using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionRepresentatives;

namespace Crpg.Module.Modes.Duel;

internal class CrpgDuelMissionMultiplayerClient : MissionMultiplayerGameModeDuelClient
{
    public override bool IsGameModeUsingAllowCultureChange => false;
    public override bool IsGameModeUsingAllowTroopChange => false;

    public override bool CanRequestCultureChange()
    {
        return false;
    }

    public override bool CanRequestTroopChange()
    {
        return false;
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        if (GameNetwork.IsClientOrReplay)
        {
            registerer.Register<CrpgUpdateDuelArenaType>(HandleCrpgDuelArenaType);
        }
    }

    private void HandleCrpgDuelArenaType(CrpgUpdateDuelArenaType message)
    {
        if (GameNetwork.MyPeer == null)
        {
            return;
        }

        MissionPeer myMissionPeer = GameNetwork.MyPeer.GetComponent<MissionPeer>();
        if (myMissionPeer == null)
        {
            return;
        }

        Action<TroopType> onMyPreferredZoneChanged = ((DuelMissionRepresentative)myMissionPeer.Representative).OnMyPreferredZoneChanged;
        onMyPreferredZoneChanged?.Invoke(message.PlayerTroopType);
    }
}
