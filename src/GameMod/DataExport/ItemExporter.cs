using System.Xml;
using Crpg.GameMod.Api.Models;
using Crpg.GameMod.Api.Models.Items;
using Crpg.GameMod.Helpers.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;

namespace Crpg.GameMod.DataExport;

internal class ItemExporter : IDataExporter
{
    private const string ItemFilesPath = "../../Modules/SandBoxCore/ModuleData/items";

    private static readonly HashSet<ItemObject.ItemTypeEnum> BlacklistedItemTypes = new()
    {
        ItemObject.ItemTypeEnum.Invalid,
        ItemObject.ItemTypeEnum.Goods,
        ItemObject.ItemTypeEnum.Animal,
        ItemObject.ItemTypeEnum.Book,
    };

    private static readonly HashSet<string> BlacklistedItems = new()
    {
        "a_aserai_scale_b_shoulder_e", // Name conflict with a_aserai_scale_b_shoulder_c rank 2.
        "a_brass_lamellar_shoulder_b", // Name conflict with a_brass_lamellar_shoulder_white_a rank 2.
        "a_brass_lamellar_shoulder_white_b", // Name conflict a_brass_lamellar_shoulder_b.
        "arming_coif", // Name conflict with imperial_leather_coif.
        "aserai_horse_tournament", // Name conflict with aserai_horse.
        "aserai_lord_helmet_a", // Name conflict with southern_lord_helmet.
        "ballista_projectile", // Can't be equipped.
        "ballista_projectile_burning", // Can't be equipped.
        "battania_civil_cloak", // Name conflict with battania_cloak.
        "battania_female_civil_a", // Name conflict with battania_dress_b.
        "battania_horse_tournament", // Name conflict with battania_horse.
        "battania_shield_targe_a", // Name conflict with battania_targe_b rank 2.
        "battered_kite_shield", // Name conflict with western_kite_shield rank -2.
        "bolt_b", // Name conflict with bolt_a.
        "bolt_c", // Name conflict with bolt_a.
        "bolt_d", // Name conflict with bolt_a.
        "bolt_e", // Name conflict with bolt_a.
        "boulder", // Can't be equipped.
        "bound_horsemans_kite_shield", // Name conflict with northern_scouts_shield.
        "camel_tournament", // Name conflict with camel.
        "desert_round_shield", // Name conflict with bound_desert_round_shield rank 2.
        "eastern_leather_boots", // Name conflict with leather_boots.
        "empire_crown_v2", // Name conflict with empire_crown_west.
        "empire_horse_tournament", // Name conflict with empire_horse.
        "empire_lance_1_t3_blunt", // Name conflict with vlandia_lance_1_t3_blunt.
        "empire_sword_1_t2", // Name conflict with iron_spatha_sword_t2.
        "empire_sword_1_t2_blunt", // Name conflict with iron_spatha_sword_t2.
        "grapeshot_fire_projectile", // Can't be equipped.
        "grapeshot_fire_stack", // Can't be equipped.
        "grapeshot_projectile", // Can't be equipped.
        "grapeshot_stack", // Can't be equipped.
        "heavy_horsemans_kite_shield", // Name conflict with bound_horsemans_kite_shield rank 2.
        "imperial_open_mail_coif", // Name conflict with open_mail_coif.
        "ironrim_riders_kite_shield", // Name conflict with ironrim_kite_shield rank 2.
        "khuzait_horse_tournament", // Name conflict with khuzait_horse.
        "khuzait_lamellar_strapped", // Name conflict with basic_imperial_leather_armor rank 2.
        "khuzait_leather_stitched", // Name conflict with eastern_plated_leather_vest rank 1.
        "leather_coat", // Name conflict with leather_tunic.
        "leatherlame_roundkettle_over_imperial_mail", // Name conflict with roundkettle_over_imperial_mail.
        "longbow_recurve_desert_bow", // Name conflict with tribal_bow rank 1.
        "lordly_padded_mitten", // Name conflict with padded_mitten rank 3.
        "mule_unmountable", // Can't be equipped.
        "pack_camel_unmountable", // Can't be equipped.
        "pot", // Can't be equipped.
        "reinforced_kite_shield", // Name conflict with western_kite_shield rank 2.
        "reinforced_mail_mitten", // Name conflict with mail_mitten rank 2.
        "reinforced_padded_mitten", // Name conflict with padded_mitten rank 2.
        "southern_lamellar_armor", // Name conflict with desert_lamellar.
        "southern_lord_helmet", // Name conflict with desert_helmet rank 3.
        "strapped_round_shield", // Name conflict with leather_round_shield rank 2.
        "stronger_eastern_wicker_shield", // Name conflict with eastern_wicker_shield rank 2.
        "stronger_footmans_wicker_shield", // Name conflict with footmans_wicker_shield rank 2.
        "sturgia_horse_tournament", // Name conflict with sturgia_horse.
        "sturgia_old_shield_c", // Name conflict with leather_round_shield rank 2.
        "sturgia_old_shield_c", // Name conflict with strapped_round_shield.
        "sturgian_helmet_b_close", // Name conflict with closed_goggled_helmet.
        "sturgian_helmet_b_open", // Name conflict with sturgian_helmet_open.
        "sturgian_lamellar_gambeson", // Name conflict with sturgian_lamellar_gambeson_heavy.
        "torch", // Invisible.
        "vlandia_horse_tournament", // Name conflict with vlandia_horse.
        "vlandia_mace_3_t5", // Name conflict with pernach_mace_t3.
        "womens_headwrap_c", // Name conflict with head_wrapped.
        "wooden_sword_t2", // Name conflict with wooden_sword_t1.
        "woodland_throwing_axe_1_t1", // Name conflict with highland_throwing_axe_1_t2.
    };

    public Task Export(string outputPath)
    {
        var mbItems = DeserializeMbItems(ItemFilesPath)
            .DistinctBy(i => i.StringId)
            .Where(FilterItem) // Remove test and blacklisted items
            .OrderBy(i => i.StringId)
            .ToArray();
        var crpgItems = mbItems.Select(MbToCrpgItem);

        Directory.CreateDirectory(outputPath);
        SerializeCrpgItems(crpgItems, outputPath);
        return GenerateItemsThumbnail(mbItems, Path.Combine(outputPath, "Items"));
    }

    private static bool FilterItem(ItemObject mbItem) => !mbItem.StringId.Contains("test")
                                                         && !mbItem.StringId.Contains("dummy")
                                                         && !mbItem.Name.Contains("_")
                                                         && !BlacklistedItemTypes.Contains(mbItem.ItemType)
                                                         && !BlacklistedItems.Contains(mbItem.StringId);

    private static CrpgItemCreation MbToCrpgItem(ItemObject mbItem)
    {
        CrpgItemCreation crpgItem = new()
        {
            TemplateMbId = mbItem.StringId,
            Name = mbItem.Name.ToString(),
            Culture = MbToCrpgCulture(mbItem.Culture),
            Type = MbToCrpgItemType(mbItem.Type),
            Weight = mbItem.Weight,
        };

        if (mbItem.ArmorComponent != null)
        {
            crpgItem.Armor = new CrpgItemArmorComponent
            {
                HeadArmor = mbItem.ArmorComponent.HeadArmor,
                BodyArmor = mbItem.ArmorComponent.BodyArmor,
                ArmArmor = mbItem.ArmorComponent.ArmArmor,
                LegArmor = mbItem.ArmorComponent.LegArmor,
                MaterialType = MbToCrpgArmorMaterialType(mbItem.ArmorComponent.MaterialType),
            };
        }

        if (mbItem.HorseComponent != null)
        {
            crpgItem.Mount = new CrpgItemMountComponent
            {
                BodyLength = mbItem.HorseComponent.BodyLength,
                ChargeDamage = mbItem.HorseComponent.ChargeDamage,
                Maneuver = mbItem.HorseComponent.Maneuver,
                Speed = mbItem.HorseComponent.Speed,
                HitPoints = mbItem.HorseComponent.HitPoints + mbItem.HorseComponent.HitPointBonus,
            };
        }

        if (mbItem.WeaponComponent != null)
        {
            crpgItem.Weapons = mbItem.WeaponComponent.Weapons.Select(w => new CrpgItemWeaponComponent
            {
                Class = MbToCrpgWeaponClass(w.WeaponClass),
                Accuracy = w.Accuracy,
                MissileSpeed = w.MissileSpeed,
                StackAmount = w.MaxDataValue,
                Length = w.WeaponLength,
                Balance = w.WeaponBalance,
                Handling = w.Handling,
                BodyArmor = w.BodyArmor,
                Flags = MbToCrpgWeaponFlags(w.WeaponFlags),
                ThrustDamage = w.ThrustDamage,
                ThrustDamageType = MbToCrpgDamageType(w.ThrustDamageType),
                ThrustSpeed = w.ThrustSpeed,
                SwingDamage = w.SwingDamage,
                SwingDamageType = MbToCrpgDamageType(w.SwingDamageType),
                SwingSpeed = w.SwingSpeed,
            }).ToArray();
        }

        return crpgItem;
    }

    private static CrpgItemType MbToCrpgItemType(ItemObject.ItemTypeEnum t) => t switch
    {
        ItemObject.ItemTypeEnum.Invalid => CrpgItemType.Undefined, // To be consistent with WeaponClass.
        ItemObject.ItemTypeEnum.Horse => CrpgItemType.Mount, // Horse includes camel and mule.
        ItemObject.ItemTypeEnum.HorseHarness => CrpgItemType.MountHarness, // Horse includes camel and mule.
        ItemObject.ItemTypeEnum.Cape => CrpgItemType.ShoulderArmor, // Cape is a bad name.
        _ => (CrpgItemType)Enum.Parse(typeof(CrpgItemType), t.ToString()),
    };

    private static CrpgCulture MbToCrpgCulture(BasicCultureObject? culture) => culture == null
        ? CrpgCulture.Neutral // Consider no culture as neutral.
        : (CrpgCulture)Enum.Parse(typeof(CrpgCulture), culture.ToString());

    private static CrpgArmorMaterialType MbToCrpgArmorMaterialType(ArmorComponent.ArmorMaterialTypes t) => t switch
    {
        ArmorComponent.ArmorMaterialTypes.None => CrpgArmorMaterialType.Undefined, // To be consistent with WeaponClass.
        _ => (CrpgArmorMaterialType)Enum.Parse(typeof(CrpgArmorMaterialType), t.ToString()),
    };

    private static CrpgWeaponClass MbToCrpgWeaponClass(WeaponClass wc) =>
        (CrpgWeaponClass)Enum.Parse(typeof(CrpgWeaponClass), wc.ToString());

    private static CrpgWeaponFlags MbToCrpgWeaponFlags(WeaponFlags wf) => (CrpgWeaponFlags)wf;

    private static CrpgDamageType MbToCrpgDamageType(DamageTypes t) => t switch
    {
        DamageTypes.Invalid => CrpgDamageType.Undefined, // To be consistent with WeaponClass.
        _ => (CrpgDamageType)Enum.Parse(typeof(CrpgDamageType), t.ToString()),
    };

    private static IEnumerable<ItemObject> DeserializeMbItems(string folderPath)
    {
        var game = Game.CreateGame(new MultiplayerGame(), new MultiplayerGameManager());
        game.Initialize();

        var items = Enumerable.Empty<ItemObject>();
        foreach (string path in Directory.EnumerateFiles(folderPath))
        {
            XmlDocument itemsDoc = new();
            using (var r = XmlReader.Create(path, new XmlReaderSettings { IgnoreComments = true }))
            {
                itemsDoc.Load(r);
            }

            var fileItems = itemsDoc
                .LastChild
                .ChildNodes
                .Cast<XmlNode>()
                .Select(itemNode =>
                {
                    ItemObject item = new();
                    item.Deserialize(game.ObjectManager, itemNode);
                    return item;
                });
            items = items.Concat(fileItems);
        }

        return items;
    }

    private static void SerializeCrpgItems(IEnumerable<CrpgItemCreation> items, string outputPath)
    {
        var serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Newtonsoft.Json.Formatting.Indented,
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            Converters = new JsonConverter[] { new ArrayStringEnumFlagsConverter(), new StringEnumConverter() },
        });

        using StreamWriter s = new(Path.Combine(outputPath, "items.json"));
        serializer.Serialize(s, items);
    }

    private static Task GenerateItemsThumbnail(IEnumerable<ItemObject> mbItems, string outputPath)
    {
        List<Task> createTextureTasks = new();
        foreach (var mbItem in mbItems)
        {
            /*
            Bannerlord generates image thumbnails by loading the 3D texture, spawning a camera and taking a screenshot
            from it. For each item type, a different camera angle is used. For shields and hand armors, it seems like
            they are placed on an agent. To do that without spawning an agent, their type is overriden by one that
            does not need an agent. It was observed that the bow's camera angle and the animal's camera angle were
            good substitute for respectively shield and hand armor.
             */
            mbItem.Type = mbItem.Type switch
            {
                ItemObject.ItemTypeEnum.Shield => ItemObject.ItemTypeEnum.Bow,
                ItemObject.ItemTypeEnum.HandArmor => ItemObject.ItemTypeEnum.Animal,
                _ => mbItem.Type,
            };

            TaskCompletionSource<object?> createTextureTaskSource = new();
            createTextureTasks.Add(createTextureTaskSource.Task);

            // Texture.SaveToFile doesn't accept absolute paths
            // TODO: what is second argument "additionalArgs"?
            TableauCacheManager.Current.BeginCreateItemTexture(mbItem, null, texture =>
            {
                texture.SaveToFile(Path.Combine(outputPath, mbItem.StringId + ".png"));
                createTextureTaskSource.SetResult(null);
            });
        }

        return Task.WhenAll(createTextureTasks);
    }
}
