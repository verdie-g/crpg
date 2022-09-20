using System;
using System.Collections.Generic;
using System.Text;
using Crpg.Module.Common.ChatCommands;
using Crpg.Module.Common.ChatCommands.UserCommands;
using Crpg.Module.Common.Network;
using Crpg.Module.Helpers;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
#if CRPG_SERVER
using TaleWorlds.MountAndBlade.DedicatedCustomServer;
#endif
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.GameHandler;
internal class CrpgChatBox : TaleWorlds.Core.GameHandler
{
    private class QueuedMessageInfo
    {
        public NetworkCommunicator SourcePeer { get; private set; }
        public string Message { get; private set; }
        public Color Color { get; private set; }
        private const float _timeOutDuration = 3f;
        private readonly DateTime _creationTime;
        public bool IsExpired => (DateTime.Now - _creationTime).TotalSeconds >= _timeOutDuration;
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
    }

    private static ChatBox? _chatBox;
    private static List<QueuedMessageInfo>? _queuedServerMessages;
    private bool _isNetworkInitialized = false;
    public CrpgChatBox()
    {
        _queuedServerMessages = null;

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
            _queuedServerMessages?.Add(new QueuedMessageInfo(targetPlayer, message));
            return;
        }

        if (targetPlayer.IsServerPeer == false && targetPlayer.IsSynchronized)
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
            /*
            if (!targetPlayer.IsInBList || !targetPlayer.IsInCList)
            {
                GameNetwork.BeginModuleEventAsServer(targetPlayer);
                GameNetwork.WriteMessage(new CrpgServerMessage(color, message, false));
                GameNetwork.EndModuleEventAsServer();
            }
            */
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
            ChatCommandHandler.CheckForCommand(fromPeer, message.Substring(1));
        }
    }

    protected override void OnGameNetworkBegin()
    {
        _queuedServerMessages = new List<QueuedMessageInfo>();
        _isNetworkInitialized = true;
        AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);

#if CRPG_SERVER
        ChatCommandHandler.RegisterCommand(new PingCmd());
        ChatCommandHandler.RegisterCommand(new PlayerlistCmd());
        ChatCommandHandler.RegisterCommand(new KickCmd());
        ChatCommandHandler.RegisterCommand(new KillCmd());
        ChatCommandHandler.RegisterCommand(new TeleportCmd());
        ChatCommandHandler.RegisterCommand(new MuteCmd());
#endif

    }

    protected override void OnGameNetworkEnd()
    {
        base.OnGameNetworkEnd();
        AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
    }

    public override void OnBeforeSave()
    {
    }

    public override void OnAfterSave()
    {
    }

    protected override void OnTick(float dt)
    {
        if (GameNetwork.IsServer && _isNetworkInitialized)
        {
            for (int j = 0; j < _queuedServerMessages?.Count; j++)
            {
                QueuedMessageInfo queuedMessageInfo2 = _queuedServerMessages[j];
                if (queuedMessageInfo2.SourcePeer.IsSynchronized)
                {
                    ServerSendMessageToPlayer(queuedMessageInfo2.SourcePeer, queuedMessageInfo2.Message);
                    _queuedServerMessages.RemoveAt(j);
                }
                else if (queuedMessageInfo2.IsExpired)
                {
                    _queuedServerMessages.RemoveAt(j);
                }
            }
        }
    }

    private void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
    {
        GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new(mode);
        if (GameNetwork.IsClient)
        {
            networkMessageHandlerRegisterer.Register<CrpgServerMessage>(HandleCrpgServerMessage);
            return;
        }
    }

    private void HandleCrpgServerMessage(CrpgServerMessage message)
    {
        if (message.Message == null)
        {
            return;
        }

        string msg = message.IsMessageTextId ? GameTexts.FindText(message.Message, null).ToString() : message.Message;
        InformationManager.DisplayMessage(new InformationMessage(msg, new Color(message.Red, message.Green, message.Blue, message.Alpha)));
    }
}
