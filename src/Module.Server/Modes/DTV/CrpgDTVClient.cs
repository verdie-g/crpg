﻿using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionRepresentatives;

namespace Crpg.Module.Modes.Dtv;

internal class CrpgDtvClient : MissionMultiplayerGameModeBaseClient
{
/*    private MissionPeer? _missionPeer;
    private TeamDeathmatchMissionRepresentative? _myRepresentative;*/

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
/*        _missionPeer = GameNetwork.MyPeer.GetComponent<MissionPeer>();
        _myRepresentative = GameNetwork.MyPeer.GetComponent<TeamDeathmatchMissionRepresentative>();*/
    }
}

internal class CrpgDtvDataClient : MissionNetwork
{
    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        registerer.Register<CrpgDtvWaveEndMessage>(HandleWaveEnd);
        registerer.Register<CrpgDtvRoundEndMessage>(HandleRoundEnd);
        registerer.Register<CrpgDtvVirginDeathMessage>(HandleVirginDeath);
    }

    private void HandleWaveEnd(CrpgDtvWaveEndMessage message)
    {
        var roundData = message.RoundData;
        InformationManager.DisplayMessage(new InformationMessage($"Wave {roundData.Wave - 1} cleared!",
               new Color(218, 112, 214)));
    }

    private void HandleRoundEnd(CrpgDtvRoundEndMessage message)
    {
        var roundData = message.RoundData;
        InformationManager.DisplayMessage(new InformationMessage
        {
            Information = $"Round {roundData.Round} cleared!",
            Color = new Color(0.48f, 0f, 1f),
            SoundEventPath = "event:/ui/notification/quest_finished",
        });
    }

    private void HandleVirginDeath(CrpgDtvVirginDeathMessage message)
    {
        var roundData = message.RoundData;
        InformationManager.DisplayMessage(new InformationMessage
        {
            Information = $"The Innocent Virgin has been slaughtered!",
            Color = new Color(0.90f, 0.25f, 0.25f),
            SoundEventPath = "event:/ui/notification/death",
        });
    }
}
