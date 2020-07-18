using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using Module = TaleWorlds.MountAndBlade.Module;
using Path = System.IO.Path;

namespace Crpg.DumpItemsMod
{
    public class DumpItemsSubModule : MBSubModuleBase
    {
        private const string OutputPath = "../../Items";

        protected override void OnSubModuleLoad()
        {
            Directory.CreateDirectory(OutputPath);

            Module.CurrentModule.AddInitialStateOption(new InitialStateOption("Dump Items", new TextObject("Dump Items"), 9990, () =>
            {
                DumpItems();
                InformationManager.DisplayMessage(new InformationMessage("Exporting items to " + Path.GetFullPath(OutputPath)));
            }, false));
        }

        private static void DumpItems()
        {
            var mbItems = DeserializeMbItems("../../Modules/Native/ModuleData/mpitems.xml")
                .OrderBy(i => i.Value)
                .DistinctBy(i => i.StringId)
                .ToArray();
            var crpgItems = mbItems
                .Select(MbToCrpgItem)
                .OrderBy(i => i.Type)
                .ThenBy(i => i.Value);

            SerializeCrpgItems(crpgItems, OutputPath);
            GenerateItemsThumbnail(mbItems, OutputPath);
        }

        private static Item MbToCrpgItem(ItemObject mbItem)
        {
            var crpgItem = new Item
            {
                MbId = mbItem.StringId,
                Name = mbItem.Name.ToString(),
                Type = MbToCrpgItemType(mbItem.Type),
                Value = mbItem.Value,
                Weight = mbItem.Weight,
            };

            if (mbItem.ArmorComponent != null)
            {
                crpgItem.Armor = new ItemArmorComponent
                {
                    HeadArmor = mbItem.ArmorComponent.HeadArmor,
                    BodyArmor = mbItem.ArmorComponent.BodyArmor,
                    ArmArmor = mbItem.ArmorComponent.ArmArmor,
                    LegArmor = mbItem.ArmorComponent.LegArmor,
                };
            }

            if (mbItem.HorseComponent != null)
            {
                crpgItem.Horse = new ItemHorseComponent
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
                crpgItem.Weapons = mbItem.WeaponComponent.Weapons.Select(w => new ItemWeaponComponent
                {
                    Accuracy = w.Accuracy,
                    MissileSpeed = w.MissileSpeed,
                    StackAmount = w.MaxDataValue,
                    Length = w.WeaponLength,
                    Handling = w.Handling,
                    BodyArmor = w.BodyArmor,
                    Flags = (ulong)w.WeaponFlags,
                    ThrustDamage = w.ThrustDamage,
                    ThrustDamageType = (int)w.ThrustDamageType,
                    ThrustSpeed = w.ThrustSpeed,
                    SwingDamage = w.SwingDamage,
                    SwingDamageType = (int)w.SwingDamageType,
                    SwingSpeed = w.SwingSpeed,
                }).ToArray();
            }

            return crpgItem;
        }

        private static int MbToCrpgItemType(ItemObject.ItemTypeEnum t) => t switch
        {
            ItemObject.ItemTypeEnum.HeadArmor => 0,
            ItemObject.ItemTypeEnum.Cape => 1,
            ItemObject.ItemTypeEnum.BodyArmor => 2,
            ItemObject.ItemTypeEnum.HandArmor => 3,
            ItemObject.ItemTypeEnum.LegArmor => 4,
            ItemObject.ItemTypeEnum.HorseHarness => 5,
            ItemObject.ItemTypeEnum.Horse => 6,
            ItemObject.ItemTypeEnum.Shield => 7,
            ItemObject.ItemTypeEnum.Bow => 8,
            ItemObject.ItemTypeEnum.Crossbow => 9,
            ItemObject.ItemTypeEnum.OneHandedWeapon => 10,
            ItemObject.ItemTypeEnum.TwoHandedWeapon => 11,
            ItemObject.ItemTypeEnum.Polearm => 12,
            ItemObject.ItemTypeEnum.Thrown => 13,
            ItemObject.ItemTypeEnum.Arrows => 14,
            ItemObject.ItemTypeEnum.Bolts => 15,
            _ => throw new ArgumentOutOfRangeException(nameof(t)),
        };

        private static IEnumerable<ItemObject> DeserializeMbItems(string path)
        {
            var game = Game.CreateGame(new MultiplayerGame(), new MultiplayerGameManager());
            game.Initialize();
            SetItemValueModel(game.BasicModels, new DefaultItemValueModel());

            var itemsDoc = new XmlDocument();
            using (var r = XmlReader.Create(path, new XmlReaderSettings { IgnoreComments = true }))
            {
                itemsDoc.Load(r);
            }

            return itemsDoc
                .LastChild
                .ChildNodes
                .Cast<XmlNode>()
                .Select(itemNode =>
                {
                    itemNode.Attributes.Remove(itemNode.Attributes["value"]); // force recomputation of value
                    var item = new ItemObject();
                    item.Deserialize(game.ObjectManager, itemNode);
                    return item;
                });
        }

        private static void SerializeCrpgItems(IEnumerable<Item> items, string outputPath)
        {
            var serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Newtonsoft.Json.Formatting.Indented,
                ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            });

            using var s = new StreamWriter(Path.Combine(outputPath, "mpitems.json"));
            serializer.Serialize(s, items);
        }

        private static void GenerateItemsThumbnail(IEnumerable<ItemObject> mbItems, string outputPath)
        {
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
                    _ => mbItem.Type
                };

                // Texture.SaveToFile doesn't accept absolute paths
                TableauCacheManager.Current.BeginCreateItemTexture(mbItem, texture =>
                    texture.SaveToFile(Path.Combine(outputPath, mbItem.StringId + ".png")));
            }
        }

        private static void SetItemValueModel(BasicGameModels gameModels, ItemValueModel gm)
        {
            gameModels
                .GetType()
                .GetProperty(nameof(gameModels.ItemValueModel), BindingFlags.Instance | BindingFlags.Public)
                .SetValue(gameModels, gm);
        }
    }
}
