using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
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
        registerer.Register<CrpgDtvSetTimerMessage>(HandleSetTimer);
        registerer.Register<CrpgDtvRoundStartMessage>(HandleRoundStart);
        registerer.Register<CrpgDtvWaveStartMessage>(HandleWaveStart);
        registerer.Register<CrpgDtvGameEnd>(HandleViscountDeath);
    }

    private void HandleSetTimer(CrpgDtvSetTimerMessage message)
    {
        TimerComponent.StartTimerAsClient(message.StartTime, message.Duration);
    }

    private void HandleRoundStart(CrpgDtvRoundStartMessage message)
    {
        TextObject textObject = new("{=bbg3UpPX}Round {ROUND} starting...",
            new Dictionary<string, object> { ["ROUND"] = message.Round + 1 });
        InformationManager.DisplayMessage(new InformationMessage
        {
            Information = textObject.ToString(),
            Color = new Color(0.48f, 0f, 1f),
            SoundEventPath = message.Round == 0 ? null : "event:/ui/notification/quest_finished",
        });
    }

    private void HandleWaveStart(CrpgDtvWaveStartMessage message)
    {
        TextObject textObject = new("{=1y04FNHB}Wave {WAVE} started!",
            new Dictionary<string, object> { ["WAVE"] = message.Wave + 1 });
        InformationManager.DisplayMessage(new InformationMessage
        {
            Information = textObject.ToString(),
            Color = new Color(218, 112, 214),
            SoundEventPath = message.Wave == 0 ? null : "event:/ui/notification/quest_update",
        });
    }

    private void HandleViscountDeath(CrpgDtvGameEnd message)
    {
        InformationManager.DisplayMessage(new InformationMessage
        {
            Information = message.ViscountDead
                ? new TextObject("{=4HrC30kl}The Viscount has been slaughtered!").ToString()
                : new TextObject("{=tdfOMWmf}The defenders have been slaughtered!").ToString(),
            Color = new Color(0.90f, 0.25f, 0.25f),
            SoundEventPath = "event:/ui/notification/death",
        });
    }
}
