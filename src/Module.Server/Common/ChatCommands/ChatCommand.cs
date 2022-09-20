using Crpg.Module.Common.GameHandler;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands;
internal abstract class ChatCommand
{
    private string _command = string.Empty;
    public string Command
    {
        get => _command;
        protected set => _command = value.ToLower();
    }

    protected List<string> Pattern { get; set; }
    protected string Description { get; set; }

    public ChatCommand()
    {
        Command = string.Empty;
        Pattern = new string[] { string.Empty }.ToList();
        Description = $"Description here";
    }

    public virtual bool Execute(NetworkCommunicator fromPeer, string cmd, List<string> parameters)
    {
        if (!CheckRequirements(fromPeer))
        {
            return false;
        }

        foreach (string pattern in Pattern)
        {
            var (succes, list) = Sscanf(parameters, pattern);
            if (succes && list != null)
            {
                ExecuteSuccess(fromPeer, cmd, list);
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

    // Parameters contains the parsed parameters
    protected abstract void ExecuteSuccess(NetworkCommunicator fromPeer, string cmd, List<object> parameters);

    // Called when the command fails.
    protected virtual void ExecuteFailed(NetworkCommunicator fromPeer)
    {
        // Example: Invalid usage.. please type !command ID Message
    }

    protected (bool succes, List<object>? values) Sscanf(List<string> parameters, string pattern)
    {
        int formatLen = pattern.Length;
        int parameterLen = pattern.Length;
        List<object> parsedItems = new();
        if (formatLen == 0 && parameterLen == 0)
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
                            foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
                            {
                                if (networkPeer.IsSynchronized && networkPeer.Index == id)
                                {
                                    parsedItems.Add(networkPeer);
                                    break;
                                }
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

    protected virtual CrpgChatBox GetChat()
    {
        return Game.Current.GetGameHandler<CrpgChatBox>();
    }
}
