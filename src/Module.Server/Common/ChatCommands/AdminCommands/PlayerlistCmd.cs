using Crpg.Module.Common.GameHandler;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.UserCommands;
internal class PlayerlistCmd : AdminCmd
{
    public PlayerlistCmd()
        : base()
    {
        Command = "pl";
        PatternList = new Pattern[]
        {
            new Pattern(new ParameterType[] { }.ToList(), ExecuteSuccess),
        }.ToList();
    }

    private void ExecuteSuccess(NetworkCommunicator fromPeer, string cmd, List<object> parameters)
    {
        PrintPlayerList(fromPeer, GameNetwork.NetworkPeers.ToList());
    }
}
