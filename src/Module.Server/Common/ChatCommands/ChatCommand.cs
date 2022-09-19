﻿using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands;
internal abstract class ChatCommand
{
    public string Command { get; private set; }
    public List<string> Pattern { get; private set; }
    private readonly string description;

    public ChatCommand(string command, List<string> pattern)
    {
        Command = command.ToLower();
        Pattern = pattern;
        description = $"Description here";
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
    protected virtual void ExecuteSuccess(NetworkCommunicator fromPeer, string cmd, List<dynamic> parameters)
    {
        // Do stuff in here
    }

    // Called when the command fails.
    protected virtual void ExecuteFailed(NetworkCommunicator fromPeer)
    {
        // Example: Invalid usage.. please type !command ID Message
    }

    protected (bool succes, List<dynamic>? values) Sscanf(List<string> parameters, string pattern)
    {
        int formatLen = pattern.Length;
        int parameterLen = pattern.Length;
        List<dynamic> parsedItems = new();
        if (parameterLen >= formatLen)
        {
            try
            {
                for (int i = 0; i < formatLen; i++)
                {
                    switch (pattern[i])
                    {
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
}
