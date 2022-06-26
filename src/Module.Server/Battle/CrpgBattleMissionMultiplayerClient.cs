using Crpg.Module.Common;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Battle;

internal class CrpgBattleMissionMultiplayerClient : MissionMultiplayerGameModeBaseClient
{
    private CrpgPeer? _crpgPeer;

    public override bool IsGameModeUsingGold => false;
    public override bool IsGameModeTactical => false;
    public override bool IsGameModeUsingRoundCountdown => true;
    public override MissionLobbyComponent.MultiplayerGameType GameType => MissionLobbyComponent.MultiplayerGameType.Battle;
    public override bool IsGameModeUsingCasualGold => false;

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        RoundComponent.OnPreparationEnded += OnPreparationEnded;
        MissionNetworkComponent.OnMyClientSynchronized += OnMyClientSynchronized;
    }

    public override void OnRemoveBehavior()
    {
      base.OnRemoveBehavior();
      RoundComponent.OnPreparationEnded -= OnPreparationEnded;
      MissionNetworkComponent.OnMyClientSynchronized -= OnMyClientSynchronized;
    }

    public override void OnGoldAmountChangedForRepresentative(MissionRepresentativeBase representative, int goldAmount)
    {
    }

    public override int GetGoldAmount()
    {
        return 0;
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);
    }

#if CRPG_CLIENT
    protected override void AddRemoveMessageHandlers(
        GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        // TODO: crpg rewards.
        // registerer.Register(new GameNetworkMessage.ServerMessageHandlerDelegate<TDMGoldGain>(HandleServerEventPersonalGoldGain));
    }
#endif

    private void OnPreparationEnded()
    {
    }

    private void OnMyClientSynchronized()
    {
        _crpgPeer = GameNetwork.MyPeer.GetComponent<CrpgPeer>();
    }
}
