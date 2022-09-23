using Crpg.Module.Common.ChatCommands.Admin;
using Crpg.Module.Common.ChatCommands.User;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands;

internal class ChatCommandHandler
{
    public static readonly char CommandPrefix = '!';

    private static readonly ChatCommand[] Commands =
    {
        new PingCommand(),
        new PlayerListCommand(),
        new KickCommand(),
        new KillCommand(),
        new TeleportCommand(),
        new MuteCommand(),
    };

    public static bool TryExecuteCommand(NetworkCommunicator fromPeer, string input)
    {
        string[] tokens = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
        {
            return false;
        }

        string name = tokens[0].ToLowerInvariant();
        var command = Commands.FirstOrDefault(c => c.Name == name);
        if (command == null)
        {
            return false;
        }

        _ = HideChatInput(fromPeer);
        command.Execute(fromPeer, name, tokens.Skip(1).ToArray());
        return true;
    }

    // Hacky workaround until we can actually control which message should be sent to everyone.
    private static async Task HideChatInput(NetworkCommunicator fromPeer)
    {
        bool muted = fromPeer.IsMuted;
        fromPeer.IsMuted = true;
        await Task.Delay(100);
        fromPeer.IsMuted = muted;
    }
}
