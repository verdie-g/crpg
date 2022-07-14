using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Users;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class UpdateCrpgUser : GameNetworkMessage
{
    private static readonly CompressionInfo.Integer GenerationCompressionInfo = new(0, 100, true);
    private static readonly CompressionInfo.Integer ExperienceCompressionInfo = new(0, int.MaxValue, true);
    private static readonly CompressionInfo.Integer LevelCompressionInfo = new(0, 50, true);
    public CrpgUser User { get; set; } = default!;

    protected override void OnWrite()
    {
        WriteIntToPacket(User.Character.Generation, GenerationCompressionInfo);
        WriteIntToPacket(User.Character.Level, LevelCompressionInfo);
        WriteIntToPacket(User.Character.Experience, ExperienceCompressionInfo);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        int generation = ReadIntFromPacket(GenerationCompressionInfo, ref bufferReadValid);
        int level = ReadIntFromPacket(LevelCompressionInfo, ref bufferReadValid);
        int experience = ReadIntFromPacket(ExperienceCompressionInfo, ref bufferReadValid);
        User = new CrpgUser
        {
            Character = new CrpgCharacter
            {
                Generation = generation,
                Level = level,
                Experience = experience,
            },
        };
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "Update cRPG User";
    }
}
