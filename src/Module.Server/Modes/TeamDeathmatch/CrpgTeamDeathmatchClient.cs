using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionRepresentatives;

namespace Crpg.Module.Modes.TeamDeathmatch;

internal class CrpgTeamDeathmatchClient : MissionMultiplayerGameModeBaseClient
{
    private const string BattleWinningSoundEventString = "event:/alerts/report/battle_winning";
    private const string BattleLosingSoundEventString = "event:/alerts/report/battle_losing";
    private const float BattleWinLoseAlertThreshold = 0.1f;

    private TeamDeathmatchMissionRepresentative? _myRepresentative;
    private bool _battleEndingNotificationGiven;

    public override bool IsGameModeUsingGold => false;
    public override bool IsGameModeTactical => false;
    public override bool IsGameModeUsingRoundCountdown => true;
    public override MultiplayerGameType GameType => MultiplayerGameType.TeamDeathmatch;

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        MissionNetworkComponent.OnMyClientSynchronized += OnMyClientSynchronized;
        ScoreboardComponent.OnRoundPropertiesChanged += OnTeamScoresChanged;
    }

    public override void AfterStart()
    {
        Mission.SetMissionMode(MissionMode.Battle, true);
    }

    public override int GetGoldAmount()
    {
        return 0;
    }

    public override void OnGoldAmountChangedForRepresentative(MissionRepresentativeBase representative, int goldAmount)
    {
    }

    public override void OnRemoveBehavior()
    {
        MissionNetworkComponent.OnMyClientSynchronized -= OnMyClientSynchronized;
        ScoreboardComponent.OnRoundPropertiesChanged -= OnTeamScoresChanged;
        base.OnRemoveBehavior();
    }

    private void OnMyClientSynchronized()
    {
        _myRepresentative = GameNetwork.MyPeer.GetComponent<TeamDeathmatchMissionRepresentative>();
    }

    private void OnTeamScoresChanged()
    {
        if (GameNetwork.IsDedicatedServer
            || _battleEndingNotificationGiven
            || _myRepresentative?.MissionPeer.Team == null
            || _myRepresentative.MissionPeer.Team.Side == BattleSideEnum.None)
        {
            return;
        }

        float minScoreToWinMatch = MultiplayerOptions.OptionType.MinScoreToWinMatch.GetIntValue();
        int peerSideRoundScore = ScoreboardComponent.GetRoundScore(_myRepresentative.MissionPeer.Team.Side);
        int oppositeSideRoundScore = ScoreboardComponent.GetRoundScore(_myRepresentative.MissionPeer.Team.Side.GetOppositeSide());
        float peerSideProgression = (minScoreToWinMatch - peerSideRoundScore) / minScoreToWinMatch;
        float oppositeSideProgression = (minScoreToWinMatch - oppositeSideRoundScore) / minScoreToWinMatch;

        MatrixFrame cameraFrame = Mission.Current.GetCameraFrame();
        Vec3 position = cameraFrame.origin + cameraFrame.rotation.u;
        if (peerSideProgression <= BattleWinLoseAlertThreshold && oppositeSideProgression > BattleWinLoseAlertThreshold)
        {
            MBSoundEvent.PlaySound(SoundEvent.GetEventIdFromString(BattleWinningSoundEventString), position);
            _battleEndingNotificationGiven = true;
        }

        if (oppositeSideProgression <= BattleWinLoseAlertThreshold && peerSideProgression > BattleWinLoseAlertThreshold)
        {
            MBSoundEvent.PlaySound(SoundEvent.GetEventIdFromString(BattleLosingSoundEventString), position);
            _battleEndingNotificationGiven = true;
        }
    }
}
