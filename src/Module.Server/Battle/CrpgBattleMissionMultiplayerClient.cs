using Crpg.Module.Common;
using Crpg.Module.Common.Models;
using Crpg.Module.Common.Network;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Battle;

internal class CrpgBattleMissionMultiplayerClient : MissionMultiplayerGameModeBaseClient, ICommanderInfo
{
    internal const int FlagsRemovalTime = 120;

    private FlagCapturePoint[] _flags = Array.Empty<FlagCapturePoint>();
    private Team?[] _flagOwners = Array.Empty<Team>();
    private bool _notifiedForFlagRemoval;
    private float _remainingTimeForBellSoundToStop = float.MinValue;
    private SoundEvent? _bellSoundEvent;
    private CrpgRepresentative? _crpgRepresentative;

    public event Action<BattleSideEnum, float>? OnMoraleChangedEvent;
    public event Action? OnFlagNumberChangedEvent;
    public event Action<FlagCapturePoint, Team?>? OnCapturePointOwnerChangedEvent;

    public override bool IsGameModeUsingGold => false;
    public override bool IsGameModeTactical => true;
    public override bool IsGameModeUsingRoundCountdown => true;
    public override MissionLobbyComponent.MultiplayerGameType GameType => MissionLobbyComponent.MultiplayerGameType.Battle;
    public override bool IsGameModeUsingCasualGold => false;
    public IEnumerable<FlagCapturePoint> AllCapturePoints => _flags;
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
      _crpgRepresentative?.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
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
        if (_remainingTimeForBellSoundToStop > 0.0)
        {
            _remainingTimeForBellSoundToStop -= dt;
        }

        if (_bellSoundEvent == null
            || (_remainingTimeForBellSoundToStop > 0.0
            && MissionLobbyComponent.CurrentMultiplayerState == MissionLobbyComponent.MultiplayerGameState.Playing))
        {
            return;
        }

        _remainingTimeForBellSoundToStop = float.MinValue;
        _bellSoundEvent.Stop();
        _bellSoundEvent = null;
    }

    public override void AfterStart()
    {
        Mission.Current.SetMissionMode(MissionMode.Battle, true);
    }

    public override void OnClearScene()
    {
        _notifiedForFlagRemoval = false;
        ResetFlags();

        if (_bellSoundEvent != null)
        {
            _remainingTimeForBellSoundToStop = float.MinValue;
            _bellSoundEvent.Stop();
            _bellSoundEvent = null;
        }
    }

    public void ChangeMorale(float morale)
    {
        for (BattleSideEnum side = BattleSideEnum.Defender; side < BattleSideEnum.NumSides; side += 1)
        {
            float num = (morale + 1.0f) / 2.0f;
            if (side == BattleSideEnum.Defender)
            {
                OnMoraleChangedEvent?.Invoke(BattleSideEnum.Defender, 1f - num);
            }
            else if (side == BattleSideEnum.Attacker)
            {
                OnMoraleChangedEvent?.Invoke(BattleSideEnum.Attacker, num);
            }
        }

        BattleSideEnum mySide = _crpgRepresentative?.MissionPeer.Team.Side ?? BattleSideEnum.None;
        if (mySide == BattleSideEnum.None)
        {
            return;
        }

        float absMorale = MathF.Abs(morale);
        if (_remainingTimeForBellSoundToStop < 0.0)
        {
            _remainingTimeForBellSoundToStop = absMorale < 0.6 || absMorale >= 1.0
                ? float.MinValue
                : float.MaxValue;
            if (_remainingTimeForBellSoundToStop <= 0.0)
            {
                return;
            }

            _bellSoundEvent =
                (mySide == BattleSideEnum.Defender && morale >= 0.6f) ||
                (mySide == BattleSideEnum.Attacker && morale <= -0.6f)
                    ? SoundEvent.CreateEventFromString("event:/multiplayer/warning_bells_defender", Mission.Scene)
                    : SoundEvent.CreateEventFromString("event:/multiplayer/warning_bells_attacker", Mission.Scene);
            MatrixFrame flagGlobalFrame = _flags
                .Where(flag => !flag.IsDeactivated)
                .GetRandomElementInefficiently()
                .GameEntity.GetGlobalFrame();
            _bellSoundEvent.PlayInPosition(flagGlobalFrame.origin + flagGlobalFrame.rotation.u * 3f);
        }
        else
        {
            if (absMorale < 1.0 && absMorale >= 0.6)
            {
                return;
            }

            _remainingTimeForBellSoundToStop = float.MinValue;
        }
    }

    public void CaptureFlag(FlagCapturePoint flag, Team owner)
    {
        _flagOwners[flag.FlagIndex] = owner;
        OnCapturePointOwnerChangedEvent?.Invoke(flag, owner);

        var myTeam = _crpgRepresentative?.MissionPeer.Team;
        if (myTeam == null)
        {
            return;
        }

        MatrixFrame cameraFrame = Mission.Current.GetCameraFrame();
        Vec3 position = cameraFrame.origin + cameraFrame.rotation.u;
        string eventStr = myTeam == owner
            ? "event:/alerts/report/flag_captured"
            : "event:/alerts/report/flag_lost";
        MBSoundEvent.PlaySound(SoundEvent.GetEventIdFromString(eventStr), position);
    }

    public void ChangeNumberOfFlags()
    {
        OnFlagNumberChangedEvent?.Invoke();
    }

    public Team? GetFlagOwner(FlagCapturePoint flag)
    {
        return _flagOwners[flag.FlagIndex];
    }

    protected override void AddRemoveMessageHandlers(
        GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        if (GameNetwork.IsClientOrReplay)
        {
            registerer.Register<UpdateCrpgUser>(HandleUpdateCrpgUser);
            registerer.Register<CrpgRewardUser>(HandleRewardUser);
            registerer.Register<CrpgRewardError>(HandleRewardError);
            registerer.Register<CrpgNotification>(HandleNotification);
            registerer.Register<FlagDominationMoraleChangeMessage>(OnMoraleChange);
            registerer.Register<FlagDominationCapturePointMessage>(OnCapturePoint);
            registerer.Register<FlagDominationFlagsRemovedMessage>(OnFlagsRemoved);
        }
    }

    protected override int GetWarningTimer()
    {
        if (!IsRoundInProgress)
        {
            return 0;
        }

        float timerStart = MultiplayerOptions.OptionType.RoundTimeLimit.GetIntValue() - FlagsRemovalTime;
        float timerEnd = timerStart + 30f;
        if (RoundComponent.RemainingRoundTime < timerStart
            || RoundComponent.RemainingRoundTime > timerEnd)
        {
            return 0;
        }

        int warningTimer = MathF.Ceiling(30.0f - timerEnd - RoundComponent.RemainingRoundTime);
        if (!_notifiedForFlagRemoval)
        {
            _notifiedForFlagRemoval = true;
            NotificationsComponent.FlagsWillBeRemovedInXSeconds(30);
        }

        return warningTimer;
    }

    private void OnPreparationEnded()
    {
        ResetFlags();
        OnFlagNumberChangedEvent?.Invoke();
        foreach (var flag in _flags)
        {
            OnCapturePointOwnerChangedEvent?.Invoke(flag, null);
        }
    }

    private void OnMyClientSynchronized()
    {
        _crpgRepresentative = GameNetwork.MyPeer.GetComponent<CrpgRepresentative>();
        _crpgRepresentative.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
    }

    private void OnMoraleChange(FlagDominationMoraleChangeMessage message)
    {
        ChangeMorale(message.Morale);
    }

    private void OnCapturePoint(FlagDominationCapturePointMessage message)
    {
        var capturedFlag = _flags.FirstOrDefault(flag => flag.FlagIndex == message.FlagIndex);
        if (capturedFlag == null)
        {
            return;
        }

        CaptureFlag(capturedFlag, message.OwnerTeam);
    }

    private void OnFlagsRemoved(FlagDominationFlagsRemovedMessage message)
    {
        ChangeNumberOfFlags();
    }

    private void ResetFlags()
    {
        _flags = Mission.Current.MissionObjects.FindAllWithType<FlagCapturePoint>().ToArray();
        _flagOwners = new Team[_flags.Length];
    }

    private void HandleUpdateCrpgUser(UpdateCrpgUser message)
    {
        // Hack to workaround not being able to spawn custom character.
        CrpgAgentStatCalculateModel.MyUser = message.User;

        // Print a welcome message to new players. For convenience, new player are considered character of generation
        // 0 and small level. This doesn't handle the case of second characters for the same user but it's good enough.
        if (RoundComponent.RoundCount > 1 || RoundComponent.CurrentRoundState == MultiplayerRoundState.Ending)
        {
            return;
        }

        var user = message.User;
        if (user.Character.Generation == 0 && user.Character.Level < 4)
        {
            InformationManager.DisplayMessage(new InformationMessage(
                "Welcome to cRPG! Gain experience and gold in battles and upgrade your character on the website https://c-rpg.eu"));
        }
    }

    private void HandleRewardUser(CrpgRewardUser message)
    {
        var reward = message.Reward;
        if (reward.Experience != 0)
        {
            InformationManager.DisplayMessage(new InformationMessage($"Gained {reward.Experience} experience.",
                new Color(218, 112, 214)));
        }

        if (reward.Gold != 0)
        {
            InformationManager.DisplayMessage(new InformationMessage($"Gained {reward.Gold} gold.",
                new Color(65, 105, 225)));
        }

        if (message.RepairCost != 0)
        {
            InformationManager.DisplayMessage(new InformationMessage($"Lost {message.RepairCost} gold for upkeep.",
                new Color(0.74f, 0.28f, 0.01f)));
        }

        if (message.SoldItemIds.Count != 0)
        {
            var soldItemNames = message.SoldItemIds
                .Select(i => MBObjectManager.Instance.GetObject<ItemObject>(i)?.Value)
                .Where(i => i != null);
            string soldItemNamesStr = string.Join(", ", soldItemNames);
            string s = message.SoldItemIds.Count > 1 ? "s" : string.Empty;
            InformationManager.DisplayMessage(new InformationMessage($"Sold item{s} {soldItemNamesStr} to pay for upkeep.",
                new Color(0.74f, 0.28f, 0.01f)));
        }

        if (reward.LevelUp)
        {
            InformationManager.DisplayMessage(new InformationMessage
            {
                Information = "Level up!",
                Color = new Color(128, 0, 128),
                SoundEventPath = "event:/ui/notification/levelup",
            });
        }
    }

    private void HandleRewardError(CrpgRewardError message)
    {
        InformationManager.DisplayMessage(new InformationMessage("Could not join cRPG main server. Your reward was lost.", new Color(0.75f, 0.01f, 0.01f)));
    }

    private void HandleNotification(CrpgNotification notification)
    {
        TextObject msg = notification.IsMessageTextId ? GameTexts.FindText(notification.Message) : new TextObject(notification.Message);
        MBInformationManager.AddQuickInformation(msg, 0, null, notification.SoundEvent);
    }

}
