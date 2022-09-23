using Crpg.Module.Common.ChatCommands;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
#if CRPG_SERVER
using TaleWorlds.MountAndBlade.DedicatedCustomServer;
#endif

namespace Crpg.Module.Common.GameHandler;

internal class CrpgChatBox : TaleWorlds.Core.GameHandler
{
    private ChatBox? _chatBox;
    private List<QueuedMessageInfo> _queuedServerMessages;
    private bool _isNetworkInitialized;

    public CrpgChatBox()
    {
        _queuedServerMessages = new();

#if CRPG_SERVER
        DedicatedCustomGameServerState.OnActivated += DedicatedCustomGameServerStateActivated;
#endif
    }

    public void DedicatedCustomGameServerStateActivated()
    {
        _chatBox = Game.Current.GetGameHandler<ChatBox>();
        _chatBox.OnMessageReceivedAtDedicatedServer = (Action<NetworkCommunicator, string>)Delegate.Combine(_chatBox.OnMessageReceivedAtDedicatedServer, new Action<NetworkCommunicator, string>(OnMessageReceivedAtDedicatedServer));
    }

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

    public void OnMessageReceivedAtDedicatedServer(NetworkCommunicator fromPeer, string message)
    {
        if (message[0] == ChatCommandHandler.CommandPrefix)
        {
            ChatCommandHandler.TryExecuteCommand(fromPeer, message.Substring(1));
        }
    }

    public override void OnBeforeSave()
    {
    }

    public override void OnAfterSave()
    {
    }

    protected override void OnGameNetworkBegin()
    {
        _queuedServerMessages = new List<QueuedMessageInfo>();
        _isNetworkInitialized = true;
        AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
    }

    protected override void OnGameNetworkEnd()
    {
        base.OnGameNetworkEnd();
        AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
    }

    protected override void OnTick(float dt)
    {
        if (!GameNetwork.IsServer || !_isNetworkInitialized)
        {
            return;
        }

        for (int i = 0; i < _queuedServerMessages.Count; i++)
        {
            QueuedMessageInfo queuedMessageInfo2 = _queuedServerMessages[i];
            if (queuedMessageInfo2.SourcePeer.IsSynchronized)
            {
                ServerSendMessageToPlayer(queuedMessageInfo2.SourcePeer, queuedMessageInfo2.Message);
                _queuedServerMessages.RemoveAt(i);
            }
            else if (queuedMessageInfo2.IsExpired)
            {
                _queuedServerMessages.RemoveAt(i);
            }
        }
    }

    private void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
    {
        GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new(mode);
        if (GameNetwork.IsClient)
        {
            networkMessageHandlerRegisterer.Register<CrpgServerMessage>(HandleCrpgServerMessage);
        }
    }

    private void HandleCrpgServerMessage(CrpgServerMessage message)
    {
        string msg = message.IsMessageTextId ? GameTexts.FindText(message.Message).ToString() : message.Message;
        InformationManager.DisplayMessage(new InformationMessage(msg, new Color(message.Red, message.Green, message.Blue, message.Alpha)));
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
