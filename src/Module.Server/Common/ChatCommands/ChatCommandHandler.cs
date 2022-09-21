using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Library;
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

        List<string> parameters = tmpParams.ToList();
        string lowerCaseCmd = parameters[0];
        parameters.RemoveAt(0);

        foreach (ChatCommand command in RegisteredCommands)
        {
            if (command.Command == lowerCaseCmd)
            {
                _ = HideChatInput(fromPeer);
                command.Execute(fromPeer, lowerCaseCmd, parameters);
                return true;
            }
        }

        return false;
    }

    public static async Task HideChatInput(NetworkCommunicator fromPeer)
    {
        bool muted = fromPeer.IsMuted;
        fromPeer.IsMuted = true;
        await Task.Delay(100);
        fromPeer.IsMuted = muted;
    }

    public static void RegisterCommand(ChatCommand cmd)
    {
        RegisteredCommands.Add(cmd);
    }
}
