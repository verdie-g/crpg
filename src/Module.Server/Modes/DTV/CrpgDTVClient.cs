using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionRepresentatives;

namespace Crpg.Module.Modes.DTV;

internal class CrpgDTVClient : MissionMultiplayerGameModeBaseClient
{
    private MissionPeer? _missionPeer;
    private TeamDeathmatchMissionRepresentative? _myRepresentative;

    public override bool IsGameModeUsingGold => false;
    public override bool IsGameModeTactical => false;
    public override bool IsGameModeUsingRoundCountdown => true;
    public override MissionLobbyComponent.MultiplayerGameType GameType =>
        MissionLobbyComponent.MultiplayerGameType.Battle;
    public override bool IsGameModeUsingCasualGold => false;
    public bool AreMoralesIndependent => false;

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

    public override void AfterStart()
    {
        Mission.Current.SetMissionMode(MissionMode.Battle, true);
    }

    public override void OnClearScene()
    {
    }

    protected override void AddRemoveMessageHandlers(
        GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        if (GameNetwork.IsClientOrReplay)
        {
        }
    }

    private void OnPreparationEnded()
    {
    }

    private void OnMyClientSynchronized()
    {
        _missionPeer = GameNetwork.MyPeer.GetComponent<MissionPeer>();
        _myRepresentative = GameNetwork.MyPeer.GetComponent<TeamDeathmatchMissionRepresentative>();
    }
}
