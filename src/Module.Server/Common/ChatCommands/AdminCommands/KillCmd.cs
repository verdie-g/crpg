using Crpg.Module.Common.GameHandler;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.UserCommands;
internal class KillCmd : AdminCmd
{
    public KillCmd()
        : base()
    {
        Command = "kill";
        Description = $"'{ChatCommandHandler.CommandPrefix}{Command} PLAYERID' to kill a player.";
        PatternList = new Pattern[] { new Pattern("p", ExecuteKillByNetworkPeer), new Pattern("s", ExecuteKillByName) }.ToList();
    }

    protected override void ExecuteFailed(NetworkCommunicator fromPeer)
    {
        CrpgChatBox crpgChat = GetChat();
        crpgChat.ServerSendMessageToPlayer(fromPeer, ChatCommandHandler.ColorInfo, $"Wrong usage. Type {Description}");
    }

    private void ExecuteKillByNetworkPeer(NetworkCommunicator fromPeer, string cmd, List<object> parameters)
    {
        CrpgChatBox crpgChat = GetChat();
        var targetPeer = (NetworkCommunicator)parameters[0];

        Agent agent = targetPeer.ControlledAgent;
        if (agent == null || agent.Health <= 0)
        {
            crpgChat.ServerSendMessageToPlayer(fromPeer, ChatCommandHandler.ColorWarning, $"{targetPeer.UserName} is not alive.");
            return;
        }

        Blow blow = new(agent.Index);
        blow.DamageType = DamageTypes.Blunt;
        blow.BoneIndex = agent.Monster.HeadLookDirectionBoneIndex;
        blow.Position = agent.Position;
        blow.Position.z += agent.GetEyeGlobalHeight();
        blow.BaseMagnitude = 2000f;
        blow.WeaponRecord.FillAsMeleeBlow(null, null, -1, -1);
        blow.InflictedDamage = 2000;
        blow.SwingDirection = agent.LookDirection;
        blow.SwingDirection = agent.Frame.rotation.TransformToParent(new Vec3(0f, 1f));
        blow.SwingDirection.Normalize();

        blow.Direction = blow.SwingDirection;
        blow.DamageCalculated = true;
        agent.RegisterBlow(blow, default);

        crpgChat.ServerSendMessageToPlayer(fromPeer, ChatCommandHandler.ColorSuccess, $"You have killed {targetPeer.UserName}.");
        crpgChat.ServerSendMessageToPlayer(targetPeer, ChatCommandHandler.ColorFatal, $"You were killed by {fromPeer.UserName}.");
    }

    private void ExecuteKillByName(NetworkCommunicator fromPeer, string cmd, List<object> parameters)
    {
        string targetName = (string)parameters[0];
        var (success, targetPeer) = GetPlayerByName(fromPeer, targetName);
        if (!success || targetPeer == null)
        {
            return;
        }

        parameters = new List<object> { targetPeer };
        ExecuteKillByNetworkPeer(fromPeer, cmd, parameters);
    }
}
