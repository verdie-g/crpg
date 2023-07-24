using Crpg.Module.Helpers;
using Crpg.Module.Modes.Siege;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.MountAndBlade.ViewModelCollection.Multiplayer.HUDExtensions;

namespace Crpg.Module.GUI.Hud;

internal class FlagsHudVm : ViewModel
{
    private readonly CrpgSiegeClient? _siegeClient;
    private readonly ICommanderInfo _commanderInfo;
    private Team? _allyTeam;
    private MBBindingList<CapturePointVM> _allyControlPoints = default!;
    private MBBindingList<CapturePointVM> _neutralControlPoints = default!;
    private MBBindingList<CapturePointVM> _enemyControlPoints = default!;

    public FlagsHudVm()
    {
        AllyControlPoints = new MBBindingList<CapturePointVM>();
        NeutralControlPoints = new MBBindingList<CapturePointVM>();
        EnemyControlPoints = new MBBindingList<CapturePointVM>();

        MissionPeer.OnTeamChanged += OnTeamChanged;

        _commanderInfo = Mission.Current.GetMissionBehavior<ICommanderInfo>();
        _commanderInfo.OnFlagNumberChangedEvent += OnNumberOfCapturePointsChanged;
        _commanderInfo.OnCapturePointOwnerChangedEvent += OnCapturePointOwnerChanged;

        _siegeClient = Mission.Current.GetMissionBehavior<CrpgSiegeClient>();
        if (_siegeClient != null)
        {
            _siegeClient.OnCapturePointRemainingMoraleGainsChangedEvent += OnCapturePointRemainingMoraleGainsChanged;
        }

        ResetCapturePointLists();
        InitCapturePoints();
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

    public override void OnFinalize()
    {
        base.OnFinalize();

        MissionPeer.OnTeamChanged -= OnTeamChanged;

        _commanderInfo.OnCapturePointOwnerChangedEvent -= OnCapturePointOwnerChanged;
        _commanderInfo.OnFlagNumberChangedEvent -= OnNumberOfCapturePointsChanged;

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
    }

    private void OnTeamChanged(NetworkCommunicator peer, Team previousTeam, Team newTeam)
    {
        OnTeamChanged();
    }

    private void OnTeamChanged()
    {
        _allyTeam = GameNetwork.MyPeer.GetComponent<MissionPeer>()?.Team;
        if (_allyTeam == null)
        {
            return;
        }

        if (!GameNetwork.IsMyPeerReady)
        {
            return;
        }

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
        NetworkCommunicator myPeer = GameNetwork.MyPeer;
        if (myPeer?.GetComponent<MissionPeer>()?.Team == null)
        {
            return;
        }

        foreach (FlagCapturePoint flag in _commanderInfo.AllCapturePoints.Where(f => f.GameEntity != null && !f.IsDeactivated).ToArray())
        {
            HandleAddNewCapturePoint(new CapturePointVM(flag, TargetIconType.Flag_A + flag.FlagIndex));
        }
    }

    private void HandleAddNewCapturePoint(CapturePointVM flagVm)
    {
        RemoveFlagFromLists(flagVm);
        if (_allyTeam == null)
        {
            return;
        }

        Team? newTeam = _commanderInfo.GetFlagOwner(flagVm.Target);
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
