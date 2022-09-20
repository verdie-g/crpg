using Crpg.Module.Common.GameHandler;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.UserCommands;
internal class BanCmd : AdminCmd
{
    private enum BanDuration : int
    {
        Minutes = 1,
        Hours = 60,
        Days = 1440,
    }

    public BanCmd()
        : base()
    {
        Command = "ban";
        Description = $"'{ChatCommandHandler.CommandPrefix}{Command} PLAYERID DURATION REASON' to ban a player.";
        PatternList = new Pattern[] {
            new Pattern("dps", ExecuteBanByNetworkPeer), // !ban PLAYERID DURATION REASON
            new Pattern("sds", ExecuteBanByName), // !ban NamePattern DURATION REASON
        }.ToList();
    }

    protected override void ExecuteFailed(NetworkCommunicator fromPeer)
    {
        CrpgChatBox crpgChat = GetChat();
        crpgChat.ServerSendMessageToPlayer(fromPeer, ChatCommandHandler.ColorInfo, $"Wrong usage. Type {Description}");
    }

    private void ExecuteBanByNetworkPeer(NetworkCommunicator fromPeer, string cmd, List<object> parameters)
    {
        CrpgChatBox crpgChat = GetChat();
        var targetPeer = (NetworkCommunicator)parameters[0];
        double duration = (double)parameters[1];
        string reason = (string)parameters[2];
        BanDuration durationType = BanDuration.Days;
        if (parameters.Count == 4)
        {
            durationType = (BanDuration)parameters[3];
        }

        DateTime banUntilDate = DateTime.Now;
        banUntilDate.AddMinutes(duration * (int)durationType);

        // TODO: Add web request to save the restriction
        // Call webrequest. Banned until banUntilDate
        var adminCrpgRepresentative = fromPeer.GetComponent<CrpgRepresentative>();
        var victimCrpgRepresentative = targetPeer.GetComponent<CrpgRepresentative>();
        if (adminCrpgRepresentative?.User?.Character == null && victimCrpgRepresentative?.User?.Character == null)
        {
            return;
        }

        crpgChat.ServerSendMessageToPlayer(fromPeer, ChatCommandHandler.ColorFatal, $"You were banned by {fromPeer.UserName} until {banUntilDate.ToString("dd.MM.yy HH:mm")}.");
        crpgChat.ServerSendMessageToPlayer(targetPeer, ChatCommandHandler.ColorFatal, $"You banned {targetPeer.UserName} until {banUntilDate.ToString("dd.MM.yy HH:mm")}.");

    }

    private void ExecuteBanByName(NetworkCommunicator fromPeer, string cmd, List<object> parameters)
    {
        string targetName = (string)parameters[0];
        var (success, targetPeer) = GetPlayerByName(fromPeer, targetName);
        if (!success || targetPeer == null)
        {
            return;
        }

        parameters = new List<object> { targetPeer };
        ExecuteBanByNetworkPeer(fromPeer, cmd, parameters);
    }
}
