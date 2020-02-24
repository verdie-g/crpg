using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common;
using MediatR;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities;

namespace Crpg.Application.System.Commands
{
    public class SeedDataCommand : IRequest
    {
        public class Handler : IRequestHandler<SeedDataCommand>
        {
            private readonly ICrpgDbContext _db;

            public Handler(ICrpgDbContext db)
            {
                _db = db;
            }

            public async Task<Unit> Handle(SeedDataCommand request, CancellationToken cancellationToken)
            {
                _db.Items.AddRange(new Item
                    {
                        MbId = "mp_pointed_skullcap_over_cloth_headwrap",
                        Name = "Pointed Skullcap Over Cloth Headwrap",
                        Image = new Uri(
                            "https://via.placeholder.com/256x120?text=mp_pointed_skullcap_over_cloth_headwrap"),
                        Value = 69,
                        Type = ItemType.HeadArmor,
                        Weight = 1.1f,
                        HeadArmor = 16,
                    },
                    new Item
                    {
                        MbId = "mp_aserai_civil_c_head",
                        Name = "Kefiyyeh With Silken Band",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_aserai_civil_c_head"),
                        Value = 63,
                        Type = ItemType.HeadArmor,
                        Weight = 0.1f,
                        HeadArmor = 2,
                    },
                    new Item
                    {
                        MbId = "mp_aserai_civil_d_hscarf",
                        Name = "Colored Turban",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_aserai_civil_d_hscarf"),
                        Value = 97,
                        Type = ItemType.HeadArmor,
                        Weight = 0.1f,
                        HeadArmor = 4,
                    },
                    new Item
                    {
                        MbId = "mp_brass_scale_shoulders",
                        Name = "Brass Scale Shoulders",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_brass_scale_shoulders"),
                        Value = 875,
                        Type = ItemType.Cape,
                        Weight = 2.2f,
                        BodyArmor = 12,
                    },
                    new Item
                    {
                        MbId = "mp_desert_fabric_shoulderpad",
                        Name = "Desert Fabric Shoulderpad",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_desert_fabric_shoulderpad"),
                        Value = 57,
                        Type = ItemType.Cape,
                        Weight = 1.8f,
                        BodyArmor = 8,
                    },
                    new Item
                    {
                        MbId = "mp_rough_bearskin",
                        Name = "Rough Bearskin",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_rough_bearskin"),
                        Value = 352,
                        Type = ItemType.Cape,
                        Weight = 2.3f,
                        HeadArmor = 0,
                        BodyArmor = 7,
                        ArmArmor = 0,
                        LegArmor = 0,
                    },
                    new Item
                    {
                        MbId = "mp_southern_lamellar_armor",
                        Name = "Southern Lamellar Armor",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_southern_lamellar_armor"),
                        Value = 2180,
                        Type = ItemType.BodyArmor,
                        Weight = 8.8f,
                        HeadArmor = 0,
                        BodyArmor = 25,
                        ArmArmor = 1,
                        LegArmor = 4,
                    },
                    new Item
                    {
                        MbId = "mp_tassled_southern_robes",
                        Name = "Tassled Southern Robes",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_tassled_southern_robes"),
                        Value = 78,
                        Type = ItemType.BodyArmor,
                        Weight = 0.9f,
                        HeadArmor = 0,
                        BodyArmor = 3,
                        ArmArmor = 2,
                        LegArmor = 2,
                    },
                    new Item
                    {
                        MbId = "mp_kilt_over_plated_leather",
                        Name = "Kilt Over Plated Leather",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_kilt_over_plated_leather"),
                        Value = 8759,
                        Type = ItemType.BodyArmor,
                        Weight = 7.8f,
                        HeadArmor = 0,
                        BodyArmor = 30,
                        ArmArmor = 2,
                        LegArmor = 16,
                    },
                    new Item
                    {
                        MbId = "mp_strapped_leather_bracers",
                        Name = "Strapped Leather Bracers",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_strapped_leather_bracers"),
                        Value = 41,
                        Type = ItemType.HandArmor,
                        Weight = 0.5f,
                        ArmArmor = 3,
                    },
                    new Item
                    {
                        MbId = "mp_reinforced_leather_vambraces",
                        Name = "Reinforced Leather Vambraces",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_reinforced_leather_vambraces"),
                        Value = 157,
                        Type = ItemType.HandArmor,
                        Weight = 0.3f,
                        ArmArmor = 11,
                    },
                    new Item
                    {
                        MbId = "mp_buttoned_leather_bracers",
                        Name = "Buttoned Leather Bracers",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_buttoned_leather_bracers"),
                        Value = 84,
                        Type = ItemType.HandArmor,
                        Weight = 0.5f,
                        ArmArmor = 5,
                    },
                    new Item
                    {
                        MbId = "mp_ragged_boots",
                        Name = "Ragged Boots",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_ragged_boots"),
                        Value = 66,
                        Type = ItemType.LegArmor,
                        Weight = 0.8f,
                        LegArmor = 8,
                    },
                    new Item
                    {
                        MbId = "mp_folded_town_boots",
                        Name = "Folded Town Boots",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_folded_town_boots"),
                        Value = 139,
                        Type = ItemType.LegArmor,
                        Weight = 1f,
                        LegArmor = 7,
                    },
                    new Item
                    {
                        MbId = "mp_strapped_shoes",
                        Name = "Strapped Shoes",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_strapped_shoes"),
                        Value = 74,
                        Type = ItemType.LegArmor,
                        Weight = 0.7f,
                        LegArmor = 4,
                    },
                    new Item
                    {
                        MbId = "mp_light_harness",
                        Name = "Light Harness",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_light_harness"),
                        Value = 84,
                        Type = ItemType.HorseHarness,
                        Weight = 30f,
                        BodyArmor = 4,
                    },
                    new Item
                    {
                        MbId = "mp_chain_barding",
                        Name = "Chain Barding",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_chain_barding"),
                        Value = 31947,
                        Type = ItemType.HorseHarness,
                        Weight = 130f,
                        BodyArmor = 49,
                    },
                    new Item
                    {
                        MbId = "mp_steppe_fur_harness",
                        Name = "Steppe Fur Harness",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_steppe_fur_harness"),
                        Value = 84,
                        Type = ItemType.HorseHarness,
                        Weight = 40f,
                        BodyArmor = 4,
                    },
                    new Item
                    {
                        MbId = "mp_vlandia_horse",
                        Name = "Hunter",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_vlandia_horse"),
                        Value = 100,
                        Type = ItemType.Horse,
                        Weight = 400f,
                        BodyLength = 103,
                        ChargeDamage = 2,
                        Maneuver = 62,
                        Speed = 43,
                        HitPoints = 0,
                    },
                    new Item
                    {
                        MbId = "mp_vlandia_horse_agile",
                        Name = "Jaculan Horse",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_vlandia_horse_agile"),
                        Value = 100,
                        Type = ItemType.Horse,
                        Weight = 395f,
                        BodyLength = 103,
                        ChargeDamage = 2,
                        Maneuver = 68,
                        Speed = 45,
                        HitPoints = 0,
                    },
                    new Item
                    {
                        MbId = "mp_vlandia_horse_war",
                        Name = "Charger",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_vlandia_horse_war"),
                        Value = 100,
                        Type = ItemType.Horse,
                        Weight = 400f,
                        BodyLength = 113,
                        ChargeDamage = 13,
                        Maneuver = 65,
                        Speed = 40,
                        HitPoints = 0,
                    },
                    new Item
                    {
                        MbId = "mp_strapped_round_shield",
                        Name = "Reinforced Large Round Shield",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_strapped_round_shield"),
                        Value = 78,
                        Type = ItemType.Shield,
                        Weight = 4.7f,
                        StackAmount = 170,
                        WeaponLength = 70,
                        PrimaryThrustDamage = 5,
                        PrimaryThrustSpeed = 82,
                        PrimarySwingDamage = 0,
                        PrimarySwingSpeed = 82,
                        PrimaryWeaponFlags = (WeaponFlags?) 268435968,
                    },
                    new Item
                    {
                        MbId = "mp_noyans_shield",
                        Name = "Decorated Eastern Shield",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_noyans_shield"),
                        Value = 134,
                        Type = ItemType.Shield,
                        Weight = 4.7f,
                        StackAmount = 120,
                        WeaponLength = 60,
                        PrimaryThrustDamage = 0,
                        PrimaryThrustSpeed = 82,
                        PrimarySwingDamage = 0,
                        PrimarySwingSpeed = 82,
                        PrimaryWeaponFlags = (WeaponFlags?) 268435968,
                    },
                    new Item
                    {
                        MbId = "mp_wide_heater_shield",
                        Name = "Wide Heater Shield",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_wide_heater_shield"),
                        Value = 119,
                        Type = ItemType.Shield,
                        Weight = 4.7f,
                        StackAmount = 160,
                        WeaponLength = 90,
                        PrimaryThrustDamage = 0,
                        PrimaryThrustSpeed = 82,
                        PrimarySwingDamage = 0,
                        PrimarySwingSpeed = 82,
                        PrimaryWeaponFlags = (WeaponFlags?) 268435968,
                    },
                    new Item
                    {
                        MbId = "mp_hunting_bow",
                        Name = "Hunting Bow",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_hunting_bow"),
                        Value = 227,
                        Type = ItemType.Bow,
                        Weight = 1f,
                        Accuracy = 89,
                        MissileSpeed = 64,
                        WeaponLength = 95,
                        PrimaryThrustDamage = 42,
                        PrimaryThrustSpeed = 110,
                        PrimaryWeaponFlags = (WeaponFlags?) 2628626,
                    },
                    new Item
                    {
                        MbId = "mp_crossbow",
                        Name = "Crossbow",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_crossbow"),
                        Value = 48500,
                        Type = ItemType.Crossbow,
                        Weight = 2.2f,
                        Accuracy = 98,
                        MissileSpeed = 82,
                        WeaponLength = 95,
                        PrimaryThrustDamage = 85,
                        PrimaryThrustSpeed = 85,
                        PrimaryWeaponFlags = (WeaponFlags?) 2360338,
                    },
                    new Item
                    {
                        MbId = "mp_hatchet_axe",
                        Name = "Hatchet",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_hatchet_axe"),
                        Value = 150,
                        Type = ItemType.OneHandedWeapon,
                        Weight = 1.54f,
                        WeaponLength = 48,
                        PrimarySwingDamage = 66,
                        PrimarySwingSpeed = 99,
                        PrimaryWeaponFlags = (WeaponFlags?) 65537,
                    },
                    new Item
                    {
                        MbId = "mp_vlandian_billhook",
                        Name = "Billhook",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_vlandian_billhook"),
                        Value = 150,
                        Type = ItemType.TwoHandedWeapon,
                        Weight = 1.08f,
                        WeaponLength = 140,
                        PrimaryThrustDamage = 16,
                        PrimaryThrustSpeed = 100,
                        PrimarySwingDamage = 90,
                        PrimarySwingSpeed = 90,
                        PrimaryWeaponFlags = (WeaponFlags?) 2162705,
                    },
                    new Item
                    {
                        MbId = "mp_vlandian_polearm",
                        Name = "Voulge",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_vlandian_polearm"),
                        Value = 150,
                        Type = ItemType.Polearm,
                        Weight = 0.74f,
                        WeaponLength = 136,
                        PrimaryThrustDamage = 30,
                        PrimaryThrustSpeed = 103,
                        PrimarySwingDamage = 113,
                        PrimarySwingSpeed = 85,
                        PrimaryWeaponFlags = (WeaponFlags?) 2097233,
                    },
                    new Item
                    {
                        MbId = "mp_battania_throwing_knife",
                        Name = "Highland ThrowingKnife",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_battania_throwing_knife"),
                        Value = 450,
                        Type = ItemType.Thrown,
                        Weight = 0.6f,
                        Accuracy = 95,
                        MissileSpeed = 33,
                        StackAmount = 5,
                        WeaponLength = 43,
                        PrimaryThrustDamage = 18,
                        PrimaryThrustSpeed = 101,
                        PrimarySwingDamage = 0,
                        PrimarySwingSpeed = 126,
                        PrimaryWeaponFlags = (WeaponFlags?) 19327881474,
                        SecondaryThrustDamage = 29,
                        SecondaryThrustSpeed = 101,
                        SecondarySwingDamage = 33,
                        SecondarySwingSpeed = 126,
                        SecondaryWeaponFlags = (WeaponFlags?) 1
                    },
                    new Item
                    {
                        MbId = "mp_arrows_steppe",
                        Name = "Steppe Arrow",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_arrows_steppe"),
                        Value = 86,
                        Type = ItemType.Arrows,
                        Weight = 0.05f,
                        Accuracy = 100,
                        MissileSpeed = 10,
                        StackAmount = 25,
                        WeaponLength = 97,
                        PrimaryWeaponFlags = (WeaponFlags?) 21474836736,
                    },
                    new Item
                    {
                        MbId = "mp_bolts_western",
                        Name = "Bolt",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_bolts_western"),
                        Value = 50,
                        Type = ItemType.Bolts,
                        Weight = 0.05f,
                        Accuracy = 100,
                        MissileSpeed = 10,
                        StackAmount = 20,
                        WeaponLength = 37,
                        PrimaryWeaponFlags = (WeaponFlags?) 21474836864,
                    }
                );

                _db.Users.AddRange(
                    new User
                    {
                        SteamId = 76561197987525637,
                        UserName = "takeoshigeru",
                        Golds = Constants.StartingGolds,
                        Role = Role.SuperAdmin,
                        AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/2c/2ce4694f06523a2ffad501f5dc30ec7a8008e90e.jpg"),
                        AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/2c/2ce4694f06523a2ffad501f5dc30ec7a8008e90e_full.jpg"),
                        AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/2c/2ce4694f06523a2ffad501f5dc30ec7a8008e90e_medium.jpg"),
                        Characters = new List<Character>
                        {
                            new Character
                            {
                                Name = "takeoshigeru",
                                Level = 23,
                                Experience = 2529284,
                            },
                            new Character
                            {
                                Name = "totoalala",
                                Level = 12,
                                Experience = 13493,
                            },
                            new Character
                            {
                                Name = "jackie",
                                Level = 1,
                                Experience = 200,
                            },
                        }
                    });

                await _db.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }
}