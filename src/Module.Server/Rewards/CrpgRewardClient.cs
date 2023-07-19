using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Rewards;

internal class CrpgRewardClient : MissionNetwork
{
    private static readonly TextObject[] ValourTextObjects =
    {
        new("{=xdQcFWfZ}Thy valor on the battlefield hath been recompensed."),
        new("{=xYyy6xz3}Forsooth, thy gallantry on the field of war hath been duly recompensed."),
        new("{=3HpaYByO}Verily, thy courage in battle hath been justly rewarded."),
        new("{=3vgBtss3}Thou hast been duly honored for thy valor on the field of combat."),
        new("{=qm2JZb2E}Thy bravery amidst the tumult of war hath earned thee due reward."),
        new("{=z664pE4D}In truth, thy deeds of valor on the battlefield have not gone unrewarded."),
        new("{=seaYzzTP}For thy valiant efforts on the field of battle, thou hast been richly rewarded."),
        new("{=xIrp6MtP}Thou hast proven thyself a brave warrior, and thy reward is justly deserved."),
        new("{=SiqtRusd}Thy prowess in battle hath been rewarded, as it ought to be."),
        new("{=tbhzI05Z}Thou hast earned thy reward by thy gallantry and bravery in the heat of battle."),
        new("{=RboA0fG3}As a reward for thy courage on the battlefield, thou hast been duly recognized."),
        new("{=u8g4PqMI}By thy valiant actions in the midst of war, thou hast earned thy rightful reward."),
        new("{=DHfICspz}Thy bravery hath been acknowledged and suitably rewarded for thy actions in battle."),
        new("{=jxFwx5hu}For thy boldness on the battlefield, thou hast been granted a well-deserved reward."),
        new("{=H6JfGFXJ}Thou hast displayed courage and valor in battle, and thy reward reflects it."),
        new("{=Jf9iQPcI}Thy actions on the field of war have earned thee justly deserved recognition and reward."),
        new("{=DbZZjYBU}In recognition of thy gallantry on the field of battle, thou hast been granted a reward."),
        new("{=cfkeonsy}Thou hast earned a reward for thy bravery in the face of danger on the battlefield."),
        new("{=5f3l5xxW}Thy valor in the midst of battle hath been rightfully rewarded."),
        new("{=D4Enf8wD}Thou hast fought bravely and thy reward is a testament to thy valor."),
        new("{=wcXPUS2Y}Thou hast earned a just reward for thy deeds of bravery in the heat of battle."),
        new("{=u2iBIkrl}For thy bravery in the face of adversity on the battlefield, thou hast received a fitting reward."),
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
            TextObject experienceTextObject = new("{=gmTamBmO}Gained {XP} experience.", new Dictionary<string, object>
            {
                ["XP"] = reward.Experience,
            });
            InformationManager.DisplayMessage(
                new InformationMessage(experienceTextObject.ToString(),
                new Color(218, 112, 214)));
        }

        int gain = reward.Gold - message.RepairCost;
        if (gain != 0)
        {
            string upkeepMessage = message.LowPopulation
                ? new TextObject("{=5Uvrn21t}free, low population server").ToString()
                : message.RepairCost.ToString();

            (Color color, TextObject goldTextObject) = gain > 0
                ? (new Color(65, 105, 225), new TextObject("{=5HdPy5hH}Gained {GAIN} gold (reward: {REWARD}, upkeep: {UPKEEP_MESSAGE})."))
                : (new Color(0.74f, 0.28f, 0.01f), new TextObject("{=FQr3PBEQ}Lost {GAIN} gold (reward: {REWARD}, upkeep: {UPKEEP_MESSAGE})."));
            goldTextObject.SetTextVariable("GAIN", Math.Abs(gain));
            goldTextObject.SetTextVariable("REWARD", reward.Gold);
            goldTextObject.SetTextVariable("UPKEEP_MESSAGE", upkeepMessage);
            InformationManager.DisplayMessage(new InformationMessage(goldTextObject.ToString(), color));
        }

        if (message.Compensation != 0)
        {
            (Color color, TextObject compensationTextObject) = message.Compensation > 0
                ? (new Color(65, 105, 225), new TextObject("{=H6BYX9wi}Gained {COMPENSATION} gold as compensation for being teamhit."))
                : (new Color(0.74f, 0.28f, 0.01f), new TextObject("{=uOMI4EkG}Lost {COMPENSATION} gold as compensation for teamhitting others."));
            compensationTextObject.SetTextVariable("COMPENSATION", message.Compensation);
            InformationManager.DisplayMessage(new InformationMessage(compensationTextObject.ToString(), color));
        }

        if (message.Valour)
        {
            var valourTextObject = ValourTextObjects.GetRandomElement();
            InformationManager.DisplayMessage(
                new InformationMessage(valourTextObject.ToString(),
                new Color(0.48f, 0f, 1f)));
        }

        if (message.BrokeItemIds.Count != 0)
        {
            var brokeItemNames = message.BrokeItemIds
                .Select(i => MBObjectManager.Instance.GetObject<ItemObject>(i)?.Name.ToString())
                .Where(i => i != null);
            TextObject brokenItemsTextObject = new(
                "{=2m5CdjYa}You were unable to afford the upkeep cost for the {?IS_PLURAL}item{?}items{\\?} {ITEMS},"
                + " which resulted in {?IS_PLURAL}them{?}it{\\?} breaking and becoming unequipped. You will need to visit"
                + " the Web UI and equip a less expensive item.", new Dictionary<string, object>
                {
                    ["IS_PLURAL"] = message.BrokeItemIds.Count > 1,
                    ["ITEMS"] = string.Join(", ", brokeItemNames),
                });
            InformationManager.DisplayMessage(
                new InformationMessage(brokenItemsTextObject.ToString(),
                new Color(0.74f, 0.28f, 0.01f)));
        }

        if (reward.LevelUp)
        {
            InformationManager.DisplayMessage(new InformationMessage
            {
                Information = new TextObject("{=5Cjwg9zQ}Level up!").ToString(),
                Color = new Color(128, 0, 128),
                SoundEventPath = "event:/ui/notification/levelup",
            });
        }
    }

    private void HandleRewardError(CrpgRewardError message)
    {
        InformationManager.DisplayMessage(
            new InformationMessage(new TextObject("{=8eHDaW4v}Could not join cRPG main server. Your reward was lost.").ToString(),
                new Color(0.75f, 0.01f, 0.01f)));
    }
}
