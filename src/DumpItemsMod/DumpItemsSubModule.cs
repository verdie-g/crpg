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
                .DistinctBy(i => i.StringId)
                .Where(i => !i.StringId.Contains("test") && !i.StringId.Contains("dummy") && !i.Name.Contains("_")) // Remove test items
                .OrderBy(i => i.StringId)
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
                Type = mbItem.Type.ToString(),
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
                    Class = w.WeaponClass.ToString(),
                    Accuracy = w.Accuracy,
                    MissileSpeed = w.MissileSpeed,
                    StackAmount = w.MaxDataValue,
                    Length = w.WeaponLength,
                    Balance = w.WeaponBalance,
                    Handling = w.Handling,
                    BodyArmor = w.BodyArmor,
                    Flags = (long)w.WeaponFlags,
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

        private static string MbToCrpgDamageType(DamageTypes t) => t switch
        {
            DamageTypes.Invalid => "Undefined",
            _ => t.ToString(),
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
