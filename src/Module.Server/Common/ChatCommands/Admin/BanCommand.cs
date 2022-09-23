using System.Globalization;
using Crpg.Module.Common.GameHandler;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.Admin;

internal class BanCommand : AdminCommand
{
    private enum BanDuration
    {
        Minutes = 1,
        Hours = 60,
        Days = 1440,
    }

    public BanCommand()
    {
        Name = "ban";
        Description = $"'{ChatCommandHandler.CommandPrefix}{Name} PLAYERID DURATION REASON' to ban a player.";
        Overloads = new CommandOverload[]
        {
            new(new[] { ChatCommandParameterType.Int32, ChatCommandParameterType.PlayerId, ChatCommandParameterType.String }, ExecuteBanByNetworkPeer), // !ban PLAYERID DURATION REASON
            new(new[] { ChatCommandParameterType.String, ChatCommandParameterType.Int32, ChatCommandParameterType.String }, ExecuteBanByName), // !ban NamePattern DURATION REASON
        };
    }

    protected override void ExecuteFailed(NetworkCommunicator fromPeer)
    {
        CrpgChatBox crpgChat = GetChat();
        crpgChat.ServerSendMessageToPlayer(fromPeer, ColorInfo, $"Wrong usage. Type {Description}");
    }

    private void ExecuteBanByNetworkPeer(NetworkCommunicator fromPeer, string cmd, object[] arguments)
    {
        CrpgChatBox crpgChat = GetChat();
        var targetPeer = (NetworkCommunicator)arguments[0];
        double duration = (double)arguments[1];
        string reason = (string)arguments[2];
        BanDuration durationType = BanDuration.Days;
        if (arguments.Length == 4)
        {
            durationType = (BanDuration)arguments[3];
        }

        DateTime banUntilDate = DateTime.Now.AddMinutes(duration * (int)durationType);

        // TODO: Add web request to save the restriction
        // Call webrequest. Banned until banUntilDate
        var adminCrpgRepresentative = fromPeer.GetComponent<CrpgRepresentative>();
        var victimCrpgRepresentative = targetPeer.GetComponent<CrpgRepresentative>();
        if (adminCrpgRepresentative?.User?.Character == null && victimCrpgRepresentative?.User?.Character == null)
        {
            return;
        }

        crpgChat.ServerSendMessageToPlayer(fromPeer, ColorFatal, $"You were banned by {fromPeer.UserName} until {banUntilDate.ToString(CultureInfo.InvariantCulture)}.");
        crpgChat.ServerSendMessageToPlayer(targetPeer, ColorFatal, $"You banned {targetPeer.UserName} until {banUntilDate.ToString(CultureInfo.InvariantCulture)}.");
    }

    private void ExecuteBanByName(NetworkCommunicator fromPeer, string cmd, object[] arguments)
    {
        string targetName = (string)arguments[0];
        if (!TryGetPlayerByName(fromPeer, targetName, out var targetPeer))
        {
            return;
        }

        arguments = new object[] { targetPeer! };
        ExecuteBanByNetworkPeer(fromPeer, cmd, arguments);
    }
}
