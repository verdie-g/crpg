using System.IO.Compression;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Api.Models.Users;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class UpdateCrpgUser : GameNetworkMessage
{
    public VirtualPlayer? Peer { get; set; }
    public CrpgUser User { get; set; } = default!;

    protected override void OnWrite()
    {
        WriteVirtualPlayerReferenceToPacket(Peer);
        WriteUserToPacket(User);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        Peer = ReadVirtualPlayerReferenceToPacket(ref bufferReadValid);
        User = ReadUserFromPacket(ref bufferReadValid);
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

    private void WriteUserToPacket(CrpgUser user)
    {
        using MemoryStream stream = new();

        // This packet is very large so it's compressed.
        using (GZipStream gZipStream = new(stream, CompressionMode.Compress, leaveOpen: true))
        using (BinaryWriter writer = new(gZipStream))
        {
            WriteCharacterToPacket(writer, user.Character);
            WriteClanMemberToPacket(writer, user.ClanMembership);
        }

        WriteByteArrayToPacket(stream.ToArray(), 0, (int)stream.Length);
    }

    private CrpgUser ReadUserFromPacket(ref bool bufferReadValid)
    {
        byte[] buffer = new byte[1024];
        int bufferLength = ReadByteArrayFromPacket(buffer, 0, buffer.Length, ref bufferReadValid);

        using MemoryStream stream = new(buffer, 0, bufferLength);
        using GZipStream gZipStream = new(stream, CompressionMode.Decompress);
        using BinaryReader reader = new(gZipStream);

        var character = ReadCharacterFromPacket(reader);
        var clanMember = ReadClanMemberFromPacket(reader);
        return new CrpgUser
        {
            Character = character,
            ClanMembership = clanMember,
        };
    }

    private void WriteCharacterToPacket(BinaryWriter writer, CrpgCharacter character)
    {
        writer.Write(character.Generation);
        writer.Write(character.Level);
        writer.Write(character.Experience);
        WriteCharacterCharacteristicsToPacket(writer, character.Characteristics);
        WriteCharacterEquippedItemsToPacket(writer, character.EquippedItems);
    }

    private CrpgCharacter ReadCharacterFromPacket(BinaryReader reader)
    {
        int generation = reader.ReadInt32();
        int level = reader.ReadInt32();
        int exp = reader.ReadInt32();
        var characteristics = ReadCharacterCharacteristicsFromPacket(reader);
        var equippedItems = ReadCharacterEquippedItemsFromPacket(reader);
        return new CrpgCharacter
        {
            Generation = generation,
            Level = level,
            Experience = exp,
            Characteristics = characteristics,
            EquippedItems = equippedItems,
        };
    }

    private void WriteCharacterCharacteristicsToPacket(BinaryWriter writer, CrpgCharacterCharacteristics characteristics)
    {
        writer.Write((short)characteristics.Attributes.Points);
        writer.Write((short)characteristics.Attributes.Strength);
        writer.Write((short)characteristics.Attributes.Agility);

        writer.Write((short)characteristics.Skills.Points);
        writer.Write((short)characteristics.Skills.IronFlesh);
        writer.Write((short)characteristics.Skills.PowerStrike);
        writer.Write((short)characteristics.Skills.PowerDraw);
        writer.Write((short)characteristics.Skills.PowerThrow);
        writer.Write((short)characteristics.Skills.Athletics);
        writer.Write((short)characteristics.Skills.Riding);
        writer.Write((short)characteristics.Skills.WeaponMaster);
        writer.Write((short)characteristics.Skills.MountedArchery);
        writer.Write((short)characteristics.Skills.Shield);

        writer.Write((short)characteristics.WeaponProficiencies.Points);
        writer.Write((short)characteristics.WeaponProficiencies.OneHanded);
        writer.Write((short)characteristics.WeaponProficiencies.TwoHanded);
        writer.Write((short)characteristics.WeaponProficiencies.Polearm);
        writer.Write((short)characteristics.WeaponProficiencies.Bow);
        writer.Write((short)characteristics.WeaponProficiencies.Throwing);
        writer.Write((short)characteristics.WeaponProficiencies.Crossbow);
    }

    private CrpgCharacterCharacteristics ReadCharacterCharacteristicsFromPacket(BinaryReader reader)
    {
        return new CrpgCharacterCharacteristics
        {
            Attributes =
            {
                Points = reader.ReadInt16(),
                Strength = reader.ReadInt16(),
                Agility = reader.ReadInt16(),
            },
            Skills =
            {
                Points = reader.ReadInt16(),
                IronFlesh = reader.ReadInt16(),
                PowerStrike = reader.ReadInt16(),
                PowerDraw = reader.ReadInt16(),
                PowerThrow = reader.ReadInt16(),
                Athletics = reader.ReadInt16(),
                Riding = reader.ReadInt16(),
                WeaponMaster = reader.ReadInt16(),
                MountedArchery = reader.ReadInt16(),
                Shield = reader.ReadInt16(),
            },
            WeaponProficiencies =
            {
                Points = reader.ReadInt16(),
                OneHanded = reader.ReadInt16(),
                TwoHanded = reader.ReadInt16(),
                Polearm = reader.ReadInt16(),
                Bow = reader.ReadInt16(),
                Throwing = reader.ReadInt16(),
                Crossbow = reader.ReadInt16(),
            },
        };
    }

    private void WriteCharacterEquippedItemsToPacket(BinaryWriter writer, IList<CrpgEquippedItem> equippedItems)
    {
        writer.Write(equippedItems.Count);
        foreach (var equippedItem in equippedItems)
        {
            // Use the internal id of the item that is smaller that the item id.
            var itemObject = MBObjectManager.Instance.GetObject<ItemObject>(equippedItem.UserItem.ItemId);
            writer.Write(itemObject != null ? itemObject.Id.InternalValue : 0);
            writer.Write((byte)equippedItem.Slot);
        }
    }

    private IList<CrpgEquippedItem> ReadCharacterEquippedItemsFromPacket(BinaryReader reader)
    {
        int equippedItemsLength = reader.ReadInt32();
        List<CrpgEquippedItem> equippedItems = new(equippedItemsLength);
        for (int i = 0; i < equippedItemsLength; i += 1)
        {
            uint internalItemId = reader.ReadUInt32();
            CrpgItemSlot slot = (CrpgItemSlot)reader.ReadByte();

            var itemObject = internalItemId == 0 ? null : MBObjectManager.Instance.GetObject(new MBGUID(internalItemId));
            if (itemObject != null)
            {
                equippedItems.Add(new CrpgEquippedItem
                {
                    UserItem = new CrpgUserItem
                    {
                        Id = 0,
                        ItemId = ((ItemObject)itemObject).StringId,
                    }, Slot = slot,
                });
            }
        }

        return equippedItems;
    }

    private void WriteClanMemberToPacket(BinaryWriter writer, CrpgClanMember? clanMember)
    {
        writer.Write(clanMember?.ClanId ?? -1);
    }

    private CrpgClanMember? ReadClanMemberFromPacket(BinaryReader reader)
    {
        int clanId = reader.ReadInt32();
        return clanId != -1 ? new CrpgClanMember { ClanId = clanId } : null;
    }
}
