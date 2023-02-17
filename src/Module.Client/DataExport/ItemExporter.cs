using System.Globalization;
using System.Xml;
using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common.Models;
using Crpg.Module.Helpers.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Tableaus;

namespace Crpg.Module.DataExport;

internal class ItemExporter : IDataExporter
{
    private static readonly string[] ItemFilePaths =
    {
        "../../Modules/cRPG/ModuleData/items/head_armors.xml",
        "../../Modules/cRPG/ModuleData/items/shoulder_armors.xml",
        "../../Modules/cRPG/ModuleData/items/body_armors.xml",
        "../../Modules/cRPG/ModuleData/items/arm_armors.xml",
        "../../Modules/cRPG/ModuleData/items/leg_armors.xml",
    };
    public async Task ComputeWeight(string gitRepoPath)
    {
        foreach (string filePath in ItemFilePaths)
        {
            var game = Game.CreateGame(new MultiplayerGame(), new MultiplayerGameManager());
            game.Initialize();
            var mbItems = game.ObjectManager.GetObjectTypeList<ItemObject>()
                .Where(i => i.StringId.StartsWith("crpg_"))
                .DistinctBy(i => i.StringId)
                .OrderBy(i => i.StringId)
                .ToArray();
            var itemsDoc = LoadMbDocument(filePath);
            itemsDoc.Save(Path.Combine("../../Modules/cRPG/ModuleData/items", Path.GetFileName(filePath)));
        }
    }
    public async Task Export(string gitRepoPath)
    {
        var game = Game.CreateGame(new MultiplayerGame(), new MultiplayerGameManager());
        game.Initialize();
        var mbItems = game.ObjectManager.GetObjectTypeList<ItemObject>()
            .Where(i => i.StringId.StartsWith("crpg_"))
            .DistinctBy(i => i.StringId)
            .OrderBy(i => i.StringId)
            .ToArray();
        var crpgItems = mbItems.Select(MbToCrpgItem);
        SerializeCrpgItems(crpgItems, Path.Combine("../../Modules/cRPG/ModuleData"));
    }

    public async Task ImageExport(string gitRepoPath)
    {
        var game = Game.CreateGame(new MultiplayerGame(), new MultiplayerGameManager());
        game.Initialize();
        var mbItems = game.ObjectManager.GetObjectTypeList<ItemObject>()
            .Where(i => i.StringId.StartsWith("crpg_"))
            .DistinctBy(i => i.StringId)
            .OrderBy(i => i.StringId)
            .ToArray();
        var crpgItems = mbItems.Select(MbToCrpgItem);
        string itemThumbnailsPath = Path.Combine("../../Modules/cRPG/images");
        Directory.CreateDirectory(itemThumbnailsPath);
        await GenerateItemsThumbnail(mbItems, itemThumbnailsPath);
    }

    private static CrpgItem MbToCrpgItem(ItemObject mbItem)
    {
        CrpgItem crpgItem = new()
        {
            Id = mbItem.StringId,
            Name = mbItem.Name.ToString(),
            Culture = MbToCrpgCulture(mbItem.Culture),
            Type = MbToCrpgItemType(mbItem.Type),
            Price = mbItem.Value,
            Weight = mbItem.Weight,
            Tier = mbItem.Tierf,
            Requirement = CrpgItemRequirementModel.ComputeItemRequirement(mbItem),
            Flags = MbToCrpgItemFlags(mbItem.ItemFlags),
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
                FamilyType = mbItem.ArmorComponent.FamilyType,
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
                FamilyType = mbItem.HorseComponent.Monster.FamilyType,
            };
        }

        List<WeaponClass> meleeClass = new()
        {
            WeaponClass.Dagger,
            WeaponClass.Mace,
            WeaponClass.TwoHandedMace,
            WeaponClass.OneHandedSword,
            WeaponClass.TwoHandedSword,
            WeaponClass.OneHandedAxe,
            WeaponClass.TwoHandedAxe,
            WeaponClass.Pick,
            WeaponClass.LowGripPolearm,
            WeaponClass.OneHandedPolearm,
            WeaponClass.TwoHandedPolearm,
        };
        if (mbItem.WeaponComponent != null)
        {
            crpgItem.Weapons = mbItem.WeaponComponent.Weapons.Select(w => new CrpgItemWeaponComponent
            {
                Class = MbToCrpgWeaponClass(w.WeaponClass),
                ItemUsage = w.ItemUsage,
                Accuracy = w.Accuracy,
                MissileSpeed = w.MissileSpeed,
                StackAmount = w.MaxDataValue,
                Length = w.WeaponLength,
                Balance = w.WeaponBalance,
                Handling = w.Handling,
                BodyArmor = w.BodyArmor,
                Flags = MbToCrpgWeaponFlags(w.WeaponFlags),
                ThrustDamage = meleeClass.Contains(w.WeaponClass) ? (int)(w.ThrustDamageFactor * CrpgStrikeMagnitudeModel.BladeDamageFactorToDamageRatio) : w.ThrustDamage,
                ThrustDamageType = MbToCrpgDamageType(w.ThrustDamageType),
                ThrustSpeed = w.ThrustSpeed,
                SwingDamage = meleeClass.Contains(w.WeaponClass) ? (int)(w.SwingDamageFactor * CrpgStrikeMagnitudeModel.BladeDamageFactorToDamageRatio) : w.SwingDamage,
                SwingDamageType = MbToCrpgDamageType(w.SwingDamageType),
                SwingSpeed = w.SwingSpeed,
            }).ToArray();
        }

        return crpgItem;
    }
    private static XmlDocument LoadMbDocument(string filePath)
    {
        XmlDocument itemsDoc = new();
        using (var r = XmlReader.Create(filePath, new XmlReaderSettings { IgnoreComments = true }))
        {
            itemsDoc.Load(r);
        }

        // Prefix all ids with "crpg_" to avoid conflicts with mb objects.
        var nodes1 = itemsDoc.LastChild.ChildNodes.Cast<XmlNode>().ToArray();
        for (int i = 0; i < nodes1.Length; i += 1)
        {
            var node1 = nodes1[i];

            if (node1.Name == "Item")
            {
                // Remove the price attribute so it is recomputed using our model.
                var valueAttr = node1.Attributes["value"];
                if (valueAttr != null)
                {
                    node1.Attributes.Remove(valueAttr);
                }

                var type = (ItemObject.ItemTypeEnum)Enum.Parse(typeof(ItemObject.ItemTypeEnum), node1.Attributes!["Type"].Value);
                if (type is ItemObject.ItemTypeEnum.HeadArmor
                         or ItemObject.ItemTypeEnum.Cape
                         or ItemObject.ItemTypeEnum.BodyArmor
                         or ItemObject.ItemTypeEnum.HandArmor
                         or ItemObject.ItemTypeEnum.LegArmor)
                {
                    ModifyNodeAttribute(node1, "weight",
                        _ => ModifyArmorWeight(node1, type).ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        return itemsDoc;
    }
    private static float ModifyArmorWeight(XmlNode node, ItemObject.ItemTypeEnum type)
    {
        XmlNode armorNode = node.SelectNodes("ItemComponent/Armor")!.Cast<XmlNode>().First();
        float armorPower =
            1.0f * (armorNode.Attributes["head_armor"] == null ? 0f : float.Parse(armorNode.Attributes["head_armor"].Value))
          + 1.0f * (armorNode.Attributes["body_armor"] == null ? 0f : float.Parse(armorNode.Attributes["body_armor"].Value))
          + 1.3f * (armorNode.Attributes["arm_armor"] == null ? 0f : float.Parse(armorNode.Attributes["arm_armor"].Value))
          + 1.1f * (armorNode.Attributes["leg_armor"] == null ? 0f : float.Parse(armorNode.Attributes["leg_armor"].Value));
        float bestArmorPower = type switch
        {
            ItemObject.ItemTypeEnum.HeadArmor => 661f,
            ItemObject.ItemTypeEnum.Cape => 400f,
            ItemObject.ItemTypeEnum.BodyArmor => 400f,
            ItemObject.ItemTypeEnum.HandArmor => 400f,
            ItemObject.ItemTypeEnum.LegArmor => 400f,
            ItemObject.ItemTypeEnum.HorseHarness => 100f,
            _ => throw new ArgumentOutOfRangeException(),
        };
        return 20f * (float)Math.Pow(armorPower, 1.3f) / bestArmorPower;
    }

    private static void ModifyNodeAttribute(
        XmlNode node,
        string attributeName,
        Func<string, string> modify,
        string? defaultValue = null)
    {
        var attr = node.Attributes![attributeName];
        if (attr == null)
        {
            if (defaultValue == null)
            {
                throw new KeyNotFoundException($"Attribute '{attributeName}' was not found and no default was provided");
            }

            attr = node.OwnerDocument!.CreateAttribute(attributeName);
            attr.Value = defaultValue;
            node.Attributes.Append(attr);
        }

        attr.Value = modify(attr.Value);
    }
    private static CrpgItemType MbToCrpgItemType(ItemObject.ItemTypeEnum t) => t switch
    {
        ItemObject.ItemTypeEnum.Invalid => CrpgItemType.Undefined, // To be consistent with WeaponClass.
        ItemObject.ItemTypeEnum.Horse => CrpgItemType.Mount, // Horse includes camel and mule.
        ItemObject.ItemTypeEnum.HorseHarness => CrpgItemType.MountHarness, // Horse includes camel and mule.
        ItemObject.ItemTypeEnum.Cape => CrpgItemType.ShoulderArmor, // Cape is a bad name.
        _ => (CrpgItemType)Enum.Parse(typeof(CrpgItemType), t.ToString()),
    };

    private static CrpgItemFlags MbToCrpgItemFlags(ItemFlags f) =>
        (CrpgItemFlags)Enum.Parse(typeof(CrpgItemFlags), f.ToString());

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
    private static void SerializeCrpgItems(IEnumerable<CrpgItem> items, string outputPath)
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
