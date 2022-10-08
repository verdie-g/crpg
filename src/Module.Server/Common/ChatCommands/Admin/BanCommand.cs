using System.Globalization;
using Crpg.Module.Common.Network;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;

namespace Crpg.Module.Common.ChatCommands.Admin;

internal class BanCommand : AdminCommand
{
    private enum BanDuration
    {
        Minutes = 1,
        Hours = 60,
        Days = 1440,
    }

    public BanCommand(ChatCommandsComponent chatComponent)
        : base(chatComponent)
    {
        Name = "ban";
        Description = $"'{ChatCommandsComponent.CommandPrefix}{Name} PLAYERID DURATION REASON' to ban a player.";
        Overloads = new CommandOverload[]
        {
            new(new[] { ChatCommandParameterType.Int32, ChatCommandParameterType.PlayerId, ChatCommandParameterType.String }, ExecuteBanByNetworkPeer), // !ban PLAYERID DURATION REASON
            new(new[] { ChatCommandParameterType.String, ChatCommandParameterType.Int32, ChatCommandParameterType.String }, ExecuteBanByName), // !ban NamePattern DURATION REASON
        };
    }

    protected override void ExecuteFailed(NetworkCommunicator fromPeer)
    {
        ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorInfo, $"Wrong usage. Type {Description}");
    }

    private async void ExecuteBanByNetworkPeer(NetworkCommunicator fromPeer, string cmd, object[] arguments)
    {
        var targetPeer = (NetworkCommunicator)arguments[0];
        int duration = (int)arguments[1];
        string reason = (string)arguments[2];
        BanDuration durationType = BanDuration.Days;
        if (arguments.Length == 4)
        {
            durationType = (BanDuration)arguments[3];
        }

        DateTime banUntilDate = DateTime.Now.AddMinutes(GetDurationMultiplier(durationType, duration));

        // TODO: Add web request to save the restriction
        // Call webrequest. Banned until banUntilDate
        var adminCrpgRepresentative = fromPeer.GetComponent<CrpgRepresentative>();
        var victimCrpgRepresentative = targetPeer.GetComponent<CrpgRepresentative>();
        if (adminCrpgRepresentative?.User?.Character == null && victimCrpgRepresentative?.User?.Character == null)
        {
            return;
        }

        ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorFatal, $"You banned {targetPeer.UserName} until {banUntilDate.ToString(CultureInfo.InvariantCulture)}.");
        ChatComponent.ServerSendServerMessageToEveryone(ColorFatal, $"{targetPeer.UserName} was banned by {fromPeer.UserName} until {banUntilDate.ToString(CultureInfo.InvariantCulture)}. Reason: {reason}");

        GameNetwork.BeginModuleEventAsServer(targetPeer);
        GameNetwork.WriteMessage(new CrpgNotification
        {
            Type = CrpgNotification.NotificationType.Announcement,
            Message = $"Banned by {fromPeer.UserName} until {banUntilDate.ToString(CultureInfo.InvariantCulture)}. Reason: {reason}",
        });
        GameNetwork.EndModuleEventAsServer();

        await Task.Delay(250);
        if (!targetPeer.IsConnectionActive)
        {
            return;
        }

        var disconnectInfo = targetPeer.PlayerConnectionInfo.GetParameter<DisconnectInfo>("DisconnectInfo") ?? new DisconnectInfo();
        disconnectInfo.Type = DisconnectType.BannedByPoll;
        targetPeer.PlayerConnectionInfo.AddParameter("DisconnectInfo", disconnectInfo);
        GameNetwork.AddNetworkPeerToDisconnectAsServer(targetPeer);
    }

    private void ExecuteBanByName(NetworkCommunicator fromPeer, string cmd, object[] arguments)
    {
        string targetName = (string)arguments[0];
        if (!TryGetPlayerByName(fromPeer, targetName, out var targetPeer))
        {
            return;
        }

        arguments = new[] { targetPeer!, arguments[1], arguments[2] };
        ExecuteBanByNetworkPeer(fromPeer, cmd, arguments);
    }

    private int GetDurationMultiplier(BanDuration md, int duration)
    {
        switch (md)
        {
            case BanDuration.Hours:
                return duration * 60;
            case BanDuration.Days:
                return duration * 60 * 24;
            default:
                return duration;
        }
    }
}
