using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common.Models;
using Crpg.Module.Helpers.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Multiplayer;
using TaleWorlds.MountAndBlade.View.Tableaus;

namespace Crpg.Module.DataExport;

internal class ItemExporter : IDataExporter
{
    private static readonly string[] ItemFilePaths =
    {
        "../../Modules/cRPG_Exporter/ModuleData/items/head_armors.xml",
        "../../Modules/cRPG_Exporter/ModuleData/items/shoulder_armors.xml",
        "../../Modules/cRPG_Exporter/ModuleData/items/body_armors.xml",
        "../../Modules/cRPG_Exporter/ModuleData/items/arm_armors.xml",
        "../../Modules/cRPG_Exporter/ModuleData/items/leg_armors.xml",
        "../../Modules/cRPG_Exporter/ModuleData/items/weapons.xml",
        "../../Modules/cRPG_Exporter/ModuleData/items/horses_and_others.xml",
        "../../Modules/cRPG_Exporter/ModuleData/items/shields.xml",
    };
    private static readonly string[] PiecesFilePaths =
    {
        "../../Modules/cRPG_Exporter/ModuleData/crafting_pieces.xml",
        "../../Modules/cRPG_Exporter/ModuleData/crafting_templates.xml",
        "../../Modules/cRPG_Exporter/ModuleData/weapon_descriptions.xml",
    };

    private static readonly Dictionary<int, (int speedbonus, int maneuverbonus, float healthbonusPercentage)> MountHeirloomBonus = new()
    {
        { 1, (1, 0, 6.2f) },
        { 2, (1, 1, 13f) },
        { 3, (1, 2, 19.8f) },
    };
    private static readonly Dictionary<int, int> HeadArmorHeirloomBonus = new()
    {
        { 1, 2 },
        { 2, 4 },
        { 3, 6 },
    };
    private static readonly Dictionary<int, (int bodyArmorBonus, int legArmorBonus, int armArmorBonus)> BodyArmorHeirloomBonus = new()
    {
        { 1, (2, 1, 1) },
        { 2, (4, 2, 2) },
        { 3, (6, 3, 3) },
    };
    private static readonly Dictionary<int, int> LegArmorHeirloomBonus = new()
    {
        { 1, 1 },
        { 2, 3 },
        { 3, 5 },
    };
    private static readonly Dictionary<int, (int bodyArmorBonus, int armArmorBonus)> ShoulderArmorHeirloomBonus = new()
    {
        { 1, (2, 0) },
        { 2, (3, 0) },
        { 3, (4, 1) },
    };
    private static readonly Dictionary<int, int> ArmArmorHeirloomBonus = new()
    {
        { 1, 1 },
        { 2, 2 },
        { 3, 4 },
    };
    private static readonly Dictionary<int, int> HorseArmorHeirloomBonus = new()
    {
        { 1, 3 },
        { 2, 6 },
        { 3, 9 },
    };
    private static readonly Dictionary<int, (int damageBonus, int amountBonusPercentage)> CutArrowHeirloomBonus = new()
    {
        { 1, (0, 25) },
        { 2, (1, 25) },
        { 3, (2, 25) },
    };
    private static readonly Dictionary<int, (int damageBonus, int amountBonusPercentage)> PierceArrowHeirloomBonus = new()
    {
        { 1, (0, 15) },
        { 2, (0, 30) },
        { 3, (1, 30) },
    };
    private static readonly Dictionary<int, (int damageBonus, int amountBonusPercentage)> BluntArrowHeirloomBonus = new()
    {
        { 1, (0, 20) },
        { 2, (0, 40) },
        { 3, (1, 40) },
    };
    private static readonly Dictionary<int, (int damageBonus, int amountBonusPercentage)> CutBoltHeirloomBonus = new()
    {
        { 1, (2, 10) },
        { 2, (4, 20) },
        { 3, (6, 30) },
    };
    private static readonly Dictionary<int, (int damageBonus, int amountBonusPercentage)> PierceBoltHeirloomBonus = new()
    {
        { 1, (1, 10) },
        { 2, (2, 20) },
        { 3, (3, 30) },
    };
    private static readonly Dictionary<int, (int damageBonus, int amountBonusPercentage)> BluntBoltHeirloomBonus = new()
    {
        { 1, (0, 10) },
        { 2, (0, 20) },
        { 3, (1, 20) },
    };
    private static readonly Dictionary<int, (int damageBonus, int accuracyBonus, int missileSpeedBonus, int reloadSpeedBonus, int aimSpeedBonus)> CrossbowHeirloomBonus = new()
    {
        { 1, (1, 1, 0, 2, 1) },
        { 2, (2, 2, 1, 4, 2) },
        { 3, (3, 3, 2, 5, 3) },
    };
    private static readonly Dictionary<int, (int damageBonus, int accuracyBonus, int missileSpeedBonus, int reloadSpeedBonus, int aimSpeedBonus)> LightCrossbowHeirloomBonus = new()
    {
        { 1, (0, 2, 1, 3, 3) },
        { 2, (1, 2, 1, 4, 5) },
        { 3, (2, 2, 2, 4, 5) },
    };
    private static readonly Dictionary<int, (int damageBonus, int accuracyBonus, int missileSpeedBonus, int reloadSpeedBonus, int aimSpeedBonus)> LongBowHeirloomBonus = new()
    {
        { 1, (0, 2, 2, 3, 1) },
        { 2, (0, 4, 4, 4, 3) },
        { 3, (1, 4, 4, 4, 3) },
    };
    private static readonly Dictionary<int, (int damageBonus, int accuracyBonus, int missileSpeedBonus, int reloadSpeedBonus, int aimSpeedBonus)> BowHeirloomBonus = new()
    {
        { 1, (0, 0, 9, 3, 2) },
        { 2, (0, 3, 9, 3, 6) },
        { 3, (0, 9, 9, 3, 6) },
    };
    private static readonly Dictionary<int, (int bonusHealthPercentage, int bodyArmorPercentage)> ShieldHeirloomBonus = new()
    {
        { 1, (8, 3) },
        { 2, (16, 6) },
        { 3, (24, 9) },
    };

    public async Task ComputeAutoStats(string gitRepoPath)
    {
        foreach (string filePath in ItemFilePaths)
        {
            var itemsDoc = XmlComputeAutoStats(filePath);
            itemsDoc.Save(Path.Combine("../../Modules/cRPG_Exporter/ModuleData/items", Path.GetFileName(filePath)));
        }
        foreach (string filePath in PiecesFilePaths)
        {
            var itemsDoc = XmlComputeAutoStats(filePath);
            itemsDoc.Save(Path.Combine("../../Modules/cRPG_Exporter/ModuleData", Path.GetFileName(filePath)));
        }
    }

    public async Task RefundShield(string gitRepoPath)
    {
        List<ItemObject.ItemTypeEnum> typesToRefund = new()
            {
                ItemObject.ItemTypeEnum.Shield,
            };
        var itemsDoc = XmlRefundItemType("../../Modules/cRPG_Exporter/ModuleData/items/shields.xml", typesToRefund);
        itemsDoc.Save(Path.Combine("../../Modules/cRPG_Exporter/ModuleData/items", Path.GetFileName("../../Modules/cRPG_Exporter/ModuleData/items/shields.xml")));
    }

    public async Task RefundBow(string gitRepoPath)
    {
        List<ItemObject.ItemTypeEnum> typesToRefund = new()
        {
            ItemObject.ItemTypeEnum.Bow,
            ItemObject.ItemTypeEnum.Arrows,
        };
        var itemsDoc = XmlRefundItemType("../../Modules/cRPG_Exporter/ModuleData/items/weapons.xml", typesToRefund);
        itemsDoc.Save(Path.Combine("../../Modules/cRPG_Exporter/ModuleData/items", Path.GetFileName("../../Modules/cRPG_Exporter/ModuleData/items/weapons.xml")));
        var itemsDoc2 = XmlRefundItemType("../../Modules/cRPG_Exporter/ModuleData/items/weapons.xml", typesToRefund);
        itemsDoc2.Save(Path.Combine("../../Modules/cRPG_Exporter/ModuleData/items", Path.GetFileName("../../Modules/cRPG_Exporter/ModuleData/items/weapons.xml")));
    }

    public async Task RefundCrossbow(string gitRepoPath)
    {
        List<ItemObject.ItemTypeEnum> typesToRefund = new()
        {
            ItemObject.ItemTypeEnum.Crossbow,
            ItemObject.ItemTypeEnum.Bolts,
        };
        var itemsDoc = XmlRefundItemType("../../Modules/cRPG_Exporter/ModuleData/items/weapons.xml", typesToRefund);
        itemsDoc.Save(Path.Combine("../../Modules/cRPG_Exporter/ModuleData/items", Path.GetFileName("../../Modules/cRPG_Exporter/ModuleData/items/weapons.xml")));
        var itemsDoc2 = XmlRefundItemType("../../Modules/cRPG_Exporter/ModuleData/items/weapons.xml", typesToRefund);
        itemsDoc2.Save(Path.Combine("../../Modules/cRPG_Exporter/ModuleData/items", Path.GetFileName("../../Modules/cRPG_Exporter/ModuleData/items/weapons.xml")));
    }

    public async Task RefundArmor(string gitRepoPath)
    {
        List<ItemObject.ItemTypeEnum> typesToRefund = new()
        {
            ItemObject.ItemTypeEnum.BodyArmor,
            ItemObject.ItemTypeEnum.LegArmor,
            ItemObject.ItemTypeEnum.HeadArmor,
            ItemObject.ItemTypeEnum.HandArmor,
            ItemObject.ItemTypeEnum.ChestArmor,
            ItemObject.ItemTypeEnum.Cape,
        };
        foreach (string filepath in ItemFilePaths)
        {
            var itemsDoc = XmlRefundItemType(filepath, typesToRefund);
            itemsDoc.Save(Path.Combine("../../Modules/cRPG_Exporter/ModuleData/items", Path.GetFileName(filepath)));
        }
    }

    public async Task RefundThrowing(string gitRepoPath)
    {
        List<ItemObject.ItemTypeEnum> typesToRefund = new()
        {
            ItemObject.ItemTypeEnum.Thrown,
        };
        var itemsDoc = XmlRefundItemType("../../Modules/cRPG_Exporter/ModuleData/items/weapons.xml", typesToRefund);
        itemsDoc.Save(Path.Combine("../../Modules/cRPG_Exporter/ModuleData/items", Path.GetFileName("../../Modules/cRPG_Exporter/ModuleData/items/weapons.xml")));
        var itemsDoc2 = XmlRefundCraftingTemplate("../../Modules/cRPG_Exporter/ModuleData/items/weapons.xml", "crpg_ThrowingKnife");
        itemsDoc2.Save(Path.Combine("../../Modules/cRPG_Exporter/ModuleData/items", Path.GetFileName("../../Modules/cRPG_Exporter/ModuleData/items/weapons.xml")));
        var itemsDoc3 = XmlRefundCraftingTemplate("../../Modules/cRPG_Exporter/ModuleData/items/weapons.xml", "crpg_ThrowingAxe");
        itemsDoc3.Save(Path.Combine("../../Modules/cRPG_Exporter/ModuleData/items", Path.GetFileName("../../Modules/cRPG_Exporter/ModuleData/items/weapons.xml")));
        var itemsDoc4 = XmlRefundCraftingTemplate("../../Modules/cRPG_Exporter/ModuleData/items/weapons.xml", "crpg_Javelin");
        itemsDoc4.Save(Path.Combine("../../Modules/cRPG_Exporter/ModuleData/items", Path.GetFileName("../../Modules/cRPG_Exporter/ModuleData/items/weapons.xml")));
    }

    public async Task RefundCav(string gitRepoPath)
    {
        List<ItemObject.ItemTypeEnum> typesToRefund = new()
        {
            ItemObject.ItemTypeEnum.Horse,
            ItemObject.ItemTypeEnum.HorseHarness,
        };
        var itemsDoc = XmlRefundItemType("../../Modules/cRPG_Exporter/ModuleData/items/horses_and_others.xml", typesToRefund);
        itemsDoc.Save(Path.Combine("../../Modules/cRPG_Exporter/ModuleData/items", Path.GetFileName("../../Modules/cRPG_Exporter/ModuleData/items/horses_and_others.xml")));
    }

    public async Task Scale(string gitRepoPath)
    {
        var itemsDoc = XmlScaleClass("../../Modules/cRPG_Exporter/ModuleData/items/weapons.xml", ItemObject.ItemTypeEnum.Bow);
        itemsDoc.Save(Path.Combine("../../Modules/cRPG_Exporter/ModuleData/items", Path.GetFileName("../../Modules/cRPG_Exporter/ModuleData/items/weapons.xml")));
    }

    public async Task Export(string gitRepoPath)
    {
        Debug.Print("user clicked on export");
        var game = Game.CreateGame(new MultiplayerGame(), new MultiplayerGameManager());
        game.Initialize();
        var mbItems = game.ObjectManager.GetObjectTypeList<ItemObject>()
            .Where(i => i.StringId.StartsWith("crpg_"))
            .DistinctBy(i => i.StringId)
            .OrderBy(i => i.StringId)
            .ToArray();
        var crpgItems = mbItems.Select(MbToCrpgItem);
        SerializeCrpgItems(crpgItems, Path.Combine("../../Modules/cRPG_Exporter/ModuleData"));
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
        string itemThumbnailsPath = Path.Combine("../../Modules/cRPG_Exporter/images");
        Directory.CreateDirectory(itemThumbnailsPath);
        await GenerateItemsThumbnail(mbItems, itemThumbnailsPath);
    }

    private static CrpgItem MbToCrpgItem(ItemObject mbItem)
    {
        CrpgItem crpgItem = new()
        {
            Id = mbItem.StringId,
            BaseId = mbItem.StringId.Substring(0, mbItem.StringId.Length - 3),
            Name = mbItem.Name.ToString(),
            Culture = MbToCrpgCulture(mbItem.Culture),
            Type = MbToCrpgItemType(mbItem.Type),
            Price = mbItem.Value,
            Weight = mbItem.Weight,
            Rank = mbItem.Type == ItemObject.ItemTypeEnum.Banner ? 0 : int.Parse(mbItem.StringId.Split('_').Last().Substring(1)),
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

    private static XmlDocument XmlRefundItemType(string filePath, List<ItemObject.ItemTypeEnum> typesToRefund)
    {
        XmlDocument itemsDoc = new();
        using (var r = XmlReader.Create(filePath, new XmlReaderSettings { IgnoreComments = true }))
        {
            itemsDoc.Load(r);
        }

        var nodes1 = itemsDoc.LastChild.ChildNodes.Cast<XmlNode>().ToArray();
        for (int i = 0; i < nodes1.Length; i += 1)
        {
            var node1 = nodes1[i];
            if (node1.Name == "Item")
            {
                var type = (ItemObject.ItemTypeEnum)Enum.Parse(typeof(ItemObject.ItemTypeEnum), node1.Attributes!["Type"].Value);
                if (typesToRefund.Contains(type))
                {
                    node1.Attributes!["id"].Value = ToggleRefund(node1.Attributes!["id"].Value);
                }
            }
        }

        return itemsDoc;
    }

    private static XmlDocument XmlRefundCraftingTemplate(string filePath, string craftingTemplateToRefund)
    {
        XmlDocument itemsDoc = new();
        using (var r = XmlReader.Create(filePath, new XmlReaderSettings { IgnoreComments = true }))
        {
            itemsDoc.Load(r);
        }

        var nodes1 = itemsDoc.LastChild.ChildNodes.Cast<XmlNode>().ToArray();
        for (int i = 0; i < nodes1.Length; i += 1)
        {
            var node1 = nodes1[i];
            var craftingTemplateAttribute = node1.Attributes["crafting_template"];
            if (craftingTemplateAttribute == null)
            {
                continue;
            }

            if (craftingTemplateToRefund == craftingTemplateAttribute.Value)
            {
                node1.Attributes!["id"].Value = ToggleRefund(node1.Attributes!["id"].Value);
            }
        }

        return itemsDoc;
    }

    private static XmlDocument XmlScaleClass(string filePath, ItemObject.ItemTypeEnum typeToRefund)
    {
        XmlDocument itemsDoc = new();
        using (var r = XmlReader.Create(filePath, new XmlReaderSettings { IgnoreComments = true }))
        {
            itemsDoc.Load(r);
        }

        var nodes1 = itemsDoc.LastChild.ChildNodes.Cast<XmlNode>().ToArray();
        for (int i = 0; i < nodes1.Length; i += 1)
        {
            var node1 = nodes1[i];
            if (node1.Name == "Item")
            {
                var type = (ItemObject.ItemTypeEnum)Enum.Parse(typeof(ItemObject.ItemTypeEnum), node1.Attributes!["Type"].Value);
                if (type == typeToRefund)
                {
                    ModifyChildNodesAttribute(node1, "ItemComponent/Weapon", "missile_speed", x => ((int)(int.Parse(x) * 0.9f)).ToString());
                }
            }
        }

        return itemsDoc;
    }

    private static XmlDocument XmlComputeAutoStats(string filePath)
    {
        XmlDocument itemsDoc = new();
        using (var r = XmlReader.Create(filePath, new XmlReaderSettings { IgnoreComments = true }))
        {
            itemsDoc.Load(r);
        }

        Dictionary<string, XmlNode> baseItem = new();
        Dictionary<(string craftingtemplate, string usablepieceid), List<XmlNode>> upgradedUsablePiece = new();
        Dictionary<(string craftingtemplate, string usablepieceid), List<XmlNode>> upgradedAvailablePiece = new();
        Dictionary<string, List<XmlNode>> upgradedItem = new();
        var nodes1 = itemsDoc.LastChild.ChildNodes.Cast<XmlNode>().ToArray();
        Debug.Print("adding to dictionary h0 items");
        for (int i = 0; i < nodes1.Length; i += 1)
        {
                var node1 = nodes1[i];
                if (node1.Name == "Item" || node1.Name == "CraftedItem" || node1.Name == "CraftingPiece")
                {
                    int heirloomLevel = IdToHeirloomLevel(node1.Attributes!["id"].Value);
                    string baseId = node1.Attributes!["id"].Value.Remove(node1.Attributes!["id"].Value.Length - 2);
                    if (heirloomLevel == 0)
                    {
                        if (!baseItem.ContainsKey(baseId))
                        {
                            baseItem[baseId] = node1;
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                        if (!upgradedItem.ContainsKey(baseId))
                        {
                            List<XmlNode> upgradedItemNodes = new()
                            {
                                node1,
                            };

                            upgradedItem[baseId] = upgradedItemNodes;
                        }
                        else
                        {
                        upgradedItem[baseId].Add(node1);
                        }
                    }
                }

                // weapon descriptions
                if (node1.Name == "WeaponDescription")
                {
                    Debug.Print($"Parsing Weapon Description {node1.Attributes!["id"].Value!}");
                    var availablePiecesNodes1 = node1.SelectSingleNode("AvailablePieces").ChildNodes.Cast<XmlNode>().ToArray();
                    for (int j = 0; j < availablePiecesNodes1.Length; j += 1)
                    {
                        var availablePieceNode = availablePiecesNodes1[j];
                        int heirloomLevel = IdToHeirloomLevel(availablePieceNode.Attributes!["id"].Value);
                        string baseId = availablePieceNode.Attributes!["id"].Value.Remove(availablePieceNode.Attributes!["id"].Value.Length - 2);
                        if (heirloomLevel == 0)
                        {
                        }
                        else
                        {
                            if (!upgradedAvailablePiece.ContainsKey((availablePieceNode.ParentNode.ParentNode.Attributes["id"]!.Value, baseId)))
                            {
                                List<XmlNode> upgradedItemNodes = new()
                                    {
                                        availablePieceNode,
                                    };

                                upgradedAvailablePiece[(availablePieceNode.ParentNode.ParentNode.Attributes["id"]!.Value, baseId)] = upgradedItemNodes;
                            }
                            else
                            {
                                upgradedAvailablePiece[(availablePieceNode.ParentNode.ParentNode.Attributes["id"]!.Value, baseId)].Add(availablePieceNode);
                            }
                        }
                    }
                }

                // Crafting Template
                if (node1.Name == "CraftingTemplate")
                {
                    Debug.Print($"Parsing CraftingTemplate {node1.Attributes!["id"].Value!}");

                    var usablePiecesNodes1 = node1.SelectSingleNode("UsablePieces").ChildNodes.Cast<XmlNode>().ToArray();

                    Debug.Print($"{node1.Attributes!["id"].Value!} has {usablePiecesNodes1.Count()} usablepieces");

                    for (int j = 0; j < usablePiecesNodes1.Length; j += 1)
                    {
                        var usablePieceNode = usablePiecesNodes1[j];
                        Debug.Print($"checking  {usablePieceNode.Attributes["piece_id"]!.Value}");
                        int heirloomLevel = IdToHeirloomLevel(usablePieceNode.Attributes!["piece_id"].Value);
                        string baseId = usablePieceNode.Attributes!["piece_id"].Value.Remove(usablePieceNode.Attributes!["piece_id"].Value.Length - 2);
                        if (heirloomLevel == 0)
                        {
                        }
                        else
                        {
                            if (!upgradedUsablePiece.ContainsKey((usablePieceNode.ParentNode.ParentNode.Attributes["id"]!.Value, baseId)))
                            {
                                List<XmlNode> upgradedItemNodes = new()
                                {
                                    usablePieceNode,
                                };

                                upgradedUsablePiece[(usablePieceNode.ParentNode.ParentNode.Attributes["id"]!.Value, baseId)] = upgradedItemNodes;
                            }
                            else
                            {
                                upgradedUsablePiece[(usablePieceNode.ParentNode.ParentNode.Attributes["id"]!.Value, baseId)].Add(usablePieceNode);
                            }
                        }
                    }

                    Debug.Print($"Finished Parsing CraftingTemplate {node1.Attributes!["id"].Value!}");
            }
        }

        Debug.Print($"Dictionary upgradedUsablePiece has {upgradedUsablePiece.Count} items");
        Debug.Print($"Dictionary upgradedAvailablePiece has {upgradedAvailablePiece.Count} items");
        for (int i = 0; i < nodes1.Length; i += 1)
        {
            var node1 = nodes1[i];
            if (node1.Name == "Item")
            {
                int heirloomLevel = IdToHeirloomLevel(node1.Attributes!["id"].Value);
                string baseId = node1.Attributes!["id"].Value.Remove(node1.Attributes!["id"].Value.Length - 2);
                if (heirloomLevel == 0)
                {
                    if (!upgradedItem.ContainsKey(baseId))
                    {
                        XmlNode newNodeh1 = node1.CloneNode(true);
                        XmlNode newNodeh2 = node1.CloneNode(true);
                        XmlNode newNodeh3 = node1.CloneNode(true);
                        newNodeh1.Attributes!["id"].Value = baseId + "h1";
                        newNodeh2.Attributes!["id"].Value = baseId + "h2";
                        newNodeh3.Attributes!["id"].Value = baseId + "h3";
                        node1.ParentNode.InsertAfter(newNodeh1, node1);
                        node1.ParentNode.InsertAfter(newNodeh2, newNodeh1);
                        node1.ParentNode.InsertAfter(newNodeh3, newNodeh2);                 }
                }
            }
            else if (node1.Name == "CraftedItem")
            {
                int heirloomLevel = IdToHeirloomLevel(node1.Attributes!["id"].Value);
                string baseId = node1.Attributes!["id"].Value.Remove(node1.Attributes!["id"].Value.Length - 2);
                if (!upgradedItem.ContainsKey(baseId))
                {
                    XmlNode newNodeh1 = node1.CloneNode(true);
                    XmlNode newNodeh2 = node1.CloneNode(true);
                    XmlNode newNodeh3 = node1.CloneNode(true);
                    newNodeh1.Attributes!["id"].Value = baseId + "h1";
                    newNodeh2.Attributes!["id"].Value = baseId + "h2";
                    newNodeh3.Attributes!["id"].Value = baseId + "h3";
                    node1.ParentNode.InsertAfter(newNodeh1, node1);
                    node1.ParentNode.InsertAfter(newNodeh2, newNodeh1);
                    node1.ParentNode.InsertAfter(newNodeh3, newNodeh2);

                    List<XmlNode> upgradedNodes = new List<XmlNode>()
                {
                    newNodeh1,
                    newNodeh2,
                    newNodeh3,
                };

                    foreach (XmlNode upgradedNode in upgradedNodes)
                    {
                        foreach (XmlNode pieceNode in upgradedNode.LastChild.ChildNodes)
                        {
                            string piecebaseId = pieceNode.Attributes!["id"].Value.Remove(pieceNode.Attributes!["id"].Value.Length - 2);
                            pieceNode.Attributes!["id"].Value = piecebaseId + "h" + IdToHeirloomLevel(upgradedNode.Attributes!["id"].Value).ToString();
                        }
                    }
                }
            }
            else if (node1.Name == "CraftingPiece")
            {
                int heirloomLevel = IdToHeirloomLevel(node1.Attributes!["id"].Value);
                string baseId = node1.Attributes!["id"].Value.Remove(node1.Attributes!["id"].Value.Length - 2);
                if (!upgradedItem.ContainsKey(baseId))
                {
                    XmlNode newNodeh1 = node1.CloneNode(true);
                    XmlNode newNodeh2 = node1.CloneNode(true);
                    XmlNode newNodeh3 = node1.CloneNode(true);
                    newNodeh1.Attributes!["id"].Value = baseId + "h1";
                    newNodeh2.Attributes!["id"].Value = baseId + "h2";
                    newNodeh3.Attributes!["id"].Value = baseId + "h3";
                    node1.ParentNode.InsertAfter(newNodeh1, node1);
                    node1.ParentNode.InsertAfter(newNodeh2, newNodeh1);
                    node1.ParentNode.InsertAfter(newNodeh3, newNodeh2);
                }
            }
            else if (node1.Name == "WeaponDescription")
            {
                var availablePiecesNodes1 = node1.SelectSingleNode("AvailablePieces").ChildNodes.Cast<XmlNode>().ToArray();
                for (int j = 0; j < availablePiecesNodes1.Length; j += 1)
                {
                    var availablePieceNode = availablePiecesNodes1[j];
                    int heirloomLevel = IdToHeirloomLevel(availablePieceNode.Attributes!["id"].Value);
                    string baseId = availablePieceNode.Attributes!["id"].Value.Remove(availablePieceNode.Attributes!["id"].Value.Length - 2);
                    if (!upgradedAvailablePiece.ContainsKey((availablePieceNode.ParentNode.ParentNode.Attributes["id"]!.Value, baseId)))
                    {
                        XmlNode newNodeh1 = availablePieceNode.CloneNode(true);
                        XmlNode newNodeh2 = availablePieceNode.CloneNode(true);
                        XmlNode newNodeh3 = availablePieceNode.CloneNode(true);
                        newNodeh1.Attributes!["id"].Value = baseId + "h1";
                        newNodeh2.Attributes!["id"].Value = baseId + "h2";
                        newNodeh3.Attributes!["id"].Value = baseId + "h3";
                        availablePieceNode.ParentNode.InsertAfter(newNodeh1, availablePieceNode);
                        availablePieceNode.ParentNode.InsertAfter(newNodeh2, newNodeh1);
                        availablePieceNode.ParentNode.InsertAfter(newNodeh3, newNodeh2);
                    }
                }
            }
            else if (node1.Name == "CraftingTemplate")
            {
                var usablePiecesNodes1 = node1.SelectSingleNode("UsablePieces").ChildNodes.Cast<XmlNode>().ToArray();
                Debug.Print($"Parsing to clone CraftingTemplate {node1.Attributes!["id"].Value!}");
                Debug.Print($"{node1.Attributes!["id"].Value!} has {usablePiecesNodes1.Count()} usablepieces");
                for (int j = 0; j < usablePiecesNodes1.Length; j += 1)
                {
                    var usablePieceNode = usablePiecesNodes1[j];
                    Debug.Print($"checking to clone {usablePieceNode.Attributes["piece_id"].Value}");
                    int heirloomLevel = IdToHeirloomLevel(usablePieceNode.Attributes!["piece_id"].Value);
                    string baseId = usablePieceNode.Attributes!["piece_id"].Value.Remove(usablePieceNode.Attributes!["piece_id"].Value.Length - 2);
                    if (!upgradedUsablePiece.ContainsKey((usablePieceNode.ParentNode.ParentNode.Attributes["id"]!.Value, baseId)))
                    {
                        Debug.Print($"cloning {usablePieceNode.Attributes["piece_id"].Value}");
                        XmlNode newNodeh1 = usablePieceNode.CloneNode(true);
                        XmlNode newNodeh2 = usablePieceNode.CloneNode(true);
                        XmlNode newNodeh3 = usablePieceNode.CloneNode(true);
                        newNodeh1.Attributes!["piece_id"].Value = baseId + "h1";
                        newNodeh2.Attributes!["piece_id"].Value = baseId + "h2";
                        newNodeh3.Attributes!["piece_id"].Value = baseId + "h3";
                        usablePieceNode.ParentNode.InsertAfter(newNodeh1, usablePieceNode);
                        usablePieceNode.ParentNode.InsertAfter(newNodeh2, newNodeh1);
                        usablePieceNode.ParentNode.InsertAfter(newNodeh3, newNodeh2);
                    }
                }
            }
        }

        Debug.Print($"Dictionary has {baseItem.Count} items");
        for (int i = 0; i < nodes1.Length; i += 1)
        {
            var node1 = nodes1[i];
            if (node1.Name == "Item")
            {
                int heirloomLevel = IdToHeirloomLevel(node1.Attributes!["id"].Value);
                string baseId = node1.Attributes!["id"].Value.Remove(node1.Attributes!["id"].Value.Length - 2);
                if (heirloomLevel == 0)
                {
                    continue;
                }
                else
                {
                    XmlNode levelZeroItemNode = baseItem[baseId];
                }

                var nonHeirloomNode = baseItem[baseId];
                node1.Attributes["name"].Value = nonHeirloomNode.Attributes["name"].Value + $" +{heirloomLevel}";
                var type = (ItemObject.ItemTypeEnum)Enum.Parse(typeof(ItemObject.ItemTypeEnum), node1.Attributes!["Type"].Value);
                Debug.Print($"now doing {node1.Attributes["id"].Value} which is derived from {nonHeirloomNode.Attributes["id"].Value} of type {type.ToString()}");
                switch (type)
                {
                    case ItemObject.ItemTypeEnum.Horse:

                        if (MountHeirloomBonus.TryGetValue(heirloomLevel, out var newMount))
                        {
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Horse", "maneuver", newMount.maneuverbonus);
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Horse", "speed", newMount.speedbonus);
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Horse", "extra_health",0 , ihatemountsPercentage: newMount.healthbonusPercentage);
                        }

                        break;

                    case ItemObject.ItemTypeEnum.Arrows:

                        var arrowDamageType = (DamageTypes)Enum.Parse(typeof(DamageTypes), node1.SelectNodes("ItemComponent/Weapon")!.Cast<XmlNode>().Last().Attributes!["thrust_damage_type"].Value);
                        var relevantArrowDictionary = arrowDamageType switch
                        {
                            DamageTypes.Cut => CutArrowHeirloomBonus,
                            DamageTypes.Pierce => PierceArrowHeirloomBonus,
                            DamageTypes.Blunt => BluntArrowHeirloomBonus,
                            _ => throw new NotImplementedException(),
                        };
                        if (relevantArrowDictionary.TryGetValue(heirloomLevel, out var newArrow))
                        {
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "thrust_damage", newArrow.damageBonus);
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "stack_amount", 0, bonusPercentage: newArrow.amountBonusPercentage);
                        }

                        break;

                    case ItemObject.ItemTypeEnum.Bolts:
                        var boltDamageType = (DamageTypes)Enum.Parse(typeof(DamageTypes), node1.SelectNodes("ItemComponent/Weapon")!.Cast<XmlNode>().Last().Attributes!["thrust_damage_type"].Value);
                        var relevantBoltDictionary = boltDamageType switch
                        {
                            DamageTypes.Cut => CutBoltHeirloomBonus,
                            DamageTypes.Pierce => PierceBoltHeirloomBonus,
                            DamageTypes.Blunt => BluntBoltHeirloomBonus,
                            _ => throw new NotImplementedException(),
                        };

                        if (relevantBoltDictionary.TryGetValue(heirloomLevel, out var newBolt))
                        {
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "thrust_damage", newBolt.damageBonus);
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "stack_amount", 0, bonusPercentage: newBolt.amountBonusPercentage);
                        }

                        break;

                    case ItemObject.ItemTypeEnum.Shield:
                        if (ShieldHeirloomBonus.TryGetValue(heirloomLevel, out var newShield))
                        {
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "hit_points", 0, bonusPercentage: newShield.bonusHealthPercentage);
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "body_armor", 0, bonusPercentage: newShield.bodyArmorPercentage);
                        }

                        break;

                    case ItemObject.ItemTypeEnum.Bow:
                        string bowItemUsage = node1.SelectSingleNode("ItemComponent/Weapon").Attributes["item_usage"].Value;
                        if (bowItemUsage == "long_bow")
                        {
                            if (LongBowHeirloomBonus.TryGetValue(heirloomLevel, out var newBow))
                            {
                                ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "thrust_damage", newBow.damageBonus);
                                ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "speed_rating", newBow.reloadSpeedBonus);
                                ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "thrust_speed", newBow.aimSpeedBonus);
                                ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "accuracy", newBow.accuracyBonus);
                                ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "missile_speed", newBow.missileSpeedBonus);
                            }
                        }
                        else
                        {
                            if (BowHeirloomBonus.TryGetValue(heirloomLevel, out var newBow))
                            {
                                ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "thrust_damage", newBow.damageBonus);
                                ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "speed_rating", newBow.reloadSpeedBonus);
                                ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "thrust_speed", newBow.aimSpeedBonus);
                                ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "accuracy", newBow.accuracyBonus);
                                ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "missile_speed", newBow.missileSpeedBonus);
                            }
                        }

                        break;

                    case ItemObject.ItemTypeEnum.Crossbow:
                        string crossbowItemUsage = node1.SelectSingleNode("ItemComponent/Weapon").Attributes["item_usage"].Value;

                        if (crossbowItemUsage == "crossbow")
                        {
                            if (CrossbowHeirloomBonus.TryGetValue(heirloomLevel, out var newCrossbow))
                            {
                                ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "thrust_damage", newCrossbow.damageBonus);
                                ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "speed_rating", newCrossbow.reloadSpeedBonus);
                                ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "thrust_speed", newCrossbow.aimSpeedBonus);
                                ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "accuracy", newCrossbow.accuracyBonus);
                                ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "missile_speed", newCrossbow.missileSpeedBonus);
                            }
                        }
                        else
                        {
                            if (LightCrossbowHeirloomBonus.TryGetValue(heirloomLevel, out var newCrossbow))
                            {
                                ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "thrust_damage", newCrossbow.damageBonus);
                                ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "speed_rating", newCrossbow.reloadSpeedBonus);
                                ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "thrust_speed", newCrossbow.aimSpeedBonus);
                                ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "accuracy", newCrossbow.accuracyBonus);
                                ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "missile_speed", newCrossbow.missileSpeedBonus);
                            }
                        }

                        break;

                    case ItemObject.ItemTypeEnum.HeadArmor:

                        if (HeadArmorHeirloomBonus.TryGetValue(heirloomLevel, out int newHeadArmor))
                        {
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Armor", "head_armor", newHeadArmor);
                        }

                        break;

                    case ItemObject.ItemTypeEnum.BodyArmor:

                        if (BodyArmorHeirloomBonus.TryGetValue(heirloomLevel, out var newBodyArmor))
                        {
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Armor", "body_armor", newBodyArmor.bodyArmorBonus);
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Armor", "leg_armor", newBodyArmor.legArmorBonus);
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Armor", "arm_armor", newBodyArmor.armArmorBonus);
                        }

                        break;

                    case ItemObject.ItemTypeEnum.LegArmor:

                        if (LegArmorHeirloomBonus.TryGetValue(heirloomLevel, out int newLegArmor))
                        {
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Armor", "leg_armor", newLegArmor);
                        }

                        break;

                    case ItemObject.ItemTypeEnum.HandArmor:

                        if (ArmArmorHeirloomBonus.TryGetValue(heirloomLevel, out int newArmArmor))
                        {
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Armor", "arm_armor", newArmArmor);
                        }

                        break;

                    case ItemObject.ItemTypeEnum.Cape:

                        if (ShoulderArmorHeirloomBonus.TryGetValue(heirloomLevel, out var newShoulderArmor))
                        {
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Armor", "body_armor", newShoulderArmor.bodyArmorBonus, defaultValue: "0");
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Armor", "arm_armor", newShoulderArmor.armArmorBonus, defaultValue: "0");
                        }

                        break;

                    case ItemObject.ItemTypeEnum.HorseHarness:

                        if (HorseArmorHeirloomBonus.TryGetValue(heirloomLevel, out int newHorseArmor))
                        {
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Armor", "body_armor", newHorseArmor);
                        }

                        break;
                }
            }
        }

        for (int i = 0; i < nodes1.Length; i += 1)
        {
            var node1 = nodes1[i];
            if (node1.Name == "Item")
            {
                int heirloomLevel = IdToHeirloomLevel(node1.Attributes!["id"].Value);
                string baseId = node1.Attributes!["id"].Value.Remove(node1.Attributes!["id"].Value.Length - 2);
                // Remove the price attribute so it is recomputed using our model.
                var valueAttr = node1.Attributes["value"];
                if (valueAttr != null)
                {
                    node1.Attributes.Remove(valueAttr);
                }

                var nonHeirloomNode = baseItem[baseId];
                var type = (ItemObject.ItemTypeEnum)Enum.Parse(typeof(ItemObject.ItemTypeEnum), node1.Attributes!["Type"].Value);
                if (type is ItemObject.ItemTypeEnum.HeadArmor
                         or ItemObject.ItemTypeEnum.Cape
                         or ItemObject.ItemTypeEnum.BodyArmor
                         or ItemObject.ItemTypeEnum.HandArmor
                         or ItemObject.ItemTypeEnum.LegArmor
                         or ItemObject.ItemTypeEnum.HorseHarness)
                {
                    ModifyNodeAttribute(node1, "weight",
                        _ => ModifyArmorWeight(nonHeirloomNode, node1, type).ToString(CultureInfo.InvariantCulture));
                }

                if (type is ItemObject.ItemTypeEnum.Shield)
                {
                    ModifyNodeAttribute(node1, "weight",
                        _ => ModifyShieldWeight(nonHeirloomNode, node1, type).ToString(CultureInfo.InvariantCulture));
                }
                if (type is ItemObject.ItemTypeEnum.Bow)
                {
                    ModifyNodeAttribute(node1, "weight",
                        _ => ModifyBowWeight(nonHeirloomNode, node1, type, heirloomLevel).ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        return itemsDoc;
    }

    private static float ModifyArmorWeight(XmlNode nonHeirloomNode, XmlNode node, ItemObject.ItemTypeEnum type)
    {
        XmlNode armorNode = nonHeirloomNode.SelectNodes("ItemComponent/Armor")!.Cast<XmlNode>().First();
        float armorPower =
            1.0f * (armorNode.Attributes["head_armor"] == null ? 0f : float.Parse(armorNode.Attributes["head_armor"].Value))
          + 1.15f * (armorNode.Attributes["body_armor"] == null ? 0f : float.Parse(armorNode.Attributes["body_armor"].Value))
          + 1.0f * (armorNode.Attributes["arm_armor"] == null ? 0f : float.Parse(armorNode.Attributes["arm_armor"].Value))
          + 0.8f * (armorNode.Attributes["leg_armor"] == null ? 0f : float.Parse(armorNode.Attributes["leg_armor"].Value));
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
        if (type is ItemObject.ItemTypeEnum.HorseHarness)
        {
            return armorNode.Attributes["body_armor"] == null ? 0f : float.Parse(armorNode.Attributes["body_armor"].Value);
        }

        return 12.2f * (float)Math.Pow(armorPower, 1.4f) / bestArmorPower;
    }

    private static float ModifyShieldWeight(XmlNode nonHeirloomNode, XmlNode node, ItemObject.ItemTypeEnum type)
    {
        XmlNode shieldNode = nonHeirloomNode.SelectNodes("ItemComponent/Weapon")!.Cast<XmlNode>().First();
        float shieldWeightPoints =
            float.Parse(shieldNode.Attributes!["hit_points"].Value)
          * float.Parse(shieldNode.Attributes!["weapon_length"].Value)
          * float.Parse(shieldNode.Attributes!["weapon_length"].Value);
        return shieldWeightPoints / 800000f;
    }

    private static float ModifyBowWeight(XmlNode nonHeirloomNode, XmlNode node, ItemObject.ItemTypeEnum type, int heirloomLevel)
    {
        XmlNode weaponNode = nonHeirloomNode.SelectNodes("ItemComponent/Weapon")!.Cast<XmlNode>().First();
        float tier = CrpgItemValueModel.ComputeBowTier(int.Parse(weaponNode.Attributes!["thrust_damage"].Value),
            int.Parse(weaponNode.Attributes!["speed_rating"].Value),
            int.Parse(weaponNode.Attributes!["missile_speed"].Value),
            int.Parse(weaponNode.Attributes!["thrust_speed"].Value),
            int.Parse(weaponNode.Attributes!["accuracy"].Value),
            weaponNode.Attributes!["item_usage"].Value == "long_bow",
            heirloomLevel);
        return tier * int.Parse(weaponNode.Attributes!["thrust_damage"].Value) / 100f;
    }

    private static void ModifyChildHeirloomNodesAttribute(
        XmlNode nonHeirloomNode,
        XmlNode parentNode,
        string childXPath,
        string attributeName,
        int bonus,
        Func<XmlNode, bool>? filter = null,
        string? defaultValue = null,
        float bonusPercentage = 0,
        float ihatemountsPercentage = 0)
    {
        foreach (var heirloomChildNode in parentNode.SelectNodes(childXPath)!.Cast<XmlNode>())
        {
            if (filter != null && !filter(heirloomChildNode))
            {
                continue;
            }

            foreach (var nonHeirloomChildNode in nonHeirloomNode.SelectNodes(childXPath)!.Cast<XmlNode>())
            {
                if (filter != null && !filter(nonHeirloomChildNode))
                {
                    continue;
                }

                Debug.Print("going to modify the child node");
                ModifyHeirloomNodeAttribute(nonHeirloomChildNode, heirloomChildNode, attributeName, bonus, bonusPercentage, ihatemountsPercentage, defaultValue);
            }
        }
    }

    private static void ModifyHeirloomNodeAttribute(
        XmlNode nonHeirloomNode,
        XmlNode heirloomNode,
        string attributeName,
        int bonus,
        float bonusPercentage,
        float ihatemountsPercentage,
        string? defaultValue = null)
    {
        var heirloomAttr = heirloomNode.Attributes![attributeName];
        Debug.Print("heirloomAttr set");
        var nonHeirloomAttr = nonHeirloomNode.Attributes![attributeName];
        Debug.Print("nonheirloomAttr set");
        if (heirloomAttr == null)
        {
            Debug.Print("heirloomAttr is null");
            if (defaultValue == null)
            {
                throw new KeyNotFoundException($"heirloomAttribute '{attributeName}' was not found and no default was provided");
            }

            heirloomAttr = heirloomNode.OwnerDocument!.CreateAttribute(attributeName);
            heirloomAttr.Value = defaultValue;
            heirloomNode.Attributes.Append(heirloomAttr);
        }

        Debug.Print($"{nonHeirloomAttr == null}");
        string nonHeirloomAttrValue = nonHeirloomAttr == null ? "0" : nonHeirloomAttr.Value;
        Debug.Print($"old {attributeName} value: {nonHeirloomAttrValue} new {attributeName} value: {int.Parse(nonHeirloomAttrValue) + bonus + (int)Math.Ceiling(int.Parse(nonHeirloomAttrValue) * bonusPercentage)}");
        heirloomAttr.Value = (int.Parse(nonHeirloomAttrValue) + bonus + (int)Math.Ceiling(int.Parse(nonHeirloomAttrValue) * bonusPercentage / 100f) + (int)Math.Ceiling((200 + int.Parse(nonHeirloomAttrValue)) * ihatemountsPercentage / 100f)).ToString();
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

    private static void ModifyChildNodesAttribute(XmlNode parentNode,
        string childXPath,
        string attributeName,
        Func<string, string> modify,
        Func<XmlNode, bool>? filter = null,
        string? defaultValue = null)
    {
        foreach (var childNode in parentNode.SelectNodes(childXPath)!.Cast<XmlNode>())
        {
            if (filter != null && !filter(childNode))
            {
                continue;
            }

            ModifyNodeAttribute(childNode, attributeName, modify, defaultValue);
        }
    }

    private static CrpgItemType MbToCrpgItemType(ItemObject.ItemTypeEnum t) => t switch
    {
        ItemObject.ItemTypeEnum.Invalid => CrpgItemType.Undefined, // To be consistent with WeaponClass.
        ItemObject.ItemTypeEnum.Horse => CrpgItemType.Mount, // Horse includes camel and mule.
        ItemObject.ItemTypeEnum.HorseHarness => CrpgItemType.MountHarness, // Horse includes camel and mule.
        ItemObject.ItemTypeEnum.Cape => CrpgItemType.ShoulderArmor, // Cape is a bad name.
        _ => (CrpgItemType)Enum.Parse(typeof(CrpgItemType), t.ToString()),
    };
    private static int IdToHeirloomLevel(string id)
    {
        return int.Parse(id.Split('_').Last().Substring(1));
    }

    private static string ToggleRefund(string id)
    {
        var idParts = id.Split('_').ToList();
        string lastPart = idParts.Last();

        // If REFUND already exists, we remove it.
        if (idParts.Contains("REFUND"))
        {
            idParts = idParts.Where(part => part != "REFUND").ToList();
        }

        // Check if idParts contains any 'v' followed by a number
        int versionPartIndex = idParts.FindIndex(part => Regex.IsMatch(part, @"^v\d+$"));

        if (versionPartIndex != -1)
        {
            // Extract the version number, increment it and replace the original version string
            int versionNumber = int.Parse(idParts[versionPartIndex].Substring(1));  // remove 'v' and parse the number
            idParts[versionPartIndex] = "v" + (versionNumber + 1);  // replace the old version string with the new one
        }
        else // If neither REFUND nor vN is present, we append v1
        {
            idParts = idParts.Take(idParts.Count - 1).Append("v1").Append(lastPart).ToList();
        }

        // Join back into a string to get the transformed
        return string.Join("_", idParts);
    }

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
