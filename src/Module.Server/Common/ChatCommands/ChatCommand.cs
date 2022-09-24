using Crpg.Module.Common.GameHandler;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands;

internal abstract class ChatCommand
{
    protected static readonly Color ColorInfo = new(1f, 0.65f, 0f);
    protected static readonly Color ColorWarning = new(1f, 1f, 0f);
    protected static readonly Color ColorSuccess = new(0f, 1f, 0f);
    protected static readonly Color ColorFatal = new(1f, 0f, 0f);

    public string Name { get; protected set; } = string.Empty;

    /// <summary>A command can accepts several arguments types, the first one that matches the input is used.</summary>
    protected CommandOverload[] Overloads { get; set; } = Array.Empty<CommandOverload>();
    protected string Description { get; set; } = string.Empty;

    public bool Execute(NetworkCommunicator fromPeer, string commandName, string[] arguments)
    {
        if (!CheckRequirements(fromPeer))
        {
            GetChat().ServerSendMessageToPlayer(fromPeer, ColorInfo, $"Insufficient permissions.");
            return false;
        }

        foreach (CommandOverload overload in Overloads)
        {
            if (!TryParseArguments(arguments, overload.ParameterTypes, out object[]? parsedArguments))
            {
                continue;
            }

            overload.Execute(fromPeer, commandName, parsedArguments!);
            return true;
        }

        ExecuteFailed(fromPeer);
        return false;
    }

    // Used to check for permissions
    protected virtual bool CheckRequirements(NetworkCommunicator fromPeer)
    {
        // Check requirements (is Clan leader / Admin rank etc)
        return true;
    }

    // Called when the command fails.
    protected virtual void ExecuteFailed(NetworkCommunicator fromPeer)
    {
        // Example: Invalid usage.. please type !command ID Message
    }

    protected bool TryGetPlayerByName(NetworkCommunicator fromPeer, string targetName, out NetworkCommunicator? peer)
    {
        CrpgChatBox crpgChat = GetChat();

        List<NetworkCommunicator> peers = GetNetworkPeerByName(targetName);
        if (peers.Count == 0)
        {
            crpgChat.ServerSendMessageToPlayer(fromPeer, ColorFatal, "No matching name found.");
            peer = null;
            return false;
        }

        if (peers.Count > 1)
        {
            crpgChat.ServerSendMessageToPlayer(fromPeer, ColorWarning, "More than one match found. Please try the ID instead.");
            PrintPlayerList(fromPeer, peers);
            peer = null;
            return false;
        }

        peer = peers[0];
        return true;
    }

    protected CrpgChatBox GetChat()
    {
        return Game.Current.GetGameHandler<CrpgChatBox>();
    }

    protected void PrintPlayerList(NetworkCommunicator fromPeer, List<NetworkCommunicator> peerList)
    {
        CrpgChatBox crpgChat = GetChat();
        crpgChat.ServerSendMessageToPlayer(fromPeer, ColorInfo, "- Players -");
        foreach (NetworkCommunicator networkPeer in peerList)
        {
            var crpgRepresentative = networkPeer.GetComponent<CrpgRepresentative>();
            if (networkPeer.IsSynchronized && crpgRepresentative.User != null)
            {
                crpgChat.ServerSendMessageToPlayer(fromPeer, ColorWarning, $"{crpgRepresentative.User.Id} | '{networkPeer.UserName}'");
            }
        }
    }

    private List<NetworkCommunicator> GetNetworkPeerByName(string name)
    {
        List<NetworkCommunicator> peerList = new();
        if (GameNetwork.NetworkPeers == null)
        {
            return peerList;
        }

        name = name.ToLower().Trim().Replace(" ", string.Empty);
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            if (networkPeer.UserName.ToLower().Trim().Replace(" ", string.Empty).Contains(name))
            {
                peerList.Add(networkPeer);
            }
        }

        return peerList;
    }

    private bool TryParseArguments(string[] arguments, ChatCommandParameterType[] expectedTypes, out object[]? parsedArguments)
    {
        parsedArguments = new object[expectedTypes.Length];
        if (arguments.Length < expectedTypes.Length)
        {
            return false;
        }

        for (int i = 0; i < expectedTypes.Length; i++)
        {
            switch (expectedTypes[i])
            {
                case ChatCommandParameterType.Int32:
                    if (!int.TryParse(arguments[i], out int parsedInt))
                    {
                        return false;
                    }

                    parsedArguments[i] = parsedInt;
                    break;
                case ChatCommandParameterType.Float32:
                    if (!float.TryParse(arguments[i], out float parsedFloat))
                    {
                        return false;
                    }

                    parsedArguments[i] = parsedFloat;
                    break;
                case ChatCommandParameterType.String:
                    parsedArguments[i] = i < expectedTypes.Length - 1
                        ? arguments[i]
                        : string.Join(" ", arguments.Skip(i));

                    break;
                case ChatCommandParameterType.PlayerId:
                    if (!int.TryParse(arguments[i], out int id))
                    {
                        return false;
                    }

                    bool playerFound = false;
                    foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
                    {
                        var crpgRepresentative = networkPeer.GetComponent<CrpgRepresentative>();
                        if (networkPeer.IsSynchronized && crpgRepresentative.User?.Id == id)
                        {
                            parsedArguments[i] = networkPeer;
                            playerFound = true;
                            break;
                        }
                    }

                    if (playerFound)
                    {
                        break;
                    }

                    return false;
            }
        }

        return true;
    }

    protected enum ChatCommandParameterType
    {
        Int32,
        Float32,
        String,
        PlayerId,
    }

    protected class CommandOverload
    {
        public ChatCommandParameterType[] ParameterTypes { get; }
        public CommandFunc Execute { get; }

        public CommandOverload(ChatCommandParameterType[] parameterTypes, CommandFunc execute)
        {
            ParameterTypes = parameterTypes;
            Execute = execute;
        }

        public delegate void CommandFunc(NetworkCommunicator networkPeer, string cmd, object[] parameters);
    }
}
