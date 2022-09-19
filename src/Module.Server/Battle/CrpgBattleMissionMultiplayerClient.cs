using Crpg.Module.Common;
using Crpg.Module.Common.Models;
using Crpg.Module.Common.Network;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Battle;

internal class CrpgBattleMissionMultiplayerClient : MissionMultiplayerGameModeBaseClient
{
    private CrpgRepresentative? _crpgRepresentative;

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
    }

    public override void AfterStart()
    {
        Mission.Current.SetMissionMode(TaleWorlds.Core.MissionMode.Battle, true);
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
            registerer.Register<CrpgServerMessage>(HandleCrpgServerMessage);
        }
    }

    private void OnPreparationEnded()
    {
    }

    private void OnMyClientSynchronized()
    {
        _crpgRepresentative = GameNetwork.MyPeer.GetComponent<CrpgRepresentative>();
        _crpgRepresentative.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
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

    private void HandleCrpgServerMessage(CrpgServerMessage message)
    {
        InformationManager.DisplayMessage(new InformationMessage(message.Message, new Color(message.Red, message.Green, message.Blue, message.Alpha)));
    }
}
