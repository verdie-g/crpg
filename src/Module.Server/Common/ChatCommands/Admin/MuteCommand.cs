using System.Globalization;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.Admin;

internal class MuteCommand : AdminCommand
{
    public MuteCommand(ChatCommandsComponent chatComponent)
        : base(chatComponent)
    {
        Name = "mute";
        Description = $"'{ChatCommandsComponent.CommandPrefix}{Name} PLAYERID' to mute a player.";
        Overloads = new CommandOverload[]
        {
            new(new[] { ChatCommandParameterType.PlayerId, ChatCommandParameterType.TimeSpan, ChatCommandParameterType.String }, ExecuteMuteByNetworkPeer), // !mute PLAYERID DURATION REASON
            new(new[] { ChatCommandParameterType.String, ChatCommandParameterType.TimeSpan, ChatCommandParameterType.String }, ExecuteMuteByName), // !mute NAMEPATTERN DURATION REASON
        };
    }

    protected override void ExecuteFailed(NetworkCommunicator fromPeer)
    {
        ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorInfo, $"Wrong usage. Type {Description}");
    }

    private void ExecuteMuteByName(NetworkCommunicator fromPeer, string cmd, object[] arguments)
    {
        string targetName = (string)arguments[0];
        if (!TryGetPlayerByName(fromPeer, targetName, out var targetPeer))
        {
            return;
        }

        int duration = (int)arguments[1];
        string reason = (string)arguments[2];

        arguments = new object[] { targetPeer!, duration, reason };
        ExecuteMuteByNetworkPeer(fromPeer, cmd, arguments);
    }

    private void ExecuteMuteByNetworkPeer(NetworkCommunicator fromPeer, string cmd, object[] arguments)
    {
        var targetPeer = (NetworkCommunicator)arguments[0];
        var duration = (TimeSpan)arguments[1];
        string reason = (string)arguments[2];

        DateTime muteUntilDate = DateTime.UtcNow.Add(duration);

        // Call mute for backend
        var adminCrpgRepresentative = fromPeer.GetComponent<CrpgRepresentative>();
        var victimCrpgRepresentative = targetPeer.GetComponent<CrpgRepresentative>();
        if (adminCrpgRepresentative?.User?.Character == null && victimCrpgRepresentative?.User?.Character == null)
        {
            return;
        }

        // TODO: Add web request to save the restriction
        // Call webrequest. Muted until muteUntilDate
        if (duration == TimeSpan.Zero)
        {
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorSuccess, $"You were unmuted by {fromPeer.UserName}.");
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorSuccess, $"You muted {targetPeer.UserName} until {muteUntilDate.ToString(CultureInfo.InvariantCulture)}.");
            targetPeer.IsMuted = false;
        }
        else
        {
            ChatComponent.ServerSendMessageToPlayer(targetPeer, ColorFatal, $"You were muted by {fromPeer.UserName} until {muteUntilDate.ToString(CultureInfo.InvariantCulture)}.");
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorFatal, $"You muted {targetPeer.UserName} until {muteUntilDate.ToString(CultureInfo.InvariantCulture)}.");
            targetPeer.IsMuted = true;
        }
    }
}
