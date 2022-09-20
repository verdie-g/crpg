using Crpg.Module.Common.GameHandler;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands;
internal abstract class ChatCommand
{
    private string _command = string.Empty;
    protected class Pattern
    {
        public string Value { get; set; } = string.Empty;
        public delegate void CallFunc(NetworkCommunicator networkPeer, string cmd, List<object> parameters);
        public CallFunc? Function { get; private set; }

        public Pattern(string pattern, CallFunc function)
        {
            Value = pattern;
            Function = function;
        }
    }

    public string Command
    {
        get => _command;
        protected set => _command = value.ToLower();
    }

    protected List<Pattern> PatternList { get; set; }
    protected string Description { get; set; }

    public ChatCommand()
    {
        Command = string.Empty;
        PatternList = new Pattern[] { }.ToList();
        Description = $"Description here";
    }

    public virtual bool Execute(NetworkCommunicator fromPeer, string cmd, List<string> parameters)
    {
        if (!CheckRequirements(fromPeer))
        {
            return false;
        }

        foreach (Pattern pattern in PatternList)
        {
            var (succes, list) = Sscanf(parameters, pattern.Value);
            if (succes && list != null && pattern.Function != null)
            {
                pattern.Function(fromPeer, cmd, list);
                return true;
            }
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

    protected (bool success, List<object>? values) Sscanf(List<string> parameters, string pattern)
    {
        int formatLen = pattern.Length;
        int parameterLen = parameters.Count;
        List<object> parsedItems = new();
        if (formatLen == 0)
        {
            return (true, parsedItems);
        }

        if (parameterLen >= formatLen)
        {
            try
            {
                for (int i = 0; i < formatLen; i++)
                {
                    /*
                     * i & d = int
                     * f = float
                     * s = string
                     * p = player id
                     */
                    switch (pattern[i])
                    {
                        case 'p':
                            int id = int.Parse(parameters[i]);
                            bool found = false;
                            foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
                            {
                                if (networkPeer.IsSynchronized && networkPeer.Index == id)
                                {
                                    parsedItems.Add(networkPeer);
                                    found = true;
                                    break;
                                }
                            }

                            if (!found)
                            {
                                throw new Exception("No player found.");
                            }

                            break;
                        case 'i':
                        case 'd':
                            parsedItems.Add(int.Parse(parameters[i]));
                            break;
                        case 'f':
                            parsedItems.Add(float.Parse(parameters[i]));
                            break;
                        case 's':
                            if (i >= formatLen - 1)
                            {
                                string rest = parameters[formatLen - 1];
                                for (int j = formatLen; j < parameterLen; j++)
                                {
                                    rest += " " + parameters[j];
                                }

                                parsedItems.Add(rest);
                            }
                            else
                            {
                                parsedItems.Add(parameters[i]);
                            }

                            break;
                    }
                }
            }
            catch
            {
                return (false, null);
            }

            return (true, parsedItems);
        }

        return (false, null);
    }

    protected virtual (bool success, NetworkCommunicator? peer) GetPlayerByName(NetworkCommunicator fromPeer, string targetName)
    {
        CrpgChatBox crpgChat = GetChat();

        List<NetworkCommunicator> playerList = GetNetworkPeerByName(targetName);
        if (playerList.Count == 0)
        {
            crpgChat.ServerSendMessageToPlayer(fromPeer, new TaleWorlds.Library.Color(1, 1, 0), "No matching name found.");
            return (false, null);
        }

        if (playerList.Count > 1)
        {
            crpgChat.ServerSendMessageToPlayer(fromPeer, new TaleWorlds.Library.Color(1, 1, 0), "More than one match found. Please try the ID instead.");
            PrintPlayerList(fromPeer, playerList);
            return (false, null);
        }

        return (true, playerList[0]);
    }

    protected virtual CrpgChatBox GetChat()
    {
        return Game.Current.GetGameHandler<CrpgChatBox>();
    }

    protected List<NetworkCommunicator> GetNetworkPeerByName(string name)
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

    protected void PrintPlayerList (NetworkCommunicator fromPeer, List<NetworkCommunicator> peerList)
    {
        CrpgChatBox crpgChat = GetChat();
        crpgChat.ServerSendMessageToPlayer(fromPeer, ChatCommandHandler.ColorInfo, "- Players -");
        foreach (NetworkCommunicator networkPeer in peerList)
        {
            if (networkPeer.IsSynchronized)
            {
                crpgChat.ServerSendMessageToPlayer(fromPeer, ChatCommandHandler.ColorWarning, $"{networkPeer.Index} | '{networkPeer.UserName}'");
            }
        }
    }
}
