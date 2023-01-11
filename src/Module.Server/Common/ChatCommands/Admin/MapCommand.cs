using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.DedicatedCustomServer;

namespace Crpg.Module.Common.ChatCommands.Admin;

internal class MapCommand : AdminCommand
{
    protected override bool CheckRequirements(NetworkCommunicator fromPeer)
    {
        return base.CheckRequirements(fromPeer) || MultiplayerOptions.OptionType.GamePassword.GetStrValue() != null;
    }

    public MapCommand(ChatCommandsComponent chatComponent)
        : base(chatComponent)
    {
        Name = "map";
        Description = $"'{ChatCommandsComponent.CommandPrefix}{Name}' to change map.";
        Overloads = new CommandOverload[]
        {
            new(new[] { ChatCommandParameterType.String }, Execute),
        };
    }

    private void Execute(NetworkCommunicator fromPeer, object[] arguments)
    {
        string map = (string)arguments[0];

        var mapPoolComponent = Mission.Current?.GetMissionBehavior<MapPoolComponent>();
        if (mapPoolComponent == null)
        {
            return;
        }

        ChatComponent.ServerSendServerMessageToEveryone(ColorInfo, $"{fromPeer.UserName} is changing map to {map}");
        mapPoolComponent.ForceNextMap(map);
        DedicatedCustomServerSubModule.Instance.EndMission();
    }
}
