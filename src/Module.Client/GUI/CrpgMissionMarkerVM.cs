using Crpg.Module.Common;
using Crpg.Module.Duel;
using Crpg.Module.Helpers;
using Crpg.Module.Siege;
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
    // Color format #RRGGBBAA
    private static readonly uint DefaultColor = Color.ConvertStringToColor("#FFFFFFFF").ToUnsignedInteger(); // White
    private static readonly uint ClanMateColor = Color.ConvertStringToColor("#00FFFFFF").ToUnsignedInteger(); // Aqua
    private static readonly uint FriendColor = Color.ConvertStringToColor("#FFFF00FF").ToUnsignedInteger(); // Yellow

    private readonly Camera _missionCamera;
    private readonly MissionMultiplayerGameModeBaseClient _gameModeClient;
    private readonly Dictionary<MissionPeer, MissionPeerMarkerTargetVM> _missionPeerMarkers;
    private readonly MarkerDistanceComparer _distanceComparer;
    private readonly ICommanderInfo? _commanderInfo;
    private readonly HashSet<PlayerId> _friendIds;
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
            if (_gameModeClient is CrpgSiegeMissionMultiplayerClient siegeClient)
            {
                siegeClient.OnCapturePointRemainingMoraleGainsChangedEvent += OnCapturePointRemainingMoraleGainsChanged;
            }
        }

        MissionPeer.OnTeamChanged += OnTeamChanged;

        _friendIds = new HashSet<PlayerId>();
        foreach (var friendId in FriendListService.GetAllFriendsInAllPlatforms())
        {
            _friendIds.Add(friendId);
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

    private void OnCapturePointRemainingMoraleGainsChanged(int[] remainingMoraleGains)
    {
        foreach (MissionFlagMarkerTargetVM flagTarget in FlagTargets)
        {
            int flagIndex = flagTarget.TargetFlag.FlagIndex;
            if (flagIndex >= 0 && flagIndex < remainingMoraleGains.Length)
            {
                flagTarget.OnRemainingMoraleChanged(remainingMoraleGains[flagIndex]);
            }
        }

        Debug.Print("OnCapturePointRemainingMoraleGainsChanged: " + remainingMoraleGains.Length);
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

        bool isDuel = _gameModeClient is CrpgDuelMissionMultiplayerClient;
        BattleSideEnum myPeerSide = GameNetwork.MyPeer.ControlledAgent?.Team.Side ?? BattleSideEnum.None;
        if (isDuel && myPeerSide == BattleSideEnum.Defender)
        {
            return;
        }

        List<MissionPeerMarkerTargetVM> peerTargetsToRemove = PeerTargets.ToList();
        foreach (MissionPeer missionPeer in VirtualPlayer.Peers<MissionPeer>())
        {
            if (missionPeer?.Team == null
                || missionPeer.IsMine
                || (!isDuel && missionPeer.Team.Side != myPeerSide) // If it's not duel gamemode
                || (isDuel && missionPeer.Team.Side != BattleSideEnum.Defender)) // If its duel gamemode show only players which are dueling
            {
                continue;
            }

            if (BannerlordConfig.EnableDeathIcon && !missionPeer.IsControlledAgentActive)
            {
                var peerTarget = PeerTargets.FirstOrDefault(t => t.TargetPeer.Peer.Id.Equals(missionPeer.Peer.Id));
                if (peerTarget != null)
                {
                    bool hasDeathIconMarker = AlwaysVisibleTargets.Any(t =>
                        t.TargetPeer.Peer.Id.Equals(peerTarget.TargetPeer.Peer.Id));
                    if (!hasDeathIconMarker && peerTarget.TargetPeer?.ControlledAgent != null)
                    {
                        MissionAlwaysVisibleMarkerTargetVM missionAlwaysVisibleMarkerTargetVm = new(
                            peerTarget.TargetPeer, peerTarget.WorldPosition, OnRemoveAlwaysVisibleMarker);
                        missionAlwaysVisibleMarkerTargetVm.UpdateScreenPosition(_missionCamera);
                        AlwaysVisibleTargets.Add(missionAlwaysVisibleMarkerTargetVm);
                    }

                    continue;
                }
            }

            // Create new teammate markers and add them to list + set color
            if (!_missionPeerMarkers.ContainsKey(missionPeer))
            {
                bool missionPeerIsFriend = _friendIds.Contains(missionPeer.Peer.Id);
                MissionPeerMarkerTargetVM missionPeerMarkerTargetVm = new(missionPeer, missionPeerIsFriend);
                PeerTargets.Add(missionPeerMarkerTargetVm);
                _missionPeerMarkers.Add(missionPeer, missionPeerMarkerTargetVm);
                OverridePeerColor(missionPeerMarkerTargetVm, missionPeer, missionPeerIsFriend);
            }
            else
            {
                // Remove all teammates from the markerList
                peerTargetsToRemove.Remove(_missionPeerMarkers[missionPeer]);
            }
        }

        // markerList contains only old or dead teammates at this point.
        // Remove all MissionPeerMarkerTargetVM's which remain in the markerList from the final visible icons.
        foreach (MissionPeerMarkerTargetVM peerTarget in peerTargetsToRemove)
        {
            if (peerTarget != null)
            {
                PeerTargets.Remove(peerTarget);
                _missionPeerMarkers.Remove(peerTarget.TargetPeer);
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
        uint color1 = DefaultColor;
        uint color2 = DefaultColor;
        if (GameNetwork.MyPeer != null)
        {
            CrpgPeer myCrpgPeer = GameNetwork.MyPeer.GetComponent<CrpgPeer>();
            CrpgPeer? crpgPeer = missionPeer.GetNetworkPeer().GetComponent<CrpgPeer>() ?? null;
            if (myCrpgPeer?.Clan != null && crpgPeer?.Clan != null && crpgPeer.Clan.Id == myCrpgPeer.Clan.Id)
            {
                color2 = ClanMateColor;
            }
            else if (missionPeerIsFriend)
            {
                color2 = FriendColor;
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
