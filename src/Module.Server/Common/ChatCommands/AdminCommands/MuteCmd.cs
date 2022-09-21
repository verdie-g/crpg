using Crpg.Module.Api.Models;
using Crpg.Module.Common.GameHandler;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;

namespace Crpg.Module.Common.ChatCommands.UserCommands;
internal class MuteCmd : AdminCmd
{
    private enum MuteDuration : int
    {
        Minutes = 1,
        Hours = 60,
        Days = 1440,
    }

    public MuteCmd()
        : base()
    {
        Command = "mute";
        Description = $"'{ChatCommandHandler.CommandPrefix}{Command} PLAYERID' to mute a player.";
        PatternList = new Pattern[]
        {
            new Pattern("dps", ExecuteMuteByNetworkPeer), // !mute PLAYERID DURATION REASON
            new Pattern("sds", ExecuteMuteByName), // !mute NamePattern DURATION REASON
        }.ToList();
    }

    protected override void ExecuteFailed(NetworkCommunicator fromPeer)
    {
        CrpgChatBox crpgChat = GetChat();
        crpgChat.ServerSendMessageToPlayer(fromPeer, CrpgChatBox.ColorInfo, $"Wrong usage. Type {Description}");
    }

    private void ExecuteMuteByNetworkPeer(NetworkCommunicator fromPeer, string cmd, List<object> parameters)
    {
        CrpgChatBox crpgChat = GetChat();
        var targetPeer = (NetworkCommunicator)parameters[0];
        double duration = (double)parameters[1];
        string reason = (string)parameters[2];
        MuteDuration durationType = MuteDuration.Days;
        if (parameters.Count == 4)
        {
            durationType = (MuteDuration)parameters[3];
        }

        DateTime muteUntilDate = DateTime.Now;
        muteUntilDate.AddMinutes(duration * (int)durationType);

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
            crpgChat.ServerSendMessageToPlayer(fromPeer, CrpgChatBox.ColorSuccess, $"You were unmuted by {fromPeer.UserName}.");
            crpgChat.ServerSendMessageToPlayer(fromPeer, CrpgChatBox.ColorSuccess, $"You muted {targetPeer.UserName} until {muteUntilDate.ToString("dd.MM.yy HH:mm")}.");
            targetPeer.IsMuted = false;
        }
        else
        {
            crpgChat.ServerSendMessageToPlayer(targetPeer, CrpgChatBox.ColorFatal, $"You were muted by {fromPeer.UserName} until {muteUntilDate.ToString("dd.MM.yy HH:mm")}.");
            crpgChat.ServerSendMessageToPlayer(fromPeer, CrpgChatBox.ColorFatal, $"You muted {targetPeer.UserName} until {muteUntilDate.ToString("dd.MM.yy HH:mm")}.");
            targetPeer.IsMuted = true;
        }
    }

    private void ExecuteMuteByName(NetworkCommunicator fromPeer, string cmd, List<object> parameters)
    {
        string targetName = (string)parameters[0];
        var (success, targetPeer) = GetPlayerByName(fromPeer, targetName);
        if (!success || targetPeer == null)
        {
            return;
        }

        parameters = new List<object> { targetPeer };
        ExecuteMuteByNetworkPeer(fromPeer, cmd, parameters);
    }
}
