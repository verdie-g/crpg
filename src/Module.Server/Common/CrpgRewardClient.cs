using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Common;

internal class CrpgRewardClient : MissionNetwork
{
    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        registerer.Register<CrpgRewardUser>(HandleRewardUser);
        registerer.Register<CrpgRewardError>(HandleRewardError);
    }

    private void HandleRewardUser(CrpgRewardUser message)
    {
        var reward = message.Reward;
        if (reward.Experience != 0)
        {
            InformationManager.DisplayMessage(new InformationMessage($"Gained {reward.Experience} experience.",
                new Color(218, 112, 214)));
        }

        int gain = reward.Gold - message.RepairCost;
        if (gain != 0)
        {
            (Color color, string verb) = gain > 0
                ? (new Color(65, 105, 225), "Gained")
                : (new Color(0.74f, 0.28f, 0.01f), "Lost");
            InformationManager.DisplayMessage(
                new InformationMessage($"{verb} {gain} gold (reward: {reward.Gold}, upkeep: {message.RepairCost}).",
                    color));
        }

        if (message.Valour)
        {
            InformationManager.DisplayMessage(new InformationMessage("Thy valour on the battlefield has been rewarded!",
                new Color(218, 112, 214)));
        }

        if (message.SoldItemIds.Count != 0)
        {
            var soldItemNames = message.SoldItemIds
                .Select(i => MBObjectManager.Instance.GetObject<ItemObject>(i)?.Name.ToString())
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
}
