using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Games;
using Crpg.Application.Games.Commands;
using Crpg.Domain.Entities;
using MediatR;

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
                        MbId = "mp_laced_cloth_coif",
                        Name = "Laced Cloth Coif",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_laced_cloth_coif"),
                        Value = 48,
                        Type = ItemType.HeadArmor,
                        Weight = 0.4f,
                        HeadArmor = 4,
                    },
                    new Item
                    {
                        MbId = "mp_nomad_padded_hood",
                        Name = "Nomad Padded Hood",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_nomad_padded_hood"),
                        Value = 48,
                        Type = ItemType.HeadArmor,
                        Weight = 0.4f,
                        HeadArmor = 4,
                    },
                    new Item
                    {
                        MbId = "mp_wrapped_desert_cap",
                        Name = "Wrapped Southern Cap",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_wrapped_desert_cap"),
                        Value = 61,
                        Type = ItemType.HeadArmor,
                        Weight = 0.5f,
                        HeadArmor = 6,
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
                        MbId = "mp_vlandia_bandit_cape_a",
                        Name = "Rough Padded Cap",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_vlandia_bandit_cape_a"),
                        Value = 69,
                        Type = ItemType.HeadArmor,
                        Weight = 2.5f,
                        HeadArmor = 16,
                    },
                    new Item
                    {
                        MbId = "mp_pointed_skullcap_over_cloth_headwrap",
                        Name = "Pointed Skullcap Over Cloth Headwrap",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_pointed_skullcap_over_cloth_headwrap"),
                        Value = 69,
                        Type = ItemType.HeadArmor,
                        Weight = 1.1f,
                        HeadArmor = 16,
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
                        MbId = "mp_nordic_fur_cap",
                        Name = "Northern Cap",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_nordic_fur_cap"),
                        Value = 100,
                        Type = ItemType.HeadArmor,
                        Weight = 0.3f,
                        HeadArmor = 10,
                    },
                    new Item
                    {
                        MbId = "mp_battania_civil_hood",
                        Name = "Highland Hood",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_battania_civil_hood"),
                        Value = 148,
                        Type = ItemType.HeadArmor,
                        Weight = 0.5f,
                        HeadArmor = 9,
                    },
                    new Item
                    {
                        MbId = "mp_arming_coif",
                        Name = "Leather Coif",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_arming_coif"),
                        Value = 188,
                        Type = ItemType.HeadArmor,
                        Weight = 0.3f,
                        HeadArmor = 11,
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
                        MbId = "mp_burlap_sack_dress",
                        Name = "Burlap Dress",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_burlap_sack_dress"),
                        Value = 8,
                        Type = ItemType.BodyArmor,
                        Weight = 0.5f,
                        HeadArmor = 0,
                        BodyArmor = 1,
                        ArmArmor = 0,
                        LegArmor = 1
                    },
                    new Item
                    {
                        MbId = "mp_sackcloth_tunic",
                        Name = "Sackcloth Tunic",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_sackcloth_tunic"),
                        Value = 11,
                        Type = ItemType.BodyArmor,
                        Weight = 0.6f,
                        HeadArmor = 0,
                        BodyArmor = 2,
                        ArmArmor = 1,
                        LegArmor = 1
                    },
                    new Item
                    {
                        MbId = "mp_battania_civil_a",
                        Name = "Highland Tunic",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_battania_civil_a"),
                        Value = 19,
                        Type = ItemType.BodyArmor,
                        Weight = 0.6f,
                        HeadArmor = 0,
                        BodyArmor = 1,
                        ArmArmor = 1,
                        LegArmor = 1
                    },
                    new Item
                    {
                        MbId = "mp_khuzait_civil_coat_b",
                        Name = "Eastern Thick Coat",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_khuzait_civil_coat_b"),
                        Value = 34,
                        Type = ItemType.BodyArmor,
                        Weight = 2.7f,
                        HeadArmor = 0,
                        BodyArmor = 6,
                        ArmArmor = 0,
                        LegArmor = 1
                    },
                    new Item
                    {
                        MbId = "mp_hemp_tunic",
                        Name = "Hemp Tunic",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_hemp_tunic"),
                        Value = 48,
                        Type = ItemType.BodyArmor,
                        Weight = 0.4f,
                        HeadArmor = 0,
                        BodyArmor = 1,
                        ArmArmor = 1,
                        LegArmor = 1
                    },
                    new Item
                    {
                        MbId = "mp_rough_tied_boots",
                        Name = "Rough Tied Boots",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_rough_tied_boots"),
                        Value = 1,
                        Type = ItemType.LegArmor,
                        Weight = 0.9f,
                        LegArmor = 9
                    },
                    new Item
                    {
                        MbId = "mp_leather_shoes",
                        Name = "Leather Shoes",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_leather_shoes"),
                        Value = 67,
                        Type = ItemType.LegArmor,
                        Weight = 0.7f,
                        LegArmor = 3
                    },
                    new Item
                    {
                        MbId = "mp_wrapped_shoes",
                        Name = "Wrapped Shoes",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_wrapped_shoes"),
                        Value = 74,
                        Type = ItemType.LegArmor,
                        Weight = 2f,
                        HeadArmor = 0,
                        BodyArmor = 0,
                        ArmArmor = 0,
                        LegArmor = 4
                    },
                    new Item
                    {
                        MbId = "mp_strapped_shoes",
                        Name = "Strapped Shoes",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_strapped_shoes"),
                        Value = 74,
                        Type = ItemType.LegArmor,
                        Weight = 0.7f,
                        LegArmor = 4
                    },
                    new Item
                    {
                        MbId = "mp_strapped_leather_boots",
                        Name = "Strapped Leather Boots",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_strapped_leather_boots"),
                        Value = 81,
                        Type = ItemType.LegArmor,
                        Weight = 0.9f,
                        LegArmor = 5
                    },
                    new Item
                    {
                        MbId = "mp_northern_tunic",
                        Name = "Northern Tunic",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_northern_tunic"),
                        Value = 66,
                        Type = ItemType.BodyArmor,
                        Weight = 0.4f,
                        HeadArmor = 0,
                        BodyArmor = 2,
                        ArmArmor = 1,
                        LegArmor = 1
                    },
                    new Item
                    {
                        MbId = "mp_aserai_civil_e",
                        Name = "Thawb",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_aserai_civil_e"),
                        Value = 80,
                        Type = ItemType.BodyArmor,
                        Weight = 1.1f,
                        HeadArmor = 0,
                        BodyArmor = 4,
                        ArmArmor = 2,
                        LegArmor = 1
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
                        HitPoints = 200,
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
                        HitPoints = 200,
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
                        HitPoints = 235,
                    },
                    new Item
                    {
                        MbId = "mp_strapped_round_shield",
                        Name = "Reinforced Large Round Shield",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_strapped_round_shield"),
                        Value = 78,
                        Type = ItemType.Shield,
                        Weight = 4.7f,
                        BodyArmor = 5,
                        StackAmount = 170,
                        WeaponLength = 70,
                        PrimaryThrustDamage = 5,
                        PrimaryThrustSpeed = 82,
                        PrimarySwingDamage = 0,
                        PrimarySwingSpeed = 82,
                        PrimaryWeaponFlags = (WeaponFlags?)268435968,
                    },
                    new Item
                    {
                        MbId = "mp_noyans_shield",
                        Name = "Decorated Eastern Shield",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_noyans_shield"),
                        Value = 134,
                        Type = ItemType.Shield,
                        Weight = 4.7f,
                        BodyArmor = 25,
                        StackAmount = 120,
                        WeaponLength = 60,
                        PrimaryThrustDamage = 0,
                        PrimaryThrustSpeed = 82,
                        PrimarySwingDamage = 0,
                        PrimarySwingSpeed = 82,
                        PrimaryWeaponFlags = (WeaponFlags?)268435968,
                    },
                    new Item
                    {
                        MbId = "mp_wide_heater_shield",
                        Name = "Wide Heater Shield",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_wide_heater_shield"),
                        Value = 119,
                        Type = ItemType.Shield,
                        Weight = 4.7f,
                        BodyArmor = 15,
                        StackAmount = 160,
                        WeaponLength = 90,
                        PrimaryThrustDamage = 0,
                        PrimaryThrustSpeed = 82,
                        PrimarySwingDamage = 0,
                        PrimarySwingSpeed = 82,
                        PrimaryWeaponFlags = (WeaponFlags?)268435968,
                    },
                    new Item
                    {
                        MbId = "mp_hunting_bow",
                        Name = "Hunting Bow",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_hunting_bow"),
                        Value = 227,
                        Type = ItemType.Bow,
                        Weight = 1f,
                        ThrustDamageType = DamageType.Pierce,
                        SwingDamageType = DamageType.Blunt,
                        Accuracy = 89,
                        MissileSpeed = 64,
                        WeaponLength = 95,
                        PrimaryThrustDamage = 42,
                        PrimaryThrustSpeed = 110,
                        PrimaryWeaponFlags = (WeaponFlags?)2628626,
                    },
                    new Item
                    {
                        MbId = "mp_crossbow",
                        Name = "Crossbow",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_crossbow"),
                        Value = 48500,
                        Type = ItemType.Crossbow,
                        Weight = 2.2f,
                        ThrustDamageType = DamageType.Pierce,
                        SwingDamageType = DamageType.Blunt,
                        Accuracy = 98,
                        MissileSpeed = 82,
                        WeaponLength = 95,
                        PrimaryThrustDamage = 85,
                        PrimaryThrustSpeed = 85,
                        PrimaryWeaponFlags = (WeaponFlags?)2360338,
                    },
                    new Item
                    {
                        MbId = "mp_khuzait_sichel",
                        Name = "Eastern Short Sickle",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_khuzait_sichel"),
                        Value = 1,
                        Type = ItemType.OneHandedWeapon,
                        Weight = 1.16f,
                        BodyArmor = 0,
                        SwingDamageType = DamageType.Cut,
                        Accuracy = 0,
                        MissileSpeed = 0,
                        StackAmount = 0,
                        WeaponLength = 112,
                        PrimarySwingDamage = 52,
                        PrimarySwingSpeed = 93,
                        PrimaryHandling = 90,
                        PrimaryWeaponFlags = (WeaponFlags?)65537,
                    },
                    new Item
                    {
                        MbId = "mp_aserai_axe",
                        Name = "Southern Axe",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_aserai_axe"),
                        Value = 1,
                        Type = ItemType.OneHandedWeapon,
                        Weight = 1.23f,
                        WeaponLength = 76,
                        PrimarySwingDamage = 67,
                        PrimarySwingSpeed = 90,
                        PrimaryHandling = 89,
                        PrimaryWeaponFlags = (WeaponFlags?)65537
                    },
                    new Item
                    {
                        MbId = "mp_sturgia_mace",
                        Name = "Northern Mace",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_sturgia_mace"),
                        Value = 1,
                        Type = ItemType.OneHandedWeapon,
                        Weight = 1.37f,
                        BodyArmor = 0,
                        SwingDamageType = DamageType.Blunt,
                        Accuracy = 0,
                        MissileSpeed = 0,
                        StackAmount = 0,
                        WeaponLength = 81,
                        PrimarySwingDamage = 51,
                        PrimarySwingSpeed = 86,
                        PrimaryHandling = 87,
                        PrimaryWeaponFlags = (WeaponFlags?)1,
                    },
                    new Item
                    {
                        MbId = "mp_hatchet_axe",
                        Name = "Hatchet",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_hatchet_axe"),
                        Value = 150,
                        Type = ItemType.OneHandedWeapon,
                        SwingDamageType = DamageType.Cut,
                        Weight = 1.54f,
                        WeaponLength = 48,
                        PrimarySwingDamage = 66,
                        PrimarySwingSpeed = 99,
                        PrimaryHandling = 102,
                        PrimaryWeaponFlags = (WeaponFlags?)65537,
                    },
                    new Item
                    {
                        MbId = "mp_empire_axe",
                        Name = "Imperial Axe",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_empire_axe"),
                        Value = 1,
                        Type = ItemType.OneHandedWeapon,
                        Weight = 1.16f,
                        ThrustDamageType = DamageType.Blunt,
                        SwingDamageType = 0,
                        WeaponLength = 76,
                        PrimaryThrustDamage = 41,
                        PrimaryThrustSpeed = 94,
                        PrimarySwingDamage = 73,
                        PrimarySwingSpeed = 88,
                        PrimaryHandling = 86,
                        PrimaryWeaponFlags = (WeaponFlags?)65537
                    },
                    new Item
                    {
                        MbId = "mp_battania_axe",
                        Name = "Highland Axe",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_battania_axe"),
                        Value = 1,
                        Type = ItemType.OneHandedWeapon,
                        Weight = 0.73f,
                        ThrustDamageType = DamageType.Pierce,
                        WeaponLength = 85,
                        PrimaryThrustDamage = 14,
                        PrimaryThrustSpeed = 98,
                        PrimarySwingDamage = 63,
                        PrimarySwingSpeed = 99,
                        PrimaryHandling = 95,
                        PrimaryWeaponFlags = (WeaponFlags?)65537
                    },
                    new Item
                    {
                        MbId = "mp_empire_long_twohandedaxe",
                        Name = "Skoldern Twohanded Axe",
                        Value = 1,
                        Type = ItemType.TwoHandedWeapon,
                        Weight = 0.94f,
                        ThrustDamageType = DamageType.Blunt,
                        WeaponLength = 131,
                        PrimaryThrustDamage = 20,
                        PrimaryThrustSpeed = 102,
                        PrimarySwingDamage = 102,
                        PrimarySwingSpeed = 93,
                        PrimaryHandling = 81,
                        PrimaryWeaponFlags = (WeaponFlags?)2162705
                    },
                    new Item
                    {
                        MbId = "mp_vlandian_billhook",
                        Name = "Billhook",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_vlandian_billhook"),
                        Value = 150,
                        Type = ItemType.TwoHandedWeapon,
                        Weight = 1.08f,
                        ThrustDamageType = DamageType.Blunt,
                        WeaponLength = 140,
                        PrimaryThrustDamage = 16,
                        PrimaryThrustSpeed = 100,
                        PrimarySwingDamage = 90,
                        PrimarySwingSpeed = 90,
                        PrimaryHandling = 78,
                        PrimaryWeaponFlags = (WeaponFlags?)2162705,
                    },
                    new Item
                    {
                        MbId = "mp_vlandian_polearm",
                        Name = "Voulge",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_vlandian_polearm"),
                        Value = 150,
                        Type = ItemType.Polearm,
                        Weight = 0.74f,
                        ThrustDamageType = DamageType.Pierce,
                        SwingDamageType = DamageType.Blunt,
                        WeaponLength = 136,
                        PrimaryThrustDamage = 30,
                        PrimaryThrustSpeed = 103,
                        PrimarySwingDamage = 113,
                        PrimarySwingSpeed = 85,
                        PrimaryHandling = 83,
                        PrimaryWeaponFlags = (WeaponFlags?)2097233,
                    },
                    new Item
                    {
                        MbId = "mp_khuzait_glaive",
                        Name = "Glaive",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_khuzait_glaive"),
                        Value = 150,
                        Type = ItemType.Polearm,
                        Weight = 1.23f,
                        ThrustDamageType = DamageType.Pierce,
                        SwingDamageType = DamageType.Cut,
                        WeaponLength = 149,
                        PrimaryThrustDamage = 32,
                        PrimaryThrustSpeed = 91,
                        PrimarySwingDamage = 48,
                        PrimarySwingSpeed = 52,
                        PrimaryHandling = 76,
                        PrimaryWeaponFlags = (WeaponFlags?)65,
                        SecondaryThrustDamage = 38,
                        SecondaryThrustSpeed = 99,
                        SecondarySwingDamage = 106,
                        SecondarySwingSpeed = 77,
                        SecondaryHandling = 75,
                        SecondaryWeaponFlags = (WeaponFlags?)2097233
                    },
                    new Item
                    {
                        MbId = "mp_throwing_stone",
                        Name = "Stone",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_throwing_stone"),
                        Value = 10,
                        Type = ItemType.Thrown,
                        Weight = 0.1f,
                        BodyArmor = 0,
                        ThrustDamageType = DamageType.Blunt,
                        SwingDamageType = DamageType.Blunt,
                        Accuracy = 50,
                        MissileSpeed = 40,
                        StackAmount = 15,
                        WeaponLength = 10,
                        PrimaryThrustDamage = 16,
                        PrimaryThrustSpeed = 102,
                        PrimarySwingDamage = 0,
                        PrimarySwingSpeed = 102,
                        PrimaryHandling = 102,
                        PrimaryWeaponFlags = (WeaponFlags?)2148008194
                    },
                    new Item
                    {
                        MbId = "mp_sling_stone",
                        Name = "Sling",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_sling_stone"),
                        Value = 10,
                        Type = ItemType.Thrown,
                        Weight = 0.1f,
                        BodyArmor = 0,
                        ThrustDamageType = DamageType.Blunt,
                        SwingDamageType = DamageType.Blunt,
                        Accuracy = 88,
                        MissileSpeed = 40,
                        StackAmount = 15,
                        WeaponLength = 10,
                        PrimaryThrustDamage = 16,
                        PrimaryThrustSpeed = 102,
                        PrimarySwingDamage = 0,
                        PrimarySwingSpeed = 102,
                        PrimaryHandling = 102,
                        PrimaryWeaponFlags = (WeaponFlags?)6442975490
                    },
                    new Item
                    {
                        MbId = "mp_battania_throwing_knife",
                        Name = "Highland ThrowingKnife",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_battania_throwing_knife"),
                        Value = 450,
                        Type = ItemType.Thrown,
                        Weight = 0.6f,
                        ThrustDamageType = DamageType.Pierce,
                        SwingDamageType = DamageType.Blunt,
                        Accuracy = 95,
                        MissileSpeed = 33,
                        StackAmount = 5,
                        WeaponLength = 43,
                        PrimaryThrustDamage = 18,
                        PrimaryThrustSpeed = 101,
                        PrimarySwingDamage = 0,
                        PrimarySwingSpeed = 126,
                        PrimaryHandling = 116,
                        PrimaryWeaponFlags = (WeaponFlags?)19327881474,
                        SecondaryThrustDamage = 29,
                        SecondaryThrustSpeed = 101,
                        SecondarySwingDamage = 33,
                        SecondarySwingSpeed = 126,
                        SecondaryHandling = 116,
                        SecondaryWeaponFlags = (WeaponFlags?)1
                    },
                    new Item
                    {
                        MbId = "mp_arrows_steppe",
                        Name = "Steppe Arrow",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_arrows_steppe"),
                        Value = 86,
                        Type = ItemType.Arrows,
                        Weight = 0.05f,
                        ThrustDamageType = DamageType.Pierce,
                        Accuracy = 100,
                        MissileSpeed = 10,
                        StackAmount = 25,
                        WeaponLength = 97,
                        PrimaryWeaponFlags = (WeaponFlags?)21474836736,
                    },
                    new Item
                    {
                        MbId = "mp_bolts_western",
                        Name = "Bolt",
                        Image = new Uri("https://via.placeholder.com/256x120?text=mp_bolts_western"),
                        Value = 50,
                        Type = ItemType.Bolts,
                        Weight = 0.05f,
                        ThrustDamageType = DamageType.Pierce,
                        Accuracy = 100,
                        MissileSpeed = 10,
                        StackAmount = 20,
                        WeaponLength = 37,
                        PrimaryWeaponFlags = (WeaponFlags?)21474836864,
                    });

                _db.Users.AddRange(
                    new User
                    {
                        SteamId = 76561197987525637,
                        UserName = "takeoshigeru",
                        Gold = 3000,
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
                                Experience = ExperienceTable.GetExperienceForLevel(23),
                                Statistics = new CharacterStatistics
                                {
                                    Attributes = new CharacterAttributes
                                    {
                                        Points = 4,
                                        Strength = 3,
                                        Agility = 3,
                                    },
                                    Skills = new CharacterSkills { Points = 7 },
                                    WeaponProficiencies = new CharacterWeaponProficiencies { Points = 43 },
                                },
                            },
                            new Character
                            {
                                Name = "totoalala",
                                Level = 12,
                                Experience = ExperienceTable.GetExperienceForLevel(12),
                            },
                            new Character
                            {
                                Name = "Retire me",
                                Level = 31,
                                Experience = ExperienceTable.GetExperienceForLevel(31) + 100,
                            },
                        }
                    });

                await _db.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }
}