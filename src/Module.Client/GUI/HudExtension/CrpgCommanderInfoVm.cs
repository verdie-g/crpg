using System.ComponentModel;
using Crpg.Module.Helpers;
using Crpg.Module.Modes.Siege;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.HUDExtensions;
using TaleWorlds.MountAndBlade.Objects;

namespace Crpg.Module.GUI.HudExtension;

public class CrpgCommanderInfoVm : ViewModel
{
    private readonly MissionMultiplayerGameModeBaseClient _gameMode;
    private readonly CrpgSiegeClient? _siegeClient;
    private readonly MissionScoreboardComponent _missionScoreboardComponent;
    private readonly ICommanderInfo? _commanderInfo;
    private int _attackerTeamInitialMemberCount;
    private int _defenderTeamInitialMemberCount;
    private Team? _allyTeam;
    private Team? _enemyTeam;
    private bool _areMoraleEventsRegistered;
    private MBBindingList<CapturePointVM> _allyControlPoints = null!;
    private MBBindingList<CapturePointVM> _neutralControlPoints = null!;
    private MBBindingList<CapturePointVM> _enemyControlPoints = null!;
    private int _allyMoraleIncreaseLevel;
    private int _enemyMoraleIncreaseLevel;
    private int _allyMoralePercentage;
    private int _enemyMoralePercentage;
    private int _allyMemberCount;
    private int _enemyMemberCount;
    private PowerLevelComparer _powerLevelComparer = null!;
    private bool _showTacticalInfo;
    private bool _usePowerComparer;
    private bool _useMoraleComparer;
    private bool _areMoralesIndependent;
    private bool _showControlPointStatus;
    private string _allyTeamColor = null!;
    private string _allyTeamColorSecondary = null!;
    private string _enemyTeamColor = null!;
    private string _enemyTeamColorSecondary = null!;

    public CrpgCommanderInfoVm()
    {
        AllyControlPoints = new MBBindingList<CapturePointVM>();
        NeutralControlPoints = new MBBindingList<CapturePointVM>();
        EnemyControlPoints = new MBBindingList<CapturePointVM>();
        _gameMode = Mission.Current.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
        _missionScoreboardComponent = Mission.Current.GetMissionBehavior<MissionScoreboardComponent>();
        _commanderInfo = Mission.Current.GetMissionBehavior<ICommanderInfo>();
        ShowTacticalInfo = true;
        UpdateWarmupDependentFlags(_gameMode.IsInWarmup);
        UsePowerComparer = _gameMode.GameType == MultiplayerGameType.Battle && _gameMode.ScoreboardComponent != null;
        if (UsePowerComparer)
        {
            PowerLevelComparer = new PowerLevelComparer(1.0, 1.0);
        }

        if (UseMoraleComparer)
        {
            RegisterMoraleEvents();
        }

        _siegeClient = Mission.Current.GetMissionBehavior<CrpgSiegeClient>();
        if (_siegeClient != null)
        {
            _siegeClient.OnCapturePointRemainingMoraleGainsChangedEvent += OnCapturePointRemainingMoraleGainsChanged;
        }

        Mission.Current.OnMissionReset += OnMissionReset;

        var visualSpawnComponent = Mission.Current.GetMissionBehavior<MultiplayerMissionAgentVisualSpawnComponent>();
        if (visualSpawnComponent != null)
        {
            visualSpawnComponent.OnMyAgentSpawnedFromVisual += OnMyAgentSpawnedFromVisual;
        }

        OnTeamChanged();
    }

    [DataSourceProperty]
    public MBBindingList<CapturePointVM> AllyControlPoints
    {
        get => _allyControlPoints;
        set
        {
            if (value == _allyControlPoints)
            {
                return;
            }

            _allyControlPoints = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public MBBindingList<CapturePointVM> NeutralControlPoints
    {
        get => _neutralControlPoints;
        set
        {
            if (value == _neutralControlPoints)
            {
                return;
            }

            _neutralControlPoints = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public MBBindingList<CapturePointVM> EnemyControlPoints
    {
        get => _enemyControlPoints;
        set
        {
            if (value == _enemyControlPoints)
            {
                return;
            }

            _enemyControlPoints = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public string AllyTeamColor
    {
        get => _allyTeamColor;
        set
        {
            if (value == _allyTeamColor)
            {
                return;
            }

            _allyTeamColor = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public string AllyTeamColorSecondary
    {
        get => _allyTeamColorSecondary;
        set
        {
            if (value == _allyTeamColorSecondary)
            {
                return;
            }

            _allyTeamColorSecondary = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public string EnemyTeamColor
    {
        get => _enemyTeamColor;
        set
        {
            if (value == _enemyTeamColor)
            {
                return;
            }

            _enemyTeamColor = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public string EnemyTeamColorSecondary
    {
        get => _enemyTeamColorSecondary;
        set
        {
            if (value == _enemyTeamColorSecondary)
            {
                return;
            }

            _enemyTeamColorSecondary = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public int AllyMoraleIncreaseLevel
    {
        get => _allyMoraleIncreaseLevel;
        set
        {
            if (value == _allyMoraleIncreaseLevel)
            {
                return;
            }

            _allyMoraleIncreaseLevel = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public int EnemyMoraleIncreaseLevel
    {
        get => _enemyMoraleIncreaseLevel;
        set
        {
            if (value == _enemyMoraleIncreaseLevel)
            {
                return;
            }

            _enemyMoraleIncreaseLevel = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public int AllyMoralePercentage
    {
        get => _allyMoralePercentage;
        set
        {
            if (value == _allyMoralePercentage)
            {
                return;
            }

            _allyMoralePercentage = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public int EnemyMoralePercentage
    {
        get => _enemyMoralePercentage;
        set
        {
            if (value == _enemyMoralePercentage)
            {
                return;
            }

            _enemyMoralePercentage = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public int AllyMemberCount
    {
        get => _allyMemberCount;
        set
        {
            if (value == _allyMemberCount)
            {
                return;
            }

            _allyMemberCount = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public int EnemyMemberCount
    {
        get => _enemyMemberCount;
        set
        {
            if (value == _enemyMemberCount)
            {
                return;
            }

            _enemyMemberCount = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public PowerLevelComparer PowerLevelComparer
    {
        get => _powerLevelComparer;
        set
        {
            if (value == _powerLevelComparer)
            {
                return;
            }

            _powerLevelComparer = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public bool UsePowerComparer
    {
        get => _usePowerComparer;
        set
        {
            if (value == _usePowerComparer)
            {
                return;
            }

            _usePowerComparer = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public bool UseMoraleComparer
    {
        get => _useMoraleComparer;
        set
        {
            if (value == _useMoraleComparer)
            {
                return;
            }

            _useMoraleComparer = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public bool ShowTacticalInfo
    {
        get => _showTacticalInfo;
        set
        {
            if (value == _showTacticalInfo)
            {
                return;
            }

            _showTacticalInfo = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public bool AreMoralesIndependent
    {
        get => _areMoralesIndependent;
        set
        {
            if (value == _areMoralesIndependent)
            {
                return;
            }

            _areMoralesIndependent = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public bool ShowControlPointStatus
    {
        get => _showControlPointStatus;
        set
        {
            if (value == _showControlPointStatus)
            {
                return;
            }

            _showControlPointStatus = value;
            OnPropertyChangedWithValue(value);
        }
    }

    public override void OnFinalize()
    {
        base.OnFinalize();
        if (_commanderInfo != null)
        {
            _commanderInfo.OnMoraleChangedEvent -= OnUpdateMorale;
            _commanderInfo.OnFlagNumberChangedEvent -= OnNumberOfCapturePointsChanged;
        }

        Mission.Current.OnMissionReset -= OnMissionReset;

        var visualSpawnComponent = Mission.Current.GetMissionBehavior<MultiplayerMissionAgentVisualSpawnComponent>();
        if (visualSpawnComponent != null)
        {
            visualSpawnComponent.OnMyAgentSpawnedFromVisual -= OnMyAgentSpawnedFromVisual;
        }

        if (_siegeClient != null)
        {
            _siegeClient.OnCapturePointRemainingMoraleGainsChangedEvent -= OnCapturePointRemainingMoraleGainsChanged;
        }
    }

    public void Tick(float dt)
    {
        foreach (CapturePointVM allyFlagVm in AllyControlPoints)
        {
            allyFlagVm.Refresh(0.0f, 0.0f, 0.0f);
        }

        foreach (CapturePointVM enemyFlagVm in EnemyControlPoints)
        {
            enemyFlagVm.Refresh(0.0f, 0.0f, 0.0f);
        }

        foreach (CapturePointVM neutralFlagVm in NeutralControlPoints)
        {
            neutralFlagVm.Refresh(0.0f, 0.0f, 0.0f);
        }

        if (_allyTeam == null || !UsePowerComparer)
        {
            return;
        }

        int activeAttackersCount = Mission.Current.AttackerTeam.ActiveAgents.Count;
        int activeDefendersCount = Mission.Current.DefenderTeam.ActiveAgents.Count;
        AllyMemberCount = _allyTeam.Side == BattleSideEnum.Attacker ? activeAttackersCount : activeDefendersCount;
        EnemyMemberCount = _allyTeam.Side == BattleSideEnum.Attacker ? activeDefendersCount : activeAttackersCount;
        int initialAttackerPower = _allyTeam.Side == BattleSideEnum.Attacker ? _attackerTeamInitialMemberCount : _defenderTeamInitialMemberCount;
        Team? allyTeam = _allyTeam;
        int initialDefenderPower = (allyTeam != null
            ? (allyTeam.Side == BattleSideEnum.Attacker ? 1 : 0)
            : 0) != 0 ? _defenderTeamInitialMemberCount : _attackerTeamInitialMemberCount;
        if (initialDefenderPower == 0 && initialAttackerPower == 0)
        {
            PowerLevelComparer.Update(1.0, 1.0, 1.0, 1.0);
        }
        else
        {
            PowerLevelComparer.Update(EnemyMemberCount, AllyMemberCount, initialDefenderPower, initialAttackerPower);
        }
    }

    public void UpdateWarmupDependentFlags(bool isInWarmup)
    {
        UseMoraleComparer = !isInWarmup && _gameMode.IsGameModeTactical && _commanderInfo != null;
        ShowControlPointStatus = !isInWarmup;
        if (isInWarmup || !UseMoraleComparer)
        {
            return;
        }

        RegisterMoraleEvents();
    }

    public void RefreshColors(
        string allyTeamColor,
        string allyTeamColorSecondary,
        string enemyTeamColor,
        string enemyTeamColorSecondary)
    {
        AllyTeamColor = allyTeamColor;
        AllyTeamColorSecondary = allyTeamColorSecondary;
        EnemyTeamColor = enemyTeamColor;
        EnemyTeamColorSecondary = enemyTeamColorSecondary;
        if (!UsePowerComparer)
        {
            return;
        }

        PowerLevelComparer.SetColors(EnemyTeamColor, AllyTeamColor);
    }

    public void OnUpdateMorale(BattleSideEnum side, float morale)
    {
        if (_allyTeam != null && _allyTeam.Side == side)
        {
            AllyMoralePercentage = MathF.Round(MathF.Abs(morale * 100f));
        }
        else
        {
            if (_enemyTeam == null || _enemyTeam.Side != side)
            {
                return;
            }

            EnemyMoralePercentage = MathF.Round(MathF.Abs(morale * 100f));
        }
    }

    public void OnTeamChanged()
    {
        _allyTeam = GameNetwork.MyPeer.GetComponent<MissionPeer>().Team;
        if (_allyTeam == null)
        {
            return;
        }

        if (!GameNetwork.IsMyPeerReady || !ShowTacticalInfo)
        {
            return;
        }

        _enemyTeam = Mission.Current.Teams.FirstOrDefault(t => t.IsEnemyOf(_allyTeam));
        if (_allyTeam.Side == BattleSideEnum.None)
        {
            _allyTeam = Mission.Current.AttackerTeam;
        }
        else
        {
            ResetCapturePointLists();
            InitCapturePoints();
        }
    }

    private void OnMyAgentSpawnedFromVisual()
    {
        ShowTacticalInfo = true;
        OnTeamChanged();
        if (!UsePowerComparer)
        {
            return;
        }

        _attackerTeamInitialMemberCount = _missionScoreboardComponent.Sides[(int)BattleSideEnum.Attacker].Players.Count<MissionPeer>();
        _defenderTeamInitialMemberCount = _missionScoreboardComponent.Sides[(int)BattleSideEnum.Defender].Players.Count<MissionPeer>();
    }

    private void RegisterMoraleEvents()
    {
        if (_areMoraleEventsRegistered)
        {
            return;
        }

        _commanderInfo!.OnMoraleChangedEvent += OnUpdateMorale;
        _commanderInfo.OnFlagNumberChangedEvent += OnNumberOfCapturePointsChanged;
        _commanderInfo.OnCapturePointOwnerChangedEvent += OnCapturePointOwnerChanged;
        AreMoralesIndependent = _commanderInfo.AreMoralesIndependent;
        ResetCapturePointLists();
        InitCapturePoints();
        _areMoraleEventsRegistered = true;
    }

    private void OnMissionReset(object sender, PropertyChangedEventArgs e)
    {
        if (UseMoraleComparer)
        {
            AllyMoralePercentage = 50;
            EnemyMoralePercentage = 50;
        }

        if (!UsePowerComparer)
        {
            return;
        }

        PowerLevelComparer.Update(1.0, 1.0, 1.0, 1.0);
    }

    private void OnCapturePointOwnerChanged(FlagCapturePoint flag, Team newOwnerTeam)
    {
        var flagVm = FindFlagVm(flag);
        if (flagVm == null)
        {
            return;
        }

        RemoveFlagFromLists(flagVm);
        HandleAddNewCapturePoint(flagVm);
        flagVm.OnOwnerChanged(newOwnerTeam);
    }

    private void OnCapturePointRemainingMoraleGainsChanged(int[] flagRemainingMorales)
    {
        foreach (CapturePointVM allyFlagVm in AllyControlPoints)
        {
            int flagIndex = allyFlagVm.Target.FlagIndex;
            if (flagIndex >= 0 && flagRemainingMorales.Length > flagIndex)
            {
                ReflectionHelper.InvokeMethod(allyFlagVm, "OnRemainingMoraleChanged",
                    new object[] { flagRemainingMorales[flagIndex] });
            }
        }

        foreach (CapturePointVM enemyFlagVm in EnemyControlPoints)
        {
            int flagIndex = enemyFlagVm.Target.FlagIndex;
            if (flagIndex >= 0 && flagRemainingMorales.Length > flagIndex)
            {
                ReflectionHelper.InvokeMethod(enemyFlagVm, "OnRemainingMoraleChanged",
                    new object[] { flagRemainingMorales[flagIndex] });
            }
        }

        foreach (CapturePointVM neutralFlagVm in NeutralControlPoints)
        {
            int flagIndex = neutralFlagVm.Target.FlagIndex;
            if (flagIndex >= 0 && flagRemainingMorales.Length > flagIndex)
            {
                ReflectionHelper.InvokeMethod(neutralFlagVm, "OnRemainingMoraleChanged",
                    new object[] { flagRemainingMorales[flagIndex] });
            }
        }
    }

    private void OnNumberOfCapturePointsChanged()
    {
        ResetCapturePointLists();
        InitCapturePoints();
    }

    private void InitCapturePoints()
    {
        if (_commanderInfo == null)
        {
            return;
        }

        NetworkCommunicator myPeer = GameNetwork.MyPeer;
        if (myPeer?.GetComponent<MissionPeer>()?.Team == null)
        {
            return;
        }

        foreach (FlagCapturePoint flag in _commanderInfo.AllCapturePoints.Where(f => !f.IsDeactivated).ToArray())
        {
            HandleAddNewCapturePoint(new CapturePointVM(flag, TargetIconType.Flag_A + flag.FlagIndex));
        }

        RefreshMoraleIncreaseLevels();
    }

    private void HandleAddNewCapturePoint(CapturePointVM flagVm)
    {
        RemoveFlagFromLists(flagVm);
        if (_allyTeam == null)
        {
            return;
        }

        Team? newTeam = _commanderInfo!.GetFlagOwner(flagVm.Target);
        if (newTeam != null && (newTeam.Side == BattleSideEnum.None || newTeam.Side == BattleSideEnum.NumSides))
        {
            newTeam = null;
        }

        flagVm.OnOwnerChanged(newTeam);
        bool isDeactivated = flagVm.Target.IsDeactivated;
        if ((newTeam == null || newTeam.TeamIndex == -1) && !isDeactivated)
        {
            NeutralControlPoints.Insert(MathF.Min(NeutralControlPoints.Count, flagVm.Target.FlagIndex), flagVm);
        }
        else if (_allyTeam == newTeam)
        {
            AllyControlPoints.Insert(MathF.Min(AllyControlPoints.Count, flagVm.Target.FlagIndex), flagVm);
        }
        else if (_allyTeam != newTeam)
        {
            EnemyControlPoints.Insert(MathF.Min(EnemyControlPoints.Count, flagVm.Target.FlagIndex), flagVm);
        }
        else if (newTeam.Side != BattleSideEnum.None)
        {
            Debug.FailedAssert("Incorrect flag team state");
        }

        RefreshMoraleIncreaseLevels();
    }

    private void RefreshMoraleIncreaseLevels()
    {
        AllyMoraleIncreaseLevel = MathF.Max(0, AllyControlPoints.Count - EnemyControlPoints.Count);
        EnemyMoraleIncreaseLevel = MathF.Max(0, EnemyControlPoints.Count - AllyControlPoints.Count);
    }

    private void RemoveFlagFromLists(CapturePointVM capturePoint)
    {
        if (AllyControlPoints.Contains(capturePoint))
        {
            AllyControlPoints.Remove(capturePoint);
        }
        else if (NeutralControlPoints.Contains(capturePoint))
        {
            NeutralControlPoints.Remove(capturePoint);
        }
        else
        {
            if (!EnemyControlPoints.Contains(capturePoint))
            {
                return;
            }

            EnemyControlPoints.Remove(capturePoint);
        }
    }

    private void ResetCapturePointLists()
    {
        AllyControlPoints.Clear();
        NeutralControlPoints.Clear();
        EnemyControlPoints.Clear();
    }

    private CapturePointVM? FindFlagVm(FlagCapturePoint flag)
    {
        return
            AllyControlPoints.SingleOrDefault(f => f.Target == flag)
            ?? EnemyControlPoints.SingleOrDefault(f => f.Target == flag)
            ?? NeutralControlPoints.SingleOrDefault(f => f.Target == flag);
    }
}
