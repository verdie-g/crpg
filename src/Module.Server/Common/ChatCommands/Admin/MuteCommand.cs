using Crpg.Module.Api;
using Crpg.Module.Api.Models.Restrictions;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.Admin;

internal class MuteCommand : AdminCommand
{
    private readonly ICrpgClient _crpgClient;

    public MuteCommand(ChatCommandsComponent chatComponent, ICrpgClient crpgClient)
        : base(chatComponent)
    {
        _crpgClient = crpgClient;
        Name = "mute";
        Description = $"'{ChatCommandsComponent.CommandPrefix}{Name} PLAYERID' to mute a player.";
        Overloads = new CommandOverload[]
        {
            new(new[] { ChatCommandParameterType.PlayerId, ChatCommandParameterType.TimeSpan, ChatCommandParameterType.String }, ExecuteMuteByNetworkPeer), // !mute PLAYERID DURATION REASON
            new(new[] { ChatCommandParameterType.String, ChatCommandParameterType.TimeSpan, ChatCommandParameterType.String }, ExecuteMuteByName), // !mute NAMEPATTERN DURATION REASON
        };
    }

    private void ExecuteMuteByName(NetworkCommunicator fromPeer, object[] arguments)
    {
        string targetName = (string)arguments[0];
        if (!TryGetPlayerByName(fromPeer, targetName, out var targetPeer))
        {
            return;
        }

        int duration = (int)arguments[1];
        string reason = (string)arguments[2];

        arguments = new object[] { targetPeer!, duration, reason };
        ExecuteMuteByNetworkPeer(fromPeer, arguments);
    }

    private void ExecuteMuteByNetworkPeer(NetworkCommunicator fromPeer, object[] arguments)
    {
        _ = ExecuteMuteByNetworkPeerAsync(fromPeer, arguments);
    }

    private async Task ExecuteMuteByNetworkPeerAsync(NetworkCommunicator fromPeer, object[] arguments)
    {
        var targetPeer = (NetworkCommunicator)arguments[0];
        var duration = (TimeSpan)arguments[1];
        string reason = (string)arguments[2];

        int? restrictedByUserId = fromPeer.GetComponent<CrpgPeer>()?.User?.Id;
        int? restrictedUserId = targetPeer.GetComponent<CrpgPeer>()?.User?.Id;
        if (restrictedUserId == null || restrictedByUserId == null)
        {
            return;
        }

        try
        {
            await _crpgClient.RestrictUserAsync(new CrpgRestrictionRequest
            {
                RestrictedUserId = restrictedUserId.Value,
                Duration = duration,
                Type = CrpgRestrictionType.Chat,
                Reason = reason,
                RestrictedByUserId = restrictedByUserId.Value,
            });
        }
        catch (Exception e)
        {
            Debug.Print("Could not mute: " + e);
            return;
        }

        if (duration == TimeSpan.Zero)
        {
            ChatComponent.ServerSendMessageToPlayer(targetPeer, ColorSuccess, $"You were unmuted by {fromPeer.UserName}.");
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorSuccess, $"You unmuted {targetPeer.UserName}.");
            targetPeer.IsMuted = false;
        }
        else
        {
            ChatComponent.ServerSendMessageToPlayer(targetPeer, ColorFatal, $"You were muted by {fromPeer.UserName} for {duration}.");
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorFatal, $"You muted {targetPeer.UserName} for {duration}.");
            targetPeer.IsMuted = true;
        }
    }
}
