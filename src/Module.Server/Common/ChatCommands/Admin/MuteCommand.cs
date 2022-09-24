using System.Globalization;
using Crpg.Module.Common.GameHandler;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.Admin;

internal class MuteCommand : AdminCommand
{
    private enum MuteDuration
    {
        Minutes = 1,
        Hours = 60,
        Days = 1440,
    }

    public MuteCommand()
    {
        Name = "mute";
        Description = $"'{ChatCommandHandler.CommandPrefix}{Name} PLAYERID' to mute a player.";
        Overloads = new CommandOverload[]
        {
            new(new[] { ChatCommandParameterType.Int32, ChatCommandParameterType.PlayerId, ChatCommandParameterType.String }, ExecuteMuteByNetworkPeer),
            new(new[] { ChatCommandParameterType.String, ChatCommandParameterType.Int32, ChatCommandParameterType.String }, ExecuteMuteByName),
        };
    }

    protected override void ExecuteFailed(NetworkCommunicator fromPeer)
    {
        CrpgChatBox crpgChat = GetChat();
        crpgChat.ServerSendMessageToPlayer(fromPeer, ColorInfo, $"Wrong usage. Type {Description}");
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
        CrpgChatBox crpgChat = GetChat();
        var targetPeer = (NetworkCommunicator)arguments[0];
        int duration = (int)arguments[1];
        string reason = (string)arguments[2];
        MuteDuration durationType = MuteDuration.Days;

        DateTime muteUntilDate = DateTime.Now.AddMinutes(GetDurationMultiplier(durationType, duration));

        // Call mute for backend
        var adminCrpgRepresentative = fromPeer.GetComponent<CrpgRepresentative>();
        var victimCrpgRepresentative = targetPeer.GetComponent<CrpgRepresentative>();
        if (adminCrpgRepresentative?.User?.Character == null && victimCrpgRepresentative?.User?.Character == null)
        {
            return;
        }

        // TODO: Add web request to save the restriction
        // Call webrequest. Muted until muteUntilDate
        if (duration == 0)
        {
            crpgChat.ServerSendMessageToPlayer(fromPeer, ColorSuccess, $"You were unmuted by {fromPeer.UserName}.");
            crpgChat.ServerSendMessageToPlayer(fromPeer, ColorSuccess, $"You muted {targetPeer.UserName} until {muteUntilDate.ToString(CultureInfo.InvariantCulture)}.");
            targetPeer.IsMuted = false;
        }
        else
        {
            crpgChat.ServerSendMessageToPlayer(targetPeer, ColorFatal, $"You were muted by {fromPeer.UserName} until {muteUntilDate.ToString(CultureInfo.InvariantCulture)}.");
            crpgChat.ServerSendMessageToPlayer(fromPeer, ColorFatal, $"You muted {targetPeer.UserName} until {muteUntilDate.ToString(CultureInfo.InvariantCulture)}.");
            targetPeer.IsMuted = true;
        }
    }

    private int GetDurationMultiplier(MuteDuration md, int duration)
    {
        switch (md)
        {
            case MuteDuration.Hours:
                return duration * 60;
            case MuteDuration.Days:
                return duration * 60 * 24;
            default:
                return duration;
        }
    }
}
