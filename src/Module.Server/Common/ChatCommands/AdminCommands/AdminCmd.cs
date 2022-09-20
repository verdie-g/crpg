using Crpg.Module.Common.GameHandler;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.UserCommands;
internal class AdminCmd : ChatCommand
{
    public AdminCmd()
        : base()
    {
        Command = string.Empty;
        Pattern = new string[] { string.Empty }.ToList();
    }

    protected override bool CheckRequirements(NetworkCommunicator fromPeer)
    {
        base.CheckRequirements(fromPeer);
        bool isAdmin = true;
        // TODO: Add cRPG admin check
        return isAdmin;
    }

    protected override void ExecuteSuccess(NetworkCommunicator fromPeer, string cmd, List<object> parameters)
    {
    }
}
