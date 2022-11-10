using Crpg.Module.Common;
using Crpg.Module.Duel;
using Crpg.Module.Helpers;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.MountAndBlade.ViewModelCollection.Multiplayer.FlagMarker.Targets;
using TaleWorlds.PlatformService;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.GUI;

/// <summary>
/// Copy of MultiplayerMissionMarkerVM. Used so we can adjust all "compass" (alt key) elements.
/// </summary>
internal class CrpgMissionMarkerVm : ViewModel
{
    private readonly Camera _missionCamera;
    private readonly MissionMultiplayerGameModeBaseClient _gameModeClient;
    private readonly Dictionary<MissionPeer, MissionPeerMarkerTargetVM> _missionPeerMarkers;
    private readonly MarkerDistanceComparer _distanceComparer;
    private readonly ICommanderInfo? _commanderInfo;
    private readonly List<PlayerId> _friendIDs;
    private bool _prevEnabledState;
    private bool _fadeOutTimerStarted;
    private float _fadeOutTimer;
    private bool _isEnabled;
    private MBBindingList<MissionFlagMarkerTargetVM> _flagTargets = default!;
    private MBBindingList<MissionPeerMarkerTargetVM> _peerTargets = default!;
    private MBBindingList<MissionSiegeEngineMarkerTargetVM> _siegeEngineTargets = default!;
    private MBBindingList<MissionAlwaysVisibleMarkerTargetVM> _alwaysVisibleTargets = default!;

    public CrpgMissionMarkerVm(Camera missionCamera, MissionMultiplayerGameModeBaseClient gameModeClient)
    {
        _missionCamera = missionCamera;
        _gameModeClient = gameModeClient;
        _missionPeerMarkers = new Dictionary<MissionPeer, MissionPeerMarkerTargetVM>();
        _distanceComparer = new MarkerDistanceComparer();
        _commanderInfo = Mission.Current.GetMissionBehavior<ICommanderInfo>();
        FlagTargets = new MBBindingList<MissionFlagMarkerTargetVM>();
        PeerTargets = new MBBindingList<MissionPeerMarkerTargetVM>();
        SiegeEngineTargets = new MBBindingList<MissionSiegeEngineMarkerTargetVM>();
        AlwaysVisibleTargets = new MBBindingList<MissionAlwaysVisibleMarkerTargetVM>();

        if (_commanderInfo != null)
        {
            _commanderInfo.OnFlagNumberChangedEvent += OnFlagNumberChangedEvent;
            _commanderInfo.OnCapturePointOwnerChangedEvent += OnCapturePointOwnerChangedEvent;
            OnFlagNumberChangedEvent();
            if (_gameModeClient is MissionMultiplayerSiegeClient siegeClient)
            {
                siegeClient.OnCapturePointRemainingMoraleGainsChangedEvent += OnCapturePointRemainingMoraleGainsChanged;
            }
        }

        MissionPeer.OnTeamChanged += OnTeamChanged;
        _friendIDs = new List<PlayerId>();
        IFriendListService[] friendListServices = PlatformServices.Instance.GetFriendListServices();
        foreach (IFriendListService friendListService in friendListServices)
        {
            _friendIDs.AddRange(friendListService.GetAllFriends());
        }
    }

    [DataSourceProperty]
    public MBBindingList<MissionFlagMarkerTargetVM> FlagTargets
    {
        get => _flagTargets;
        set
        {
            if (value != _flagTargets)
            {
                _flagTargets = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public MBBindingList<MissionPeerMarkerTargetVM> PeerTargets
    {
        get => _peerTargets;
        set
        {
            if (value != _peerTargets)
            {
                _peerTargets = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public MBBindingList<MissionSiegeEngineMarkerTargetVM> SiegeEngineTargets
    {
        get => _siegeEngineTargets;
        set
        {
            if (value != _siegeEngineTargets)
            {
                _siegeEngineTargets = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public MBBindingList<MissionAlwaysVisibleMarkerTargetVM> AlwaysVisibleTargets
    {
        get => _alwaysVisibleTargets;
        set
        {
            if (value != _alwaysVisibleTargets)
            {
                _alwaysVisibleTargets = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (value != _isEnabled)
            {
                _isEnabled = value;
                OnPropertyChangedWithValue(value);
                UpdateTargetStates(value);
            }
        }
    }

    public override void OnFinalize()
    {
        base.OnFinalize();
        if (_commanderInfo != null)
        {
            _commanderInfo.OnFlagNumberChangedEvent -= OnFlagNumberChangedEvent;
            _commanderInfo.OnCapturePointOwnerChangedEvent -= OnCapturePointOwnerChangedEvent;
            if (_gameModeClient is MissionMultiplayerSiegeClient siegeClient)
            {
                siegeClient.OnCapturePointRemainingMoraleGainsChangedEvent -= OnCapturePointRemainingMoraleGainsChanged;
            }
        }

        MissionPeer.OnTeamChanged -= OnTeamChanged;
    }

    public void Tick(float dt)
    {
        OnRefreshPeerMarkers();
        UpdateAlwaysVisibleTargetScreenPosition();
        if (IsEnabled)
        {
            UpdateTargetScreenPositions();
            _fadeOutTimerStarted = false;
            _fadeOutTimer = 0f;
            _prevEnabledState = IsEnabled;
        }
        else
        {
            if (_prevEnabledState)
            {
                _fadeOutTimerStarted = true;
            }

            if (_fadeOutTimerStarted)
            {
                _fadeOutTimer += dt;
            }

            if (_fadeOutTimer < 2f)
            {
                UpdateTargetScreenPositions();
            }
            else
            {
                _fadeOutTimerStarted = false;
            }
        }

        _prevEnabledState = IsEnabled;
    }

    private void OnRemoveAlwaysVisibleMarker(MissionAlwaysVisibleMarkerTargetVM marker)
    {
        AlwaysVisibleTargets.Remove(marker);
    }

    private void OnCapturePointRemainingMoraleGainsChanged(int[] remainingMoraleGainsArr)
    {
        foreach (MissionFlagMarkerTargetVM flagTarget in FlagTargets)
        {
            int flagIndex = flagTarget.TargetFlag.FlagIndex;
            if (flagIndex >= 0 && flagIndex < remainingMoraleGainsArr.Length)
            {
                flagTarget.OnRemainingMoraleChanged(remainingMoraleGainsArr[flagIndex]);
            }
        }

        Debug.Print("OnCapturePointRemainingMoraleGainsChanged: " + remainingMoraleGainsArr.Length);
    }

    private void OnTeamChanged(NetworkCommunicator peer, Team previousTeam, Team newTeam)
    {
        if (_commanderInfo != null)
        {
            OnFlagNumberChangedEvent();
        }

        if (!peer.IsMine)
        {
            return;
        }

        SiegeEngineTargets.Clear();
        foreach (GameEntity item in Mission.Current.GetActiveEntitiesWithScriptComponentOfType<SiegeWeapon>())
        {
            SiegeWeapon firstScriptOfType = item.GetFirstScriptOfType<SiegeWeapon>();
            if (newTeam.Side == firstScriptOfType.Side)
            {
                SiegeEngineTargets.Add(new MissionSiegeEngineMarkerTargetVM(firstScriptOfType));
            }
        }
    }

    private void UpdateTargetScreenPositions()
    {
        PeerTargets.ApplyActionOnAllItems(pt => pt.UpdateScreenPosition(_missionCamera));
        FlagTargets.ApplyActionOnAllItems(ft => ft.UpdateScreenPosition(_missionCamera));
        SiegeEngineTargets.ApplyActionOnAllItems(st => st.UpdateScreenPosition(_missionCamera));
        PeerTargets.Sort(_distanceComparer);
        FlagTargets.Sort(_distanceComparer);
        SiegeEngineTargets.Sort(_distanceComparer);
    }

    private void UpdateAlwaysVisibleTargetScreenPosition()
    {
        foreach (MissionAlwaysVisibleMarkerTargetVM alwaysVisibleTarget in AlwaysVisibleTargets)
        {
            alwaysVisibleTarget.UpdateScreenPosition(_missionCamera);
        }
    }

    private void OnFlagNumberChangedEvent()
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

        foreach (FlagCapturePoint flag in _commanderInfo.AllCapturePoints)
        {
            if (flag.IsDeactivated)
            {
                continue;
            }

            MissionFlagMarkerTargetVM missionFlagMarkerTargetVm = new(flag);
            FlagTargets.Add(missionFlagMarkerTargetVm);
            missionFlagMarkerTargetVm.OnOwnerChanged(_commanderInfo.GetFlagOwner(flag));
        }
    }

    private void ResetCapturePointLists()
    {
        FlagTargets.Clear();
    }

    private void OnCapturePointOwnerChangedEvent(FlagCapturePoint flag, Team team)
    {
        foreach (MissionFlagMarkerTargetVM flagTarget in FlagTargets)
        {
            if (flagTarget.TargetFlag == flag)
            {
                flagTarget.OnOwnerChanged(team);
            }
        }
    }

    private void OnRefreshPeerMarkers()
    {
        if (GameNetwork.MyPeer == null)
        {
            return;
        }

        BattleSideEnum battleSideEnum = GameNetwork.MyPeer.ControlledAgent?.Team.Side ?? BattleSideEnum.None;
        List<MissionPeerMarkerTargetVM> markerList = PeerTargets.ToList();
        bool isDuel = _gameModeClient is CrpgDuelMissionMultiplayerClient;
        foreach (MissionPeer missionPeer in VirtualPlayer.Peers<MissionPeer>())
        {
            if (isDuel && battleSideEnum == BattleSideEnum.Defender)
            {
                break;
            }

            if (missionPeer?.Team == null
                || missionPeer.IsMine
                || (!isDuel && missionPeer.Team.Side != battleSideEnum) // If it's not duel gamemode
                || (isDuel && missionPeer.Team.Side != BattleSideEnum.Defender)) // If its duel gamemode show only players which are dueling
            {
                continue;
            }

            var currentMarker = PeerTargets.FirstOrDefault(t => t.TargetPeer?.Peer.Id.Equals(missionPeer.Peer.Id) ?? false);
            if (currentMarker != null)
            {
                bool hasDeathIconMarker = AlwaysVisibleTargets
                    .Any(t => t.TargetPeer.Peer.Id.Equals(currentMarker.TargetPeer.Peer.Id));
                if (BannerlordConfig.EnableDeathIcon && !missionPeer.IsControlledAgentActive)
                {
                    if (!hasDeathIconMarker && currentMarker.TargetPeer?.ControlledAgent != null)
                    {
                        MissionAlwaysVisibleMarkerTargetVM missionAlwaysVisibleMarkerTargetVm = new(
                            currentMarker.TargetPeer, currentMarker.WorldPosition, OnRemoveAlwaysVisibleMarker);
                        missionAlwaysVisibleMarkerTargetVm.UpdateScreenPosition(_missionCamera);
                        AlwaysVisibleTargets.Add(missionAlwaysVisibleMarkerTargetVm);
                    }

                    continue;
                }
            }

            // Create new teammate markers and add them to list + set color
            if (!_missionPeerMarkers.ContainsKey(missionPeer))
            {
                bool missionPeerIsFriend = _friendIDs.Contains(missionPeer.Peer.Id);
                MissionPeerMarkerTargetVM missionPeerMarkerTargetVm = new(missionPeer, missionPeerIsFriend);
                PeerTargets.Add(missionPeerMarkerTargetVm);
                _missionPeerMarkers.Add(missionPeer, missionPeerMarkerTargetVm);
                OverridePeerColor(missionPeerMarkerTargetVm, missionPeer, missionPeerIsFriend);
            }
            else
            {
                // Remove all teammates from the markerList
                markerList.Remove(_missionPeerMarkers[missionPeer]);
            }
        }

        // markerList contains only old or dead teammates at this point.
        // Remove all MissionPeerMarkerTargetVM's which remain in the markerList from the final visible icons.
        foreach (MissionPeerMarkerTargetVM item in markerList)
        {
            if (item != null)
            {
                PeerTargets.Remove(item);
                _missionPeerMarkers.Remove(item.TargetPeer);
            }
        }
    }

    private void UpdateTargetStates(bool state)
    {
        PeerTargets.ApplyActionOnAllItems(pt => pt.IsEnabled = state);
        FlagTargets.ApplyActionOnAllItems(ft => ft.IsEnabled = state);
        SiegeEngineTargets.ApplyActionOnAllItems(st => st.IsEnabled = state);
    }

    /// <summary>
    /// Changed to override the native alt key color.
    /// In cRPG we use red for clanmates and white for everyone else.
    /// Recently played and the ingame clan feature is ignored.
    /// </summary>
    private void OverridePeerColor(MissionPeerMarkerTargetVM missionPeerMarkerTargetVm, MissionPeer missionPeer, bool missionPeerIsFriend)
    {
        // Color format #RRGGBBAA
        const string defaultColor = "#FFFFFFFF"; // white
        const string clanmateColor = "#FF0000FF"; // red
        const string friendColor = "#FFFF00FF"; // yellow
        uint color1 = Color.ConvertStringToColor(defaultColor).ToUnsignedInteger(); // white
        uint color2 = Color.ConvertStringToColor(defaultColor).ToUnsignedInteger(); // white
        if (GameNetwork.MyPeer != null)
        {
            CrpgPeer myCrpgPeer = GameNetwork.MyPeer.GetComponent<CrpgPeer>();
            if (myCrpgPeer?.Clan != null)
            {
                CrpgPeer crpgPeer = missionPeer.GetNetworkPeer().GetComponent<CrpgPeer>();
                if (crpgPeer.Clan != null && myCrpgPeer.Clan.Id == crpgPeer.Clan.Id)
                {
                    color2 = Color.ConvertStringToColor(clanmateColor).ToUnsignedInteger();
                }
            }
            else if (missionPeerIsFriend)
            {
                color2 = Color.ConvertStringToColor(friendColor).ToUnsignedInteger();
            }
        }

        ReflectionHelper.InvokeMethod(missionPeerMarkerTargetVm, "RefreshColor", new object[] { color1, color2 });
    }

    private class MarkerDistanceComparer : IComparer<MissionMarkerTargetVM>
    {
        public int Compare(MissionMarkerTargetVM x, MissionMarkerTargetVM y)
        {
            return y.Distance.CompareTo(x.Distance);
        }
    }
}
