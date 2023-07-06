﻿using Crpg.Module.Api;
using Crpg.Module.Api.Models.Restrictions;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;

namespace Crpg.Module.Common.ChatCommands.Admin;

internal class BanCommand : AdminCommand
{
    private readonly ICrpgClient _crpgClient;

    public BanCommand(ChatCommandsComponent chatComponent, ICrpgClient crpgClient)
        : base(chatComponent)
    {
        _crpgClient = crpgClient;
        Name = "ban";
        Description = $"'{ChatCommandsComponent.CommandPrefix}{Name} PLAYERID DURATION REASON' to ban a player.";
        Overloads = new CommandOverload[]
        {
            new(new[] { ChatCommandParameterType.PlayerId, ChatCommandParameterType.TimeSpan, ChatCommandParameterType.String }, ExecuteBanByNetworkPeer), // !ban PLAYERID DURATION REASON
            new(new[] { ChatCommandParameterType.String, ChatCommandParameterType.TimeSpan, ChatCommandParameterType.String }, ExecuteBanByName), // !ban NamePattern DURATION REASON
        };
    }

    private void ExecuteBanByNetworkPeer(NetworkCommunicator fromPeer, object[] arguments)
    {
        _ = ExecuteBanByNetworkPeerAsync(fromPeer, arguments);
    }

    private async Task ExecuteBanByNetworkPeerAsync(NetworkCommunicator fromPeer, object[] arguments)
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
                Type = CrpgRestrictionType.Join,
                Reason = reason,
                RestrictedByUserId = restrictedByUserId.Value,
            });
        }
        catch (Exception e)
        {
            Debug.Print("Could not ban: " + e);
            return;
        }

        if (duration == TimeSpan.Zero)
        {
            ChatComponent.ServerSendMessageToPlayer(targetPeer, ColorSuccess, $"You were unbanned by {fromPeer.UserName}.");
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorSuccess, $"You unbanned {targetPeer.UserName}.");
            return;
        }

        string durationStr = FormatTimeSpan(duration);
        ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorFatal, $"You banned {targetPeer.UserName} for {durationStr}.");
        ChatComponent.ServerSendServerMessageToEveryone(ColorFatal, $"{targetPeer.UserName} was banned by {fromPeer.UserName} for {durationStr}. Reason: {reason}");

        KickHelper.Kick(targetPeer, DisconnectType.BannedByPoll, "banned");
    }

    private void ExecuteBanByName(NetworkCommunicator fromPeer, object[] arguments)
    {
        string targetName = (string)arguments[0];
        if (!TryGetPlayerByName(fromPeer, targetName, out var targetPeer))
        {
            return;
        }

        arguments = new[] { targetPeer!, arguments[1], arguments[2] };
        ExecuteBanByNetworkPeer(fromPeer, arguments);
    }
}
