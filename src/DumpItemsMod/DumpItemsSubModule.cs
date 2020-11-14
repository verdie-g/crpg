using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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

        private static readonly string[] ItemFiles =
        {
            "../../Modules/Native/ModuleData/mpitems.xml",
            "../../Modules/SandBoxCore/ModuleData/spitems/arm_armors.xml",
            "../../Modules/SandBoxCore/ModuleData/spitems/body_armors.xml",
            "../../Modules/SandBoxCore/ModuleData/spitems/head_armors.xml",
            "../../Modules/SandBoxCore/ModuleData/spitems/horses_and_others.xml",
            "../../Modules/SandBoxCore/ModuleData/spitems/leg_armors.xml",
            "../../Modules/SandBoxCore/ModuleData/spitems/shields.xml",
            "../../Modules/SandBoxCore/ModuleData/spitems/shoulder_armors.xml",
            "../../Modules/SandBoxCore/ModuleData/spitems/tournament_weapons.xml",
            "../../Modules/SandBoxCore/ModuleData/spitems/weapons.xml",
        };

        private static HashSet<ItemObject.ItemTypeEnum> BlacklistedItemTypes = new HashSet<ItemObject.ItemTypeEnum>
        {
            ItemObject.ItemTypeEnum.Invalid,
            ItemObject.ItemTypeEnum.Goods,
            ItemObject.ItemTypeEnum.Pistol,
            ItemObject.ItemTypeEnum.Musket,
            ItemObject.ItemTypeEnum.Bullets,
            ItemObject.ItemTypeEnum.Animal,
            ItemObject.ItemTypeEnum.Book,
            ItemObject.ItemTypeEnum.Banner,
        };

        private static readonly HashSet<string> BlacklistedItems = new HashSet<string>
        {
            "mp_medium_skirt",
            "ballista_projectile",
            "ballista_projectile_burning",
        };

        protected override void OnSubModuleLoad()
        {
            Directory.CreateDirectory(OutputPath);

            Module.CurrentModule.AddInitialStateOption(new InitialStateOption("Dump Items", new TextObject("Dump Items"), 9990, () =>
            {
                InformationManager.DisplayMessage(new InformationMessage($"Exporting items to {Path.GetFullPath(OutputPath)}."));
                DumpItems().ContinueWith(_ => InformationManager.DisplayMessage(new InformationMessage("Done.")));
            }, false));
        }

        private static Task DumpItems()
        {
            var mbItems = DeserializeMbItems(ItemFiles)
                .DistinctBy(i => i.StringId)
                .Where(FilterItem) // Remove test and blacklisted items
                .OrderBy(i => i.StringId)
                .ToArray();
            var crpgItems = mbItems
                .Select(MbToCrpgItem)
                .OrderBy(i => i.Type)
                .ThenBy(i => i.Value);

            SerializeCrpgItems(crpgItems, OutputPath);
            return GenerateItemsThumbnail(mbItems, OutputPath);
        }

        private static bool FilterItem(ItemObject mbItem) => !mbItem.StringId.Contains("test")
                                                             && !mbItem.StringId.Contains("dummy")
                                                             && !mbItem.Name.Contains("_")
                                                             && !BlacklistedItemTypes.Contains(mbItem.ItemType)
                                                             && !BlacklistedItems.Contains(mbItem.StringId);

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

        private static IEnumerable<ItemObject> DeserializeMbItems(IEnumerable<string> paths)
        {
            var game = Game.CreateGame(new MultiplayerGame(), new MultiplayerGameManager());
            game.Initialize();
            SetItemValueModel(game.BasicModels, new CrpgItemValueModel());

            var items = Enumerable.Empty<ItemObject>();
            foreach (string path in paths)
            {
                var itemsDoc = new XmlDocument();
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
                        itemNode.Attributes.Remove(itemNode.Attributes["value"]); // force recomputation of value
                        var item = new ItemObject();
                        item.Deserialize(game.ObjectManager, itemNode);
                        return item;
                    });
                items = items.Concat(fileItems);
            }

            return items;
        }

        private static void SerializeCrpgItems(IEnumerable<Item> items, string outputPath)
        {
            var serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Newtonsoft.Json.Formatting.Indented,
                ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            });

            using var s = new StreamWriter(Path.Combine(outputPath, "items.json"));
            serializer.Serialize(s, items);
        }

        private static Task GenerateItemsThumbnail(IEnumerable<ItemObject> mbItems, string outputPath)
        {
            var createTextureTasks = new List<Task>();
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

                var createTextureTaskSource = new TaskCompletionSource<object?>();
                createTextureTasks.Add(createTextureTaskSource.Task);

                // Texture.SaveToFile doesn't accept absolute paths
                TableauCacheManager.Current.BeginCreateItemTexture(mbItem, texture =>
                {
                    texture.SaveToFile(Path.Combine(outputPath, mbItem.StringId + ".png"));
                    createTextureTaskSource.SetResult(null);
                });
            }

            return Task.WhenAll(createTextureTasks);
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
