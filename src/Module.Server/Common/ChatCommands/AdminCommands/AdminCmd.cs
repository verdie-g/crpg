using Crpg.Module.Common.GameHandler;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.UserCommands;
internal abstract class AdminCmd : ChatCommand
{
    public AdminCmd()
        : base()
    {
        Command = string.Empty;
        PatternList = new Pattern[] { }.ToList();
    }

    protected override bool CheckRequirements(NetworkCommunicator fromPeer)
    {
        bool hasAccess = base.CheckRequirements(fromPeer);
        // TODO: Add cRPG admin check
        hasAccess = true;
        return hasAccess;
    }

}
