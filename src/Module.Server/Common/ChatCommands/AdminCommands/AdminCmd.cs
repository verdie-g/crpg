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
        if (user == null
            || ((user?.Role.HasFlag(Api.Models.Users.CrpgUserRole.Admin) ?? false)
            && (user?.Role.HasFlag(Api.Models.Users.CrpgUserRole.Moderator) ?? false)))
        {
            return false;
        }

        return true;
    }
}
