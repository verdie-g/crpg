using Crpg.Module.Modes.Battle.FlagSystems;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;

namespace Crpg.Module.Modes.Battle;

internal class CrpgBattleClient : MissionMultiplayerGameModeBaseClient, ICommanderInfo
{
    private readonly bool _isSkirmish;
    private FlagSystem _flagSystem = default!;

    public event Action<BattleSideEnum, float>? OnMoraleChangedEvent
    {
        add => _flagSystem.OnMoraleChangedEvent += value;
        remove => _flagSystem.OnMoraleChangedEvent -= value;
    }

    public event Action? OnFlagNumberChangedEvent
    {
        add => _flagSystem.OnFlagNumberChangedEvent += value;
        remove => _flagSystem.OnFlagNumberChangedEvent -= value;
    }

    public event Action<FlagCapturePoint, Team?>? OnCapturePointOwnerChangedEvent
    {
        add => _flagSystem.OnCapturePointOwnerChangedEvent += value;
        remove => _flagSystem.OnCapturePointOwnerChangedEvent -= value;
    }

    public CrpgBattleClient(bool isSkirmish)
    {
        _isSkirmish = isSkirmish;
    }

    public override bool IsGameModeUsingGold => false;
    public override bool IsGameModeTactical => AllCapturePoints.Any();
    public override bool IsGameModeUsingRoundCountdown => true;
    public override MissionLobbyComponent.MultiplayerGameType GameType => _isSkirmish
        ? MissionLobbyComponent.MultiplayerGameType.Skirmish
        : MissionLobbyComponent.MultiplayerGameType.Battle;
    public override bool IsGameModeUsingCasualGold => false;
    public IEnumerable<FlagCapturePoint> AllCapturePoints => _flagSystem.AllCapturePoints;
    public bool AreMoralesIndependent => false;

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();

        _flagSystem = new MultipleFlagSystem(Mission, NotificationsComponent, _isSkirmish);
        _flagSystem.Reset(); // Get the flags early so it's available for the HUD.
    }

    public override void AfterStart()
    {
        Mission.Current.SetMissionMode(MissionMode.Battle, true);
    }

    public override void OnClearScene()
    {
        _flagSystem.Reset();
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
        _flagSystem.Tick(dt);
    }

    public Team? GetFlagOwner(FlagCapturePoint flag)
    {
        return _flagSystem.GetFlagOwner(flag);
    }

    protected override int GetWarningTimer()
    {
        if (!IsRoundInProgress)
        {
            return 0;
        }

        return _flagSystem.GetWarningTimer(RoundComponent.RemainingRoundTime,
            MultiplayerOptions.OptionType.RoundTimeLimit.GetIntValue());
    }
}
