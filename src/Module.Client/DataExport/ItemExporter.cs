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
        var craftingPiecesDoc = LoadMbDocument("A:/Repos_Git/crpg/src/Module.Server/ModuleData/crafting_pieces.xml");
        craftingPiecesDoc.Save("A:/Repos_Git/crpg/src/Module.Server/ModuleData/crafting_pieces_duplicated.xml");
        var weaponDoc = LoadMbDocument("A:/Repos_Git/crpg/src/Module.Server/ModuleData/items/weapons.xml");
        weaponDoc.Save("A:/Repos_Git/crpg/src/Module.Server/ModuleData/items/weapons_duplicated.xml");
        var craftingTemplatesDoc = LoadMbDocument("A:/Repos_Git/crpg/src/Module.Server/ModuleData/crafting_templates.xml");
        craftingTemplatesDoc.Save("A:/Repos_Git/crpg/src/Module.Server/ModuleData/crafting_templates_duplicated.xml");
        var weaponDescriptionsDoc = LoadMbDocument("A:/Repos_Git/crpg/src/Module.Server/ModuleData/weapon_descriptions.xml");
        weaponDescriptionsDoc.Save("A:/Repos_Git/crpg/src/Module.Server/ModuleData/weapon_descriptions_duplicated.xml");
        var mbItems = game.ObjectManager.GetObjectTypeList<ItemObject>()
            .Where(i => i.StringId.StartsWith("crpg_"))
            .DistinctBy(i => i.StringId)
            .OrderBy(i => i.StringId)
            .ToArray();
        var crpgItems = mbItems.Select(MbToCrpgItem);
        SerializeCrpgItems(crpgItems, Path.Combine(gitRepoPath, "data"));/*
        const string itemThumbnailsTempPath = "../../crpg-items";
        string itemThumbnailsPath = Path.Combine(gitRepoPath, "src/WebUI/public/items");
        Directory.CreateDirectory(itemThumbnailsTempPath);
        await GenerateItemsThumbnail(mbItems, itemThumbnailsTempPath);
        Directory.Delete(itemThumbnailsPath, recursive: true);
        Directory.Move(itemThumbnailsTempPath, itemThumbnailsPath);*/
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

        XmlDocument weaponDoc = new();
        using (var p = XmlReader.Create("A:/Repos_Git/crpg/src/Module.Server/ModuleData/items/weapons.xml", new XmlReaderSettings { IgnoreComments = true }))
        {
            weaponDoc.Load(p);
        }

        var weaponnodes1 = weaponDoc.LastChild.ChildNodes.Cast<XmlNode>().ToArray();
        var nodes1 = itemsDoc.LastChild.ChildNodes.Cast<XmlNode>().ToArray();

        if (itemsDoc.LastChild.Name == "CraftingPieces")
        {
            List<XmlNode> craftingPieceToAdd = new();
            for (int i = nodes1.Length - 1; i >= 0; i--)
            {
                var node1 = nodes1[i];
                if (node1.Name == "CraftingPiece")
                {
                    for (int p = weaponnodes1.Length - 1; p >= 0; p--)
                    {
                        var weaponnode1 = weaponnodes1[p];
                        if (weaponnode1.Name == "CraftedItem")
                        {
                            foreach (var pieceNode in weaponnode1.FirstChild.ChildNodes.Cast<XmlNode>())
                            {
                                if (pieceNode.Attributes!["id"].Value == node1.Attributes!["id"].Value)
                                {
                                    var newpiece = node1.Clone();
                                    newpiece.Attributes["id"].Value = "crpg_" + NameToId(weaponnode1.Attributes["name"].Value) + "_" + pieceNode.Attributes!["Type"].Value.ToLower();
                                    craftingPieceToAdd.Add(newpiece);

                                }
                            }
                        }
                    }

                    itemsDoc.LastChild.RemoveChild(node1);
                }
                foreach (var node in craftingPieceToAdd)
                {
                    itemsDoc.LastChild.AppendChild(node);
                }
            }
        }
        else if (itemsDoc.LastChild.Name == "Items")
        {
            for (int i = nodes1.Length - 1; i >= 0; i--)
            {
                var node1 = nodes1[i];
                if (node1.Name == "CraftedItem")
                {
                    node1.Attributes!["id"].Value = "crpg_" + NameToId(node1.Attributes["name"].Value);
                    foreach (var pieceNode in node1.FirstChild.ChildNodes.Cast<XmlNode>())
                    {
                        pieceNode.Attributes!["id"].Value = "crpg_" + NameToId(node1.Attributes["name"].Value) + "_" + pieceNode.Attributes!["Type"].Value.ToLower();
                    }
                }
            }
        }

        else if (itemsDoc.LastChild.Name == "CraftingTemplates")
        {
            for (int i = nodes1.Length - 1; i >= 0; i--)
            {
                List<XmlNode> usablePieceToAdd = new();
                var node1 = nodes1[i];
                var usablePieces = node1.SelectSingleNode("UsablePieces").ChildNodes.Cast<XmlNode>().ToArray();
                for (int k = usablePieces.Length - 1; k >= 0; k--)
                {
                    XmlNode usablePiece = usablePieces[k];
                    for (int p = weaponnodes1.Length - 1; p >= 0; p--)
                    {
                        var weaponnode1 = weaponnodes1[p];
                        if (weaponnode1.Name == "CraftedItem")
                        {
                            foreach (var pieceNode in weaponnode1.FirstChild.ChildNodes.Cast<XmlNode>())
                            {
                                if (pieceNode.Attributes!["id"].Value == usablePiece.Attributes!["piece_id"].Value)
                                {
                                    var newusablepiece = usablePiece.Clone();
                                    newusablepiece.Attributes["piece_id"].Value = "crpg_" + NameToId(weaponnode1.Attributes["name"].Value) + "_" + pieceNode.Attributes!["Type"].Value.ToLower();
                                    usablePieceToAdd.Add(newusablepiece);
                                }
                            }
                        }
                    }

                    node1.SelectSingleNode("UsablePieces").RemoveChild(usablePiece);
                }

                foreach (var node in usablePieceToAdd)
                {
                    node1.SelectSingleNode("UsablePieces").AppendChild(node);
                }
            }
        }
        else if (itemsDoc.LastChild.Name == "WeaponDescriptions")
        {
            for (int i = nodes1.Length - 1; i >= 0; i--)
            {
                List<XmlNode> availablePieceToAdd = new();
                var node1 = nodes1[i];
                var availablePieces = node1.SelectSingleNode("AvailablePieces").ChildNodes.Cast<XmlNode>().ToArray();
                for (int k = availablePieces.Length - 1; k >= 0; k--)
                {
                    XmlNode availablePiece = availablePieces[k];
                    for (int p = weaponnodes1.Length - 1; p >= 0; p--)
                    {
                        var weaponnode1 = weaponnodes1[p];
                        if (weaponnode1.Name == "CraftedItem")
                        {
                            foreach (var pieceNode in weaponnode1.FirstChild.ChildNodes.Cast<XmlNode>())
                            {
                                if (pieceNode.Attributes!["id"].Value == availablePiece.Attributes!["id"].Value)
                                {
                                    var newavailablepiece = availablePiece.Clone();
                                    newavailablepiece.Attributes["id"].Value = "crpg_" + NameToId(weaponnode1.Attributes["name"].Value) + "_" + pieceNode.Attributes!["Type"].Value.ToLower();
                                    availablePieceToAdd.Add(newavailablepiece);
                                }
                            }
                        }
                    }

                    node1.SelectSingleNode("AvailablePieces").RemoveChild(availablePiece);
                }

                foreach (var node in availablePieceToAdd)
                {
                    node1.SelectSingleNode("AvailablePieces").AppendChild(node);
                }
            }
        }
        return itemsDoc;
    }

    private static string PrefixWith(string prefix, string s)
    {
        return s.StartsWith(prefix, StringComparison.Ordinal) ? s : prefix + s;
    }
    public static string NameToId(string input)
    {
        string[] splitInput = input.Split('}');

        string partToConvert;

        // If the string contains '}', take the part after '}' else take the full string.
        if (splitInput.Length > 1)
        {
            partToConvert = splitInput[1];
        }
        else
        {
            partToConvert = splitInput[0];
        }

        // Trim leading/trailing white spaces, convert to lower case and replace spaces with underscores
        partToConvert = partToConvert.Trim().ToLower().Replace(' ', '_');

        return partToConvert;
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
        return 1.87f * (float)Math.Pow(armorPower, 1.8f) / bestArmorPower;
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
