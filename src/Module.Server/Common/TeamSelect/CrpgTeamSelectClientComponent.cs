using System.Text;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.TeamSelect;

internal class CrpgTeamSelectClientComponent : MultiplayerTeamSelectComponent
{
    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        registerer.Register<TeamBalancedMessage>(HandleTeamBalanced);
    }

    private void HandleTeamBalanced(TeamBalancedMessage message)
    {
        StringBuilder notifBuilder = new();
        TextObject textObject;
        if (message.DefendersMovedToAttackers != 0)
        {
            textObject = new("{=xAnUFQt2}{COUNT} {?IS_PLURAL}players were{?}player was{\\?} moved to the attackers team{newline}",
                new Dictionary<string, object>
                {
                    ["COUNT"] = message.DefendersMovedToAttackers,
                    ["IS_PLURAL"] = message.DefendersMovedToAttackers > 1,
                });
            notifBuilder.Append(textObject);
        }

        if (message.AttackersMovedToDefenders != 0)
        {
            textObject = new("{=YTubwz4z}{COUNT} {?IS_PLURAL}players were{?}player was{\\?} moved to the defenders team{newline}",
                new Dictionary<string, object>
                {
                    ["COUNT"] = message.AttackersMovedToDefenders,
                    ["IS_PLURAL"] = message.AttackersMovedToDefenders > 1,
                });
            notifBuilder.Append(textObject);
        }

        if (message.DefendersJoined != 0)
        {
            textObject = new("{=ymrAOIug}{COUNT} new {?IS_PLURAL}players{?}player{\\?} joined the defenders team{newline}",
                new Dictionary<string, object>
                {
                    ["COUNT"] = message.DefendersJoined,
                    ["IS_PLURAL"] = message.DefendersJoined > 1,
                });
            notifBuilder.Append(textObject);
        }

        if (message.AttackersJoined != 0)
        {
            textObject = new("{=YFfWaWqk}{COUNT} new {?IS_PLURAL}players{?}player{\\?} joined the attackers team{newline}",
                new Dictionary<string, object>
                {
                    ["COUNT"] = message.AttackersJoined,
                    ["IS_PLURAL"] = message.AttackersJoined > 1,
                });
            notifBuilder.Append(textObject);
        }

        if (notifBuilder.Length == 0)
        {
            return;
        }

        MBInformationManager.AddQuickInformation(new TextObject(notifBuilder.ToString()));
    }
}
