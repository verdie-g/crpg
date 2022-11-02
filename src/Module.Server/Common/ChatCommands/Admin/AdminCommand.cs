using Crpg.Module.Api.Models.Users;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.Admin;

internal abstract class AdminCommand : ChatCommand
{
    protected AdminCommand(ChatCommandsComponent chatComponent)
        : base(chatComponent)
    {
    }

    protected override bool CheckRequirements(NetworkCommunicator fromPeer)
    {
        var crpgUser = fromPeer.GetComponent<CrpgPeer>()?.User;
        if (crpgUser == null)
        {
            return false;
        }

        return crpgUser.Role is CrpgUserRole.Moderator or CrpgUserRole.Admin;
    }
}
