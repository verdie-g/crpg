using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Common;

internal class CrpgRewardClient : MissionNetwork
{
    private static readonly string[] ValourMessages =
    {
        "Thy valor on the battlefield hath been recompensed.",
        "Forsooth, thy gallantry on the field of war hath been duly recompensed.",
        "Verily, thy courage in battle hath been justly rewarded.",
        "Thou hast been duly honored for thy valor on the field of combat.",
        "Thy bravery amidst the tumult of war hath earned thee due reward.",
        "In truth, thy deeds of valor on the battlefield have not gone unrewarded.",
        "For thy valiant efforts on the field of battle, thou hast been richly rewarded.",
        "Thou hast proven thyself a brave warrior, and thy reward is justly deserved.",
        "Thy prowess in battle hath been rewarded, as it ought to be.",
        "Thou hast earned thy reward by thy gallantry and bravery in the heat of battle.",
        "As a reward for thy courage on the battlefield, thou hast been duly recognized.",
        "By thy valiant actions in the midst of war, thou hast earned thy rightful reward.",
        "Thy bravery hath been acknowledged and suitably rewarded for thy actions in battle.",
        "For thy boldness on the battlefield, thou hast been granted a well-deserved reward.",
        "Thou hast displayed courage and valor in battle, and thy reward reflects it.",
        "Thy actions on the field of war have earned thee justly deserved recognition and reward.",
        "In recognition of thy gallantry on the field of battle, thou hast been granted a reward.",
        "Thou hast earned a reward for thy bravery in the face of danger on the battlefield.",
        "Thy valor in the midst of battle hath been rightfully rewarded.",
        "Thou hast fought bravely and thy reward is a testament to thy valor.",
        "Thou hast earned a just reward for thy deeds of bravery in the heat of battle.",
        "For thy bravery in the face of adversity on the battlefield, thou hast received a fitting reward.",
    };

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
            string valourMessage = ValourMessages.GetRandomElement();
            InformationManager.DisplayMessage(new InformationMessage(valourMessage,
                new Color(0.48f, 0f, 1f)));
        }

        if (message.BrokeItemIds.Count != 0)
        {
            var brokeItemNames = message.BrokeItemIds
                .Select(i => MBObjectManager.Instance.GetObject<ItemObject>(i)?.Name.ToString())
                .Where(i => i != null);
            string brokeItemNamesStr = string.Join(", ", brokeItemNames);
            string s = message.BrokeItemIds.Count > 1 ? "s" : string.Empty;
            InformationManager.DisplayMessage(new InformationMessage(
                $"You were unable to afford the upkeep cost for the item{s} {brokeItemNamesStr}, which resulted in them breaking and becoming unequipped. You will need to visit the Web UI and equip a less expensive item.",
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
