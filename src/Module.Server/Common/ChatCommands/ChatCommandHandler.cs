using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands;
internal class ChatCommandHandler
{
    public static readonly char CommandPrefix = '!';
    private static readonly List<ChatCommand> RegisteredCommands = new();

    public ChatCommandHandler()
    {
    }

    public static bool CheckForCommand(NetworkCommunicator fromPeer, string cmd)
    {
        string[] tmpParams = cmd.Split(' ');
        if (tmpParams.Length == 0)
        {
            return false;
        }

        List<string> parameters = tmpParams.ToList();
        string lowerCaseCmd = parameters[0];
        parameters.RemoveAt(0);
        foreach (ChatCommand command in RegisteredCommands)
        {
            if (command.Command == lowerCaseCmd)
            {
                command.Execute(fromPeer, lowerCaseCmd, parameters);
                return true;
            }
        }

        return false;
    }

    public static void RegisterCommand(ChatCommand cmd)
    {
        RegisteredCommands.Add(cmd);
    }
}
