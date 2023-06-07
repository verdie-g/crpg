using Crpg.Module.Api.Models;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Rewards;

internal class CrpgDTVDataClient : MissionNetwork
{
    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        registerer.Register<CrpgDTVRoundEndMessage>(HandleDTVRoundEnd);
    }

    private void HandleDTVRoundEnd(CrpgDTVRoundEndMessage message)
    {
        var roundData = message.RoundData;
        InformationManager.DisplayMessage(new InformationMessage($"Wave cleared!",
               new Color(218, 112, 214)));
        InformationManager.DisplayMessage(new InformationMessage($"Round: {roundData.Round} Wave: {roundData.Wave}",
               new Color(218, 112, 214)));
    }
}
