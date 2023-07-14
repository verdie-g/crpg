using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Modes.Dtv;

internal class CrpgDtvClient : MissionMultiplayerGameModeBaseClient
{
    public override bool IsGameModeUsingGold => false;
    public override bool IsGameModeTactical => false;
    public override bool IsGameModeUsingRoundCountdown => true;
    public override MissionLobbyComponent.MultiplayerGameType GameType =>
        MissionLobbyComponent.MultiplayerGameType.Battle;
    public override bool IsGameModeUsingCasualGold => false;

    public override void OnGoldAmountChangedForRepresentative(MissionRepresentativeBase representative, int goldAmount)
    {
    }

    public override int GetGoldAmount()
    {
        return 0;
    }

    public override void AfterStart()
    {
        Mission.Current.SetMissionMode(MissionMode.Battle, true);
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        registerer.Register<CrpgDtvWaveEndMessage>(HandleWaveEnd);
        registerer.Register<CrpgDtvRoundEndMessage>(HandleRoundEnd);
        registerer.Register<CrpgDtvViscountDeathMessage>(HandleViscountDeath);
    }

    private void HandleWaveEnd(CrpgDtvWaveEndMessage message)
    {
        InformationManager.DisplayMessage(new InformationMessage($"Wave {message.Wave - 1} cleared!",
               new Color(218, 112, 214)));
    }

    private void HandleRoundEnd(CrpgDtvRoundEndMessage message)
    {
        InformationManager.DisplayMessage(new InformationMessage
        {
            Information = $"Round {message.Round} cleared!",
            Color = new Color(0.48f, 0f, 1f),
            SoundEventPath = "event:/ui/notification/quest_finished",
        });
    }

    private void HandleViscountDeath(CrpgDtvViscountDeathMessage message)
    {
        InformationManager.DisplayMessage(new InformationMessage
        {
            Information = $"The Viscount has been slaughtered!",
            Color = new Color(0.90f, 0.25f, 0.25f),
            SoundEventPath = "event:/ui/notification/death",
        });
    }
}
