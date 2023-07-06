using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands;

internal abstract class ChatCommand
{
    protected static readonly Color ColorInfo = new(1f, 0.65f, 0f);
    protected static readonly Color ColorWarning = new(1f, 1f, 0f);
    protected static readonly Color ColorSuccess = new(0f, 1f, 0f);
    protected static readonly Color ColorFatal = new(1f, 0f, 0f);

#pragma warning disable SA1401 // False positive
    protected readonly ChatCommandsComponent ChatComponent;
#pragma warning restore SA1401

    public string Name { get; protected set; } = string.Empty;

    protected ChatCommand(ChatCommandsComponent chatComponent)
    {
        ChatComponent = chatComponent;
    }

    /// <summary>A command can accepts several arguments types, the first one that matches the input is used.</summary>
    protected CommandOverload[] Overloads { get; set; } = Array.Empty<CommandOverload>();
    protected string Description { get; set; } = string.Empty;

    public void Execute(NetworkCommunicator fromPeer, string[] arguments)
    {
        if (!CheckRequirements(fromPeer))
        {
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorInfo, "Insufficient permissions.");
            return;
        }

        foreach (CommandOverload overload in Overloads)
        {
            if (!TryParseArguments(arguments, overload.ParameterTypes, out object[]? parsedArguments))
            {
                continue;
            }

            overload.Execute(fromPeer, parsedArguments!);
            return;
        }

        ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorInfo, $"Wrong usage. Type {Description}");
    }

    // Used to check for permissions
    protected virtual bool CheckRequirements(NetworkCommunicator fromPeer)
    {
        // Check requirements (is Clan leader / Admin rank etc)
        return true;
    }

    protected bool TryGetPlayerByName(NetworkCommunicator fromPeer, string targetName, out NetworkCommunicator? peer)
    {
        List<NetworkCommunicator> peers = GetNetworkPeerByName(targetName);
        if (peers.Count == 0)
        {
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorFatal, "No matching name found.");
            peer = null;
            return false;
        }

        if (peers.Count > 1)
        {
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorWarning, "More than one match found. Please try the ID instead.");
            peer = null;
            return false;
        }

        peer = peers[0];
        return true;
    }

    protected string FormatTimeSpan(TimeSpan t)
    {
        return string.Format("{0:%d} days {0:%h} hours {0:%m} minutes", t);
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

                case ChatCommandParameterType.TimeSpan:
                    if (!TryParseTimeSpan(arguments[i], out var timeSpan))
                    {
                        return false;
                    }

                    parsedArguments[i] = timeSpan;
                    break;

                case ChatCommandParameterType.PlayerId:
                    if (!TryParsePlayerId(arguments[i], out var networkPeer))
                    {
                        return false;
                    }

                    parsedArguments[i] = networkPeer!;
                    break;
            }
        }

        return true;
    }

    private bool TryParsePlayerId(string input, out NetworkCommunicator? networkPeer)
    {
        if (!int.TryParse(input, out int id))
        {
            networkPeer = null;
            return false;
        }

        foreach (NetworkCommunicator p in GameNetwork.NetworkPeers)
        {
            var crpgPeer = p.GetComponent<CrpgPeer>();
            if (p.IsSynchronized && crpgPeer.User?.Id == id)
            {
                networkPeer = p;
                return true;
            }
        }

        networkPeer = null;
        return false;
    }

    /// <summary>Parses input such as "15m". Unit supported s, m, h, d.</summary>
    private bool TryParseTimeSpan(string input, out TimeSpan timeSpan)
    {
        timeSpan = TimeSpan.Zero;
        if (input == "0")
        {
            return false;
        }

        if (input.Length < 2) // At least one number and a unit.
        {
            return false;
        }

        if (!int.TryParse(input.Substring(0, input.Length - 1), out int inputInt))
        {
            return false;
        }

        switch (input[input.Length - 1])
        {
            case 's':
                timeSpan = TimeSpan.FromSeconds(inputInt);
                break;
            case 'm':
                timeSpan = TimeSpan.FromMinutes(inputInt);
                break;
            case 'h':
                timeSpan = TimeSpan.FromHours(inputInt);
                break;
            case 'd':
                timeSpan = TimeSpan.FromDays(inputInt);
                break;
            default:
                return false;
        }

        return true;
    }

    protected enum ChatCommandParameterType
    {
        Int32,
        Float32,
        String,
        TimeSpan,
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

        public delegate void CommandFunc(NetworkCommunicator networkPeer, object[] parameters);
    }
}
