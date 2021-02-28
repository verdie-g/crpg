using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Crpg.Common.Helpers;
using Crpg.GameMod.Api.Models;
using Crpg.GameMod.Api.Models.Items;
using Crpg.GameMod.Common;
using Crpg.GameMod.Helpers.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using Path = System.IO.Path;

namespace Crpg.GameMod.DataExport
{
    internal class ItemExporter : IDataExporter
    {
        private static readonly string[] ItemFiles =
        {
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

        private static readonly HashSet<ItemObject.ItemTypeEnum> BlacklistedItemTypes = new HashSet<ItemObject.ItemTypeEnum>
        {
            ItemObject.ItemTypeEnum.Invalid,
            ItemObject.ItemTypeEnum.Goods,
            ItemObject.ItemTypeEnum.Animal,
            ItemObject.ItemTypeEnum.Book,
        };

        private static readonly HashSet<string> BlacklistedItems = new HashSet<string>
        {
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
            "camel_tournament", // Name conflict with camel.
            "desert_round_shield", // Name conflict with bound_desert_round_shield rank 2.
            "eastern_leather_boots", // Name conflict with leather_boots.
            "empire_horse_tournament", // Name conflict with empire_horse.
            "empire_sword_1_t2", // Name conflict with iron_spatha_sword_t2.
            "empire_sword_1_t2_blunt", // Name conflict with iron_spatha_sword_t2.
            "grapeshot_fire_projectile", // Can't be equipped.
            "grapeshot_fire_stack", // Can't be equipped.
            "grapeshot_projectile", // Can't be equipped.
            "grapeshot_stack", // Can't be equipped.
            "heavy_horsemans_kite_shield", // Name conflict with bound_horsemans_kite_shield rank 2.
            "khuzait_horse_tournament", // Name conflict with khuzait_horse.
            "lordly_padded_mitten", // Name conflict with padded_mitten rank 3.
            "mule_unmountable", // Can't be equipped.
            "pack_camel_unmountable", // Can't be equipped.
            "pot", // Can't be equipped.
            "reinforced_kite_shield", // Name conflict with western_kite_shield rank 2.
            "reinforced_mail_mitten", // Name conflict with mail_mitten rank 2.
            "reinforced_padded_mitten", // Name conflict with padded_mitten rank 2.
            "southern_lamellar_armor", // Name conflict with desert_lamellar.
            "strapped_round_shield", // Name conflict with leather_round_shield rank 2.
            "stronger_eastern_wicker_shield", // Name conflict with eastern_wicker_shield rank 2.
            "stronger_footmans_wicker_shield", // Name conflict with footmans_wicker_shield rank 2.
            "sturgia_horse_tournament", // Name conflict with sturgia_horse.
            "sturgia_old_shield_c", // Name conflict with leather_round_shield rank 2.
            "sturgia_old_shield_c", // Name conflict with strapped_round_shield.
            "sturgian_helmet_b_close", // Name conflict with closed_goggled_helmet.
            "sturgian_helmet_b_open", // Name conflict with sturgian_helmet_open.
            "torch", // Invisible.
            "vlandia_horse_tournament", // Name conflict with vlandia_horse.
            "vlandia_mace_3_t5", // Name conflict with pernach_mace_t3.
            "womens_headwrap_c", // Name conflict with head_wrapped.
            "wooden_sword_t2", // Name conflict with wooden_sword_t1.
            "woodland_throwing_axe_1_t1", // Name conflict with highland_throwing_axe_1_t2.
        };

        private static readonly HashSet<CrpgWeaponClass> WeaponClassesAffectedByPowerStrike = new HashSet<CrpgWeaponClass>
        {
             CrpgWeaponClass.Dagger,
             CrpgWeaponClass.OneHandedSword,
             CrpgWeaponClass.TwoHandedSword,
             CrpgWeaponClass.OneHandedAxe,
             CrpgWeaponClass.TwoHandedAxe,
             CrpgWeaponClass.Mace,
             CrpgWeaponClass.Pick,
             CrpgWeaponClass.TwoHandedMace,
             CrpgWeaponClass.OneHandedPolearm,
             CrpgWeaponClass.TwoHandedPolearm,
             CrpgWeaponClass.LowGripPolearm,
        };

        private static readonly HashSet<CrpgWeaponClass> WeaponClassesAffectedByPowerDraw = new HashSet<CrpgWeaponClass>
        {
             CrpgWeaponClass.Bow,
        };

        private static readonly HashSet<CrpgWeaponClass> WeaponClassesAffectedByPowerThrow = new HashSet<CrpgWeaponClass>
        {
             CrpgWeaponClass.Stone,
             CrpgWeaponClass.Boulder,
             CrpgWeaponClass.ThrowingAxe,
             CrpgWeaponClass.ThrowingKnife,
             CrpgWeaponClass.Javelin,
        };

        private static readonly HashSet<CrpgWeaponClass> WeaponClassesAffectedByShield = new HashSet<CrpgWeaponClass>
        {
             CrpgWeaponClass.SmallShield,
             CrpgWeaponClass.LargeShield,
        };

        public Task Export(string outputPath)
        {
            var crpgConstants = LoadCrpgConstants();

            var mbItems = DeserializeMbItems(ItemFiles)
                .DistinctBy(i => i.StringId)
                .Where(FilterItem) // Remove test and blacklisted items
                .OrderBy(i => i.StringId)
                .ToArray();
            var crpgItems = mbItems.Select(mbItem =>
            {
                var crpgItem = MbToCrpgItem(mbItem);
                return RescaleItemStats(crpgItem, crpgConstants);
            });

            Directory.CreateDirectory(outputPath);
            SerializeCrpgItems(crpgItems, outputPath);
            return GenerateItemsThumbnail(mbItems, Path.Combine(outputPath, "Items"));
        }

        private static bool FilterItem(ItemObject mbItem) => !mbItem.StringId.Contains("test")
                                                             && !mbItem.StringId.Contains("dummy")
                                                             && !mbItem.Name.Contains("_")
                                                             && !BlacklistedItemTypes.Contains(mbItem.ItemType)
                                                             && !BlacklistedItems.Contains(mbItem.StringId);

        private static CrpgConstants LoadCrpgConstants()
        {
            string path = BasePath.Name + "Modules/cRPG/ModuleData/constants.json";
            return JsonConvert.DeserializeObject<CrpgConstants>(File.ReadAllText(path));
        }

        private static CrpgItemCreation MbToCrpgItem(ItemObject mbItem)
        {
            var crpgItem = new CrpgItemCreation
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

        private static CrpgWeaponClass MbToCrpgWeaponClass(WeaponClass wc) =>
            (CrpgWeaponClass)Enum.Parse(typeof(CrpgWeaponClass), wc.ToString());

        private static CrpgWeaponFlags MbToCrpgWeaponFlags(WeaponFlags wf) => (CrpgWeaponFlags)wf;

        private static CrpgDamageType MbToCrpgDamageType(DamageTypes t) => t switch
        {
            DamageTypes.Invalid => CrpgDamageType.Undefined, // To be consistent with WeaponClass.
            _ => (CrpgDamageType)Enum.Parse(typeof(CrpgDamageType), t.ToString()),
        };

        /// <summary>
        /// Since damages can be increased with Power Strike/Draw/Throw or speed can be increased with Shield or health
        /// can be increased with iron flesh so stats of the native bannerlord items need to be rescaled accordingly.
        /// </summary>
        private static CrpgItemCreation RescaleItemStats(CrpgItemCreation item, CrpgConstants crpgConstants)
        {
            // Assume the average attributes of a lvl 30 character and decrease stats by the amount the skills would give.
            const int averageCharacterStrength = 18;
            const int averageCharacterAgility = 18;

            float averageHealth = crpgConstants.DefaultHealthPoints
                                  + MathHelper.ApplyPolynomialFunction(averageCharacterStrength, crpgConstants.HealthPointsForStrengthCoefs)
                                  + MathHelper.ApplyPolynomialFunction(averageCharacterStrength / 3, crpgConstants.HealthPointsForIronFleshCoefs);
            // Assume the average health of a native npc is 100.
            float healthFactor = 100 / averageHealth;

            float psFactor = MathHelper.ApplyPolynomialFunction(averageCharacterStrength / 3, crpgConstants.DamageFactorForPowerStrikeCoefs);
            float pdFactor = MathHelper.ApplyPolynomialFunction(averageCharacterStrength / 3, crpgConstants.DamageFactorForPowerDrawCoefs);
            float ptFactor = MathHelper.ApplyPolynomialFunction(averageCharacterStrength / 3, crpgConstants.DamageFactorForPowerThrowCoefs);

            float shieldSpeedIncrease = MathHelper.ApplyPolynomialFunction(averageCharacterAgility / 6, crpgConstants.SpeedFactorForShieldCoefs);
            float shieldDurabilityIncrease = MathHelper.ApplyPolynomialFunction(averageCharacterAgility / 6, crpgConstants.DurabilityFactorForShieldCoefs);

            foreach (var weapon in item.Weapons)
            {
                if (WeaponClassesAffectedByPowerStrike.Contains(weapon.Class))
                {
                    weapon.SwingDamage = (int)(weapon.SwingDamage / psFactor / healthFactor);
                    weapon.ThrustDamage = (int)(weapon.ThrustDamage / psFactor / healthFactor);
                }
                else if (WeaponClassesAffectedByPowerDraw.Contains(weapon.Class))
                {
                    weapon.ThrustDamage = (int)(weapon.ThrustDamage / pdFactor / healthFactor);
                }
                else if (WeaponClassesAffectedByPowerThrow.Contains(weapon.Class))
                {
                    weapon.ThrustDamage = (int)(weapon.ThrustDamage / ptFactor / healthFactor);
                }
                else if (WeaponClassesAffectedByShield.Contains(weapon.Class))
                {
                    weapon.SwingSpeed = (int)(weapon.SwingSpeed / shieldSpeedIncrease);
                    weapon.StackAmount = (int)(weapon.StackAmount / shieldDurabilityIncrease);
                }
            }

            return item;
        }

        private static IEnumerable<ItemObject> DeserializeMbItems(IEnumerable<string> paths)
        {
            var game = Game.CreateGame(new MultiplayerGame(), new MultiplayerGameManager());
            game.Initialize();

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
                        var item = new ItemObject();
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
                Converters = new JsonConverter[] { new ArrayStringEnumFlagsConverter(), new StringEnumConverter() }
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
    }
}
