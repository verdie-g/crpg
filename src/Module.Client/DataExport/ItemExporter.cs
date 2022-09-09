using System.Globalization;
using System.Xml;
using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Helpers.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Tableaus;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.DataExport;

internal class ItemExporter : IDataExporter
{
    private const string CraftingPiecesFilePath = "../../Modules/Native/ModuleData/crafting_pieces.xml";
    private const string WeaponDescriptionsFilePath = "../../Modules/Native/ModuleData/weapon_descriptions.xml";
    private const string CraftingTemplatesFilePath = "../../Modules/Native/ModuleData/crafting_templates.xml";
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
        "a_aserai_scale_b_shoulder_c", // Name conflict with aserai_scale_shoulder_a.
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
        "desert_scale_shoulders", // Name conflict with a_aserai_scale_b_shoulder_b rank 2.
        "eastern_leather_boots", // Name conflict with leather_boots.
        "empire_crown_v2", // Name conflict with empire_crown_west.
        "empire_horse_tournament", // Name conflict with empire_horse.
        "empire_lance_1_t3_blunt", // Name conflict with vlandia_lance_1_t3_blunt.
        "empire_sword_1_t2", // Name conflict with iron_spatha_sword_t2.
        "empire_sword_1_t2_blunt", // Name conflict with iron_spatha_sword_t2.
        "female_scarf", // Name conflict with scarf rank 1.
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
        "noble_pauldron_with_scarf", // Name conflict with noble_pauldron rank 2.
        "pack_camel_unmountable", // Can't be equipped.
        "pot", // Can't be equipped.
        "reinforced_kite_shield", // Name conflict with western_kite_shield rank 2.
        "reinforced_mail_mitten", // Name conflict with mail_mitten rank 2.
        "reinforced_padded_mitten", // Name conflict with padded_mitten rank 2.
        "southern_lamellar_armor", // Name conflict with desert_lamellar.
        "southern_lord_helmet", // Name conflict with desert_helmet rank 3.
        "storm_charger", // Extremely shit horse.
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
        "tournament_bolts", // Bolts with blunt damage -> Shotgun effect, over-powered.
        "vlandia_horse_tournament", // Name conflict with vlandia_horse.
        "vlandia_mace_3_t5", // Name conflict with pernach_mace_t3.
        "womens_headwrap_c", // Name conflict with head_wrapped.
        "wooden_sword_t2", // Name conflict with wooden_sword_t1.
        "woodland_throwing_axe_1_t1", // Name conflict with highland_throwing_axe_1_t2.
    };

    public async Task Export(string gitRepoPath)
    {
        string moduleDataPath = Path.Combine(gitRepoPath, "src/Module.Server/ModuleData");

        var game = Game.CreateGame(new MultiplayerGame(), new MultiplayerGameManager());
        game.Initialize();

        var craftingPiecesDoc = LoadMbDocument(CraftingPiecesFilePath);
        RegisterMbObjects<CraftingPiece>(craftingPiecesDoc, game);
        craftingPiecesDoc.Save(Path.Combine(moduleDataPath, Path.GetFileName(CraftingPiecesFilePath)));

        var weaponDescriptionsDoc = LoadMbDocument(WeaponDescriptionsFilePath);
        RegisterMbObjects<WeaponDescription>(weaponDescriptionsDoc, game);
        weaponDescriptionsDoc.Save(Path.Combine(moduleDataPath, Path.GetFileName(WeaponDescriptionsFilePath)));

        var craftingTemplatesDoc = LoadMbDocument(CraftingTemplatesFilePath);
        RegisterMbObjects<CraftingTemplate>(craftingTemplatesDoc, game);
        craftingTemplatesDoc.Save(Path.Combine(moduleDataPath, Path.GetFileName(CraftingTemplatesFilePath)));

        string moduleDataItemsPath = Path.Combine(moduleDataPath, "items");
        Directory.CreateDirectory(moduleDataItemsPath);

        var mbItems = Enumerable.Empty<ItemObject>();
        foreach (string filePath in Directory.EnumerateFiles(ItemFilesPath))
        {
            var itemsDoc = LoadMbDocument(filePath);
            RegisterMbObjects<ItemObject>(itemsDoc, game);
            mbItems = mbItems.Concat(DeserializeMbItems(itemsDoc, game));
            itemsDoc.Save(Path.Combine(moduleDataItemsPath, Path.GetFileName(filePath)));
        }

        mbItems = mbItems
            .DistinctBy(i => i.StringId)
            .OrderBy(i => i.StringId)
            .ToArray();
        var crpgItems = mbItems.Select(MbToCrpgItem);
        SerializeCrpgItems(crpgItems, Path.Combine(gitRepoPath, "data"));

        const string itemThumbnailsTempPath = "../../crpg-items";
        string itemThumbnailsPath = Path.Combine(gitRepoPath, "src/WebUI/public/items");

        Directory.CreateDirectory(itemThumbnailsTempPath);
        await GenerateItemsThumbnail(mbItems, itemThumbnailsTempPath);
        Directory.Delete(itemThumbnailsPath, recursive: true);
        Directory.Move(itemThumbnailsTempPath, itemThumbnailsPath);
    }

    private static CrpgItemCreation MbToCrpgItem(ItemObject mbItem)
    {
        CrpgItemCreation crpgItem = new()
        {
            Id = mbItem.StringId,
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

    private static IEnumerable<ItemObject> DeserializeMbItems(XmlDocument itemsDoc, Game game)
    {
        return itemsDoc
            .LastChild
            .ChildNodes
            .Cast<XmlNode>()
            .Select(itemNode =>
            {
                ItemObject item = new();
                item.Deserialize(game.ObjectManager, itemNode);
                return item;
            });
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

            // Remove test and blacklisted items
            if (node1.Name == "CraftedItem" || node1.Name == "Item")
            {
                string id = node1.Attributes!["id"].Value;
                string name = node1.Attributes!["name"].Value;
                ItemObject.ItemTypeEnum itemType = ItemObject.ItemTypeEnum.OneHandedWeapon; // A random valid item type.
                if (node1.Name == "Item")
                {
                    itemType = (ItemObject.ItemTypeEnum)Enum.Parse(typeof(ItemObject.ItemTypeEnum), node1.Attributes!["Type"].Value);
                }

                if (id.IndexOf("dummy", StringComparison.Ordinal) != -1
                    || name.IndexOf('_') != -1
                    || BlacklistedItemTypes.Contains(itemType)
                    || BlacklistedItems.Contains(id))
                {
                    node1.ParentNode!.RemoveChild(node1);
                    continue;
                }
            }

            node1.Attributes!["id"].Value = PrefixCrpg(node1.Attributes["id"].Value);
            if (node1.Name == "CraftedItem")
            {
                node1.Attributes!["crafting_template"].Value = PrefixCrpg(node1.Attributes["crafting_template"].Value);
                foreach (var pieceNode in node1.FirstChild.ChildNodes.Cast<XmlNode>())
                {
                    pieceNode.Attributes!["id"].Value = PrefixCrpg(pieceNode.Attributes["id"].Value);
                }
            }
            else if (node1.Name == "CraftingPiece")
            {
                ModifyChildNodesAttribute(node1, "BladeData/*", "damage_factor",
                    v => (float.Parse(v) * 0.35f).ToString(CultureInfo.InvariantCulture));
            }
            else if (node1.Name == "Item")
            {
                var type = (ItemObject.ItemTypeEnum)Enum.Parse(typeof(ItemObject.ItemTypeEnum), node1.Attributes!["Type"].Value);
                if (type == ItemObject.ItemTypeEnum.Horse)
                {
                    ModifyChildNodesAttribute(node1, "ItemComponent/Horse", "charge_damage",
                        v => ((int)(int.Parse(v) * 0.33f)).ToString(CultureInfo.InvariantCulture));
                    ModifyChildNodesAttribute(node1, "ItemComponent/Horse", "speed",
                        v => ((int)(int.Parse(v) * 0.75f)).ToString(CultureInfo.InvariantCulture));
                    ModifyChildNodesAttribute(node1, "ItemComponent/Horse", "extra_health",
                        v => (int.Parse(v) - 30).ToString(CultureInfo.InvariantCulture),
                        "0");
                }
                else if (type == ItemObject.ItemTypeEnum.HorseHarness)
                {
                    // Single player horse harness can go up to 78 amor when the highest you can find in native mp is 26
                    // so let's divide the armor by 3. The weight doesn't change because it's good enough.
                    ModifyChildNodesAttribute(node1, "ItemComponent/Armor", "body_armor",
                        v => ((int)(int.Parse(v) * 0.27f)).ToString(CultureInfo.InvariantCulture));
                }
                else if (type == ItemObject.ItemTypeEnum.Shield)
                {
                    ModifyChildNodesAttribute(node1, "ItemComponent/Weapon", "hit_points",
                        v => ((int)(int.Parse(v) * 0.5f)).ToString(CultureInfo.InvariantCulture));
                }
                else if (type == ItemObject.ItemTypeEnum.Bow)
                {
                    ModifyChildNodesAttribute(node1, "ItemComponent/Weapon", "thrust_damage",
                        v => ((int)(int.Parse(v) * 0.2f)).ToString(CultureInfo.InvariantCulture));
                }
                else if (type == ItemObject.ItemTypeEnum.Crossbow)
                {
                    ModifyChildNodesAttribute(node1, "ItemComponent/Weapon", "thrust_damage",
                        v => ((int)(int.Parse(v) * 0.67f)).ToString(CultureInfo.InvariantCulture));
                    ModifyChildNodesAttribute(node1, "ItemComponent/Weapon", "thrust_speed",
                        v => ((int)(int.Parse(v) * 0.50f)).ToString(CultureInfo.InvariantCulture));
                }
            }
            else if (node1.Name == "CraftingTemplate")
            {
                foreach (var node2 in node1.ChildNodes.Cast<XmlNode>())
                {
                    if (node2.Name == "WeaponDescriptions")
                    {
                        foreach (var weaponDescriptionNode in node2.ChildNodes.Cast<XmlNode>())
                        {
                            weaponDescriptionNode.Attributes!["id"].Value =
                                PrefixCrpg(weaponDescriptionNode.Attributes["id"].Value);
                        }
                    }
                    else if (node2.Name == "StatsData")
                    {
                        var weaponDescriptionAttr = node2.Attributes!["weapon_description"];
                        if (weaponDescriptionAttr != null)
                        {
                            weaponDescriptionAttr.Value = PrefixCrpg(weaponDescriptionAttr.Value);
                        }
                    }
                    else if (node2.Name == "UsablePieces")
                    {
                        var usablePieceNodes = node2.ChildNodes.Cast<XmlNode>().ToArray();
                        for (int j = 0; j < usablePieceNodes.Length; j += 1)
                        {
                            var usablePieceNode = usablePieceNodes[j];
                            var mpPieceAttr = usablePieceNode.Attributes!["piece_id"];
                            if (mpPieceAttr != null && mpPieceAttr.Value == "true")
                            {
                                node2.RemoveChild(usablePieceNode);
                                continue;
                            }

                            usablePieceNode.Attributes!["piece_id"].Value =
                                PrefixCrpg(usablePieceNode.Attributes["piece_id"].Value);
                        }
                    }
                }
            }
            else if (node1.Name == "WeaponDescription")
            {
                foreach (var node2 in node1.ChildNodes.Cast<XmlNode>())
                {
                    if (node2.Name == "AvailablePieces")
                    {
                        foreach (var availablePieceNode in node2.ChildNodes.Cast<XmlNode>())
                        {
                            availablePieceNode.Attributes!["id"].Value =
                                PrefixCrpg(availablePieceNode.Attributes["id"].Value);
                        }
                    }
                }
            }
        }

        return itemsDoc;
    }

    private static string PrefixCrpg(string s)
    {
        const string prefix = "crpg_";
        return s.StartsWith(prefix, StringComparison.Ordinal) ? prefix : prefix + s;
    }

    private static void ModifyChildNodesAttribute(XmlNode parentNode,
        string childXPath,
        string attributeName,
        Func<string, string> modify,
        string? defaultValue = null)
    {
        foreach (var childNode in parentNode.SelectNodes(childXPath)!.Cast<XmlNode>())
        {
            var attr = childNode.Attributes![attributeName];
            if (attr == null)
            {
                if (defaultValue == null)
                {
                    throw new KeyNotFoundException($"Attribute '{attributeName}' was not found and no default was provided");
                }

                attr = childNode.OwnerDocument!.CreateAttribute(attributeName);
                attr.Value = defaultValue;
                childNode.Attributes.Append(attr);
            }

            attr.Value = modify(attr.Value);
        }
    }

    private static void RegisterMbObjects<T>(XmlDocument doc, Game game) where T : MBObjectBase, new()
    {
        var nodes = doc.LastChild.ChildNodes.Cast<XmlNode>();
        foreach (var node in nodes)
        {
            T obj = new();
            obj.Deserialize(game.ObjectManager, node);
            game.ObjectManager.RegisterObject(obj);
        }
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
