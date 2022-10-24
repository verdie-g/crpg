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
    private static readonly CompressionInfo.Integer RoleCompressionInfo = new(0, 3, true);
    private static readonly CompressionInfo.Integer LevelCompressionInfo = new(0, 50, true);
    private static readonly CompressionInfo.Integer SkillCompressionInfo = new(0, 16384, true);
    public CrpgUser User { get; set; } = default!;

    protected override void OnWrite()
    {
        WriteIntToPacket((int)User.Role, RoleCompressionInfo);
        WriteIntToPacket(User.Character.Generation, GenerationCompressionInfo);
        WriteIntToPacket(User.Character.Level, LevelCompressionInfo);
        WriteIntToPacket(User.Character.Experience, ExperienceCompressionInfo);
        WriteCharacterCharacteristics(User.Character.Characteristics);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        User = new CrpgUser
        {
            Role = (CrpgUserRole)ReadIntFromPacket(RoleCompressionInfo, ref bufferReadValid),
            Character = new CrpgCharacter
            {
                Generation = ReadIntFromPacket(GenerationCompressionInfo, ref bufferReadValid),
                Level = ReadIntFromPacket(LevelCompressionInfo, ref bufferReadValid),
                Experience = ReadIntFromPacket(ExperienceCompressionInfo, ref bufferReadValid),
                Characteristics = ReadCharacterCharacteristics(ref bufferReadValid),
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

    private void WriteCharacterCharacteristics(CrpgCharacterCharacteristics characteristics)
    {
        WriteIntToPacket(characteristics.Attributes.Points, SkillCompressionInfo);
        WriteIntToPacket(characteristics.Attributes.Strength, SkillCompressionInfo);
        WriteIntToPacket(characteristics.Attributes.Agility, SkillCompressionInfo);

        WriteIntToPacket(characteristics.Skills.Points, SkillCompressionInfo);
        WriteIntToPacket(characteristics.Skills.IronFlesh, SkillCompressionInfo);
        WriteIntToPacket(characteristics.Skills.PowerStrike, SkillCompressionInfo);
        WriteIntToPacket(characteristics.Skills.PowerDraw, SkillCompressionInfo);
        WriteIntToPacket(characteristics.Skills.PowerThrow, SkillCompressionInfo);
        WriteIntToPacket(characteristics.Skills.Athletics, SkillCompressionInfo);
        WriteIntToPacket(characteristics.Skills.Riding, SkillCompressionInfo);
        WriteIntToPacket(characteristics.Skills.WeaponMaster, SkillCompressionInfo);
        WriteIntToPacket(characteristics.Skills.MountedArchery, SkillCompressionInfo);
        WriteIntToPacket(characteristics.Skills.Shield, SkillCompressionInfo);

        WriteIntToPacket(characteristics.WeaponProficiencies.Points, SkillCompressionInfo);
        WriteIntToPacket(characteristics.WeaponProficiencies.OneHanded, SkillCompressionInfo);
        WriteIntToPacket(characteristics.WeaponProficiencies.TwoHanded, SkillCompressionInfo);
        WriteIntToPacket(characteristics.WeaponProficiencies.Polearm, SkillCompressionInfo);
        WriteIntToPacket(characteristics.WeaponProficiencies.Bow, SkillCompressionInfo);
        WriteIntToPacket(characteristics.WeaponProficiencies.Throwing, SkillCompressionInfo);
        WriteIntToPacket(characteristics.WeaponProficiencies.Crossbow, SkillCompressionInfo);
    }

    private CrpgCharacterCharacteristics ReadCharacterCharacteristics(ref bool bufferReadValid)
    {
        return new CrpgCharacterCharacteristics
        {
            Attributes =
            {
                Points = ReadIntFromPacket(SkillCompressionInfo, ref bufferReadValid),
                Strength = ReadIntFromPacket(SkillCompressionInfo, ref bufferReadValid),
                Agility = ReadIntFromPacket(SkillCompressionInfo, ref bufferReadValid),
            },
            Skills =
            {
                Points = ReadIntFromPacket(SkillCompressionInfo, ref bufferReadValid),
                IronFlesh = ReadIntFromPacket(SkillCompressionInfo, ref bufferReadValid),
                PowerStrike = ReadIntFromPacket(SkillCompressionInfo, ref bufferReadValid),
                PowerDraw = ReadIntFromPacket(SkillCompressionInfo, ref bufferReadValid),
                PowerThrow = ReadIntFromPacket(SkillCompressionInfo, ref bufferReadValid),
                Athletics = ReadIntFromPacket(SkillCompressionInfo, ref bufferReadValid),
                Riding = ReadIntFromPacket(SkillCompressionInfo, ref bufferReadValid),
                WeaponMaster = ReadIntFromPacket(SkillCompressionInfo, ref bufferReadValid),
                MountedArchery = ReadIntFromPacket(SkillCompressionInfo, ref bufferReadValid),
                Shield = ReadIntFromPacket(SkillCompressionInfo, ref bufferReadValid),
            },
            WeaponProficiencies =
            {
                Points = ReadIntFromPacket(SkillCompressionInfo, ref bufferReadValid),
                OneHanded = ReadIntFromPacket(SkillCompressionInfo, ref bufferReadValid),
                TwoHanded = ReadIntFromPacket(SkillCompressionInfo, ref bufferReadValid),
                Polearm = ReadIntFromPacket(SkillCompressionInfo, ref bufferReadValid),
                Bow = ReadIntFromPacket(SkillCompressionInfo, ref bufferReadValid),
                Throwing = ReadIntFromPacket(SkillCompressionInfo, ref bufferReadValid),
                Crossbow = ReadIntFromPacket(SkillCompressionInfo, ref bufferReadValid),
            },
        };
    }
}
