using Crpg.Module.Api;
using Crpg.Module.Common.Network;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

#if CRPG_SERVER
using Crpg.Module.Common.ChatCommands.Admin;
using Crpg.Module.Common.ChatCommands.User;
#endif

namespace Crpg.Module.Common.ChatCommands;

internal class ChatCommandsComponent : MissionBehavior
{
    public const char CommandPrefix = '!';

    private readonly ChatBox _chatBox;
    private readonly List<QueuedMessageInfo> _queuedServerMessages;
    private readonly ChatCommand[] _commands;

    public ChatCommandsComponent(ChatBox chatBox, ICrpgClient crpgClient)
    {
        _chatBox = chatBox;
        _queuedServerMessages = new List<QueuedMessageInfo>();
#if CRPG_SERVER
        _commands = new ChatCommand[]
        {
            new PingCommand(this),
            new PlayerListCommand(this),
            new KickCommand(this),
            new KillCommand(this),
            new TeleportCommand(this),
            new AnnouncementCommand(this),
            new MuteCommand(this, crpgClient),
            new BanCommand(this, crpgClient),
        };
#else
        _commands = Array.Empty<ChatCommand>();
#endif
    }

    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

    public void ServerSendMessageToPlayer(NetworkCommunicator targetPlayer, Color color, string message)
    {
        if (!targetPlayer.IsSynchronized)
        {
            _queuedServerMessages.Add(new QueuedMessageInfo(targetPlayer, message));
            return;
        }

        if (!targetPlayer.IsServerPeer && targetPlayer.IsSynchronized)
        {
            GameNetwork.BeginModuleEventAsServer(targetPlayer);
            GameNetwork.WriteMessage(new CrpgServerMessage
            {
                Message = message,
                Red = color.Red,
                Green = color.Green,
                Blue = color.Blue,
                Alpha = color.Alpha,
                IsMessageTextId = false,
            });
            GameNetwork.EndModuleEventAsServer();
        }
    }

    public void ServerSendMessageToPlayer(NetworkCommunicator targetPlayer, string message)
    {
        ServerSendMessageToPlayer(targetPlayer, new Color(1, 1, 1), message);
    }

    public void ServerSendServerMessageToEveryone(Color color, string message)
    {
        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new CrpgServerMessage
        {
            Message = message,
            Red = color.Red,
            Green = color.Green,
            Blue = color.Blue,
            Alpha = color.Alpha,
            IsMessageTextId = false,
        });
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.IncludeUnsynchronizedClients);
    }

    public override void OnBehaviorInitialize()
    {
        _chatBox.OnMessageReceivedAtDedicatedServer = (Action<NetworkCommunicator, string>)Delegate.Combine(
            OnMessageReceivedAtDedicatedServer,
            _chatBox.OnMessageReceivedAtDedicatedServer);
    }

    public override void OnRemoveBehavior()
    {
        _chatBox.OnMessageReceivedAtDedicatedServer = (Action<NetworkCommunicator, string>)Delegate.Remove(
            OnMessageReceivedAtDedicatedServer,
            _chatBox.OnMessageReceivedAtDedicatedServer)!;
    }

#if CRPG_SERVER
    public override void OnMissionTick(float dt)
    {
        for (int i = 0; i < _queuedServerMessages.Count; i++)
        {
            QueuedMessageInfo queuedMessageInfo = _queuedServerMessages[i];
            if (queuedMessageInfo.SourcePeer.IsSynchronized)
            {
                ServerSendMessageToPlayer(queuedMessageInfo.SourcePeer, queuedMessageInfo.Message);
                _queuedServerMessages.RemoveAt(i);
            }
            else if (queuedMessageInfo.IsExpired)
            {
                _queuedServerMessages.RemoveAt(i);
            }
        }
    }
#endif

    private void OnMessageReceivedAtDedicatedServer(NetworkCommunicator fromPeer, string message)
    {
        if (message[0] != CommandPrefix)
        {
            return;
        }

        string[] tokens = message.Substring(1).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
        {
            return;
        }

        string name = tokens[0].ToLowerInvariant();
        var command = _commands.FirstOrDefault(c => c.Name == name);
        if (command == null)
        {
            return;
        }

        _ = HideChatInput(fromPeer);
        command.Execute(fromPeer, name, tokens.Skip(1).ToArray());
    }

    // Hacky workaround until we can actually control which message should be sent to everyone.
    private async Task HideChatInput(NetworkCommunicator fromPeer)
    {
        bool muted = fromPeer.IsMuted;
        fromPeer.IsMuted = true;
        await Task.Delay(100);
        fromPeer.IsMuted = muted;
    }

    private class QueuedMessageInfo
    {
        private const float TimeOutDuration = 3f;

        private readonly DateTime _creationTime;

        public QueuedMessageInfo(NetworkCommunicator sourcePeer, Color color, string message)
        {
            SourcePeer = sourcePeer;
            Message = message;
            Color = color;
            _creationTime = DateTime.Now;
        }

        public QueuedMessageInfo(NetworkCommunicator sourcePeer, string message)
            : this(sourcePeer, new Color(1, 1, 1), message)
        {
        }

        public NetworkCommunicator SourcePeer { get; }
        public string Message { get; }
        public Color Color { get; }
        public bool IsExpired => (DateTime.Now - _creationTime).TotalSeconds >= TimeOutDuration;
    }
}
