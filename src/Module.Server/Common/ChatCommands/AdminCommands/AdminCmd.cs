using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.UserCommands;

internal abstract class AdminCmd : ChatCommand
{
    public AdminCmd()
        : base()
    {
    }

    protected override bool CheckRequirements(NetworkCommunicator fromPeer)
    {
        // TODO: Add cRPG admin check
        var crpgRepresentative = fromPeer.GetComponent<CrpgRepresentative>();
        var user = crpgRepresentative?.User;
        if (user == null)
        {
            return false;
        }

        return user.Role == Api.Models.Users.CrpgUserRole.Admin || user.Role == Api.Models.Users.CrpgUserRole.Moderator;
    }
}
