using Crpg.Module.Api.Models;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Modes.DTV;

internal class CrpgDTVDataClient : MissionNetwork
{
    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        registerer.Register<CrpgDTVWaveEndMessage>(HandleWaveEnd);
        registerer.Register<CrpgDTVRoundEndMessage>(HandleRoundEnd);
        registerer.Register<CrpgDTVVirginDeathMessage>(HandleVirginDeath);
    }

    private void HandleWaveEnd(CrpgDTVWaveEndMessage message)
    {
        var roundData = message.RoundData;
        InformationManager.DisplayMessage(new InformationMessage($"Wave {roundData.Wave - 1} cleared!",
               new Color(218, 112, 214)));
    }

    private void HandleRoundEnd(CrpgDTVRoundEndMessage message)
    {
        var roundData = message.RoundData;
        InformationManager.DisplayMessage(new InformationMessage
        {
            Information = $"Round {roundData.Round - 1} cleared!",
            Color = new Color(0.48f, 0f, 1f),
            SoundEventPath = "event:/ui/notification/quest_finished",
        });
    }

    private void HandleVirginDeath(CrpgDTVVirginDeathMessage message)
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
