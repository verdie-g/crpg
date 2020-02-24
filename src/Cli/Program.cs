using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities;
using WeaponFlags = Crpg.Domain.Entities.WeaponFlags;

namespace Crpg.Cli
{
    class Program
    {
        static async Task Main()
        {
            Directory.SetCurrentDirectory("XXXXX/MountAndBlade/bin/Win64_Shipping_Client");
            var objectItems = DeserializeItemObjects("../../Modules/Native/ModuleData/mpitems.xml");
            var items = objectItems
                        .Select(i =>
                        {
                            var item = new ItemCreation
                            {
                                MbId = i.StringId,
                                Name = i.Name.ToString(),
                                Type = MBToCrpgItemType(i.Type),
                                Value = i.Value,
                                Weight = i.Weight,
                            };

                            if (i.ArmorComponent != null)
                            {
                                item.HeadArmor = i.ArmorComponent.HeadArmor;
                                item.BodyArmor = i.ArmorComponent.BodyArmor;
                                item.ArmArmor = i.ArmorComponent.ArmArmor;
                                item.LegArmor = i.ArmorComponent.LegArmor;
                            }

                            if (i.HorseComponent != null)
                            {
                                item.BodyLength = i.HorseComponent.BodyLength;
                                item.ChargeDamage = i.HorseComponent.ChargeDamage;
                                item.Maneuver = i.HorseComponent.Maneuver;
                                item.Speed = i.HorseComponent.Speed;
                                item.HitPoints = i.HorseComponent.HitPoints;
                            }

                            if (i.WeaponComponent != null)
                            {
                                item.ThrustDamageType = MBToCrpgDamageType(i.Weapons[0].ThrustDamageType);
                                item.SwingDamageType = MBToCrpgDamageType(i.Weapons[0].SwingDamageType);
                                item.Accuracy = i.Weapons[0].Accuracy;
                                item.MissileSpeed = i.Weapons[0].MissileSpeed;
                                item.StackAmount = i.Weapons[0].MaxDataValue;
                                item.WeaponLength = i.Weapons[0].WeaponLength;

                                if (item.ThrustDamageType != null)
                                {
                                    item.PrimaryThrustDamage = i.Weapons[0].ThrustDamage;
                                    item.PrimaryThrustSpeed = i.Weapons[0].ThrustSpeed;
                                }

                                if (item.SwingDamageType != null)
                                {
                                    item.PrimarySwingDamage = i.Weapons[0].SwingDamage;
                                    item.PrimarySwingSpeed = i.Weapons[0].SwingSpeed;
                                }

                                item.PrimaryWeaponFlags = (WeaponFlags?) i.Weapons[0].WeaponFlags;

                                if (i.Weapons.Count > 1)
                                {
                                    if (item.ThrustDamageType != null)
                                    {
                                        item.SecondaryThrustDamage = i.Weapons[1].ThrustDamage;
                                        item.SecondaryThrustSpeed = i.Weapons[1].ThrustSpeed;
                                    }

                                    if (item.SwingDamageType != null)
                                    {
                                        item.SecondarySwingDamage = i.Weapons[1].SwingDamage;
                                        item.SecondarySwingSpeed = i.Weapons[1].SwingSpeed;
                                    }

                                    item.SecondaryWeaponFlags = (WeaponFlags?) i.Weapons[1].WeaponFlags;
                                }
                            }

                            return item;
                        })
                        .ToArray();

            // insert in db
        }

        private static ItemType MBToCrpgItemType(ItemObject.ItemTypeEnum t)
        {
            switch (t)
            {
                case ItemObject.ItemTypeEnum.Horse:
                    return ItemType.Horse;
                case ItemObject.ItemTypeEnum.OneHandedWeapon:
                    return ItemType.OneHandedWeapon;
                case ItemObject.ItemTypeEnum.TwoHandedWeapon:
                    return ItemType.TwoHandedWeapon;
                case ItemObject.ItemTypeEnum.Polearm:
                    return ItemType.Polearm;
                case ItemObject.ItemTypeEnum.Arrows:
                    return ItemType.Arrows;
                case ItemObject.ItemTypeEnum.Bolts:
                    return ItemType.Bolts;
                case ItemObject.ItemTypeEnum.Shield:
                    return ItemType.Shield;
                case ItemObject.ItemTypeEnum.Bow:
                    return ItemType.Bow;
                case ItemObject.ItemTypeEnum.Crossbow:
                    return ItemType.Crossbow;
                case ItemObject.ItemTypeEnum.Thrown:
                    return ItemType.Thrown;
                case ItemObject.ItemTypeEnum.HeadArmor:
                    return ItemType.HeadArmor;
                case ItemObject.ItemTypeEnum.BodyArmor:
                    return ItemType.BodyArmor;
                case ItemObject.ItemTypeEnum.LegArmor:
                    return ItemType.LegArmor;
                case ItemObject.ItemTypeEnum.HandArmor:
                    return ItemType.HandArmor;
                case ItemObject.ItemTypeEnum.Cape:
                    return ItemType.Cape;
                case ItemObject.ItemTypeEnum.HorseHarness:
                    return ItemType.HorseHarness;
                default:
                    throw new ArgumentOutOfRangeException(nameof(t), t, null);
            }
        }

        private static DamageType? MBToCrpgDamageType(DamageTypes t)
        {
            return t == DamageTypes.Invalid ? null : (DamageType?) t;
        }

        private static IEnumerable<ItemObject> DeserializeItemObjects(string path)
        {
            var objManager = CreateObjectManager();

            var itemsDoc = new XmlDocument();
            using (var r = XmlReader.Create(path, new XmlReaderSettings { IgnoreComments = true }))
                itemsDoc.Load(r);

            return itemsDoc
                   .LastChild
                   .ChildNodes
                   .Cast<XmlNode>()
                   .Select(itemNode =>
                   {
                       itemNode.Attributes.Remove(itemNode.Attributes["value"]); // force recomputation of value
                       var item = new ItemObject();
                       item.Deserialize(objManager, itemNode);
                       return item;
                   });
        }

        private static MBObjectManager CreateObjectManager()
        {
            var game = new Game(new NopGame(), new CustomGameManager());
            game.Initialize();
            game.FirstInitialize();
            game.SecondInitialize(new GameModel[] { new DefaultItemValueModel(), new DefaultStrikeMagnitudeModel() });
            game.RegisterBasicTypes();
            game.ObjectManager.RegisterType<BasicCultureObject>("Culture", "BasicCultures");

            var gameTextManager = new GameTextManager();
            gameTextManager.LoadGameTexts("../../Modules/Native/ModuleData/module_strings.xml");
            GameTexts.Initialize(gameTextManager);

            XmlResource.InitializeXmlInformationList(new List<MbObjectXmlInformation>
            {
                new MbObjectXmlInformation
                {
                    Id = "CraftingTemplates",
                    Name = "crafting_templates",
                    ModuleName = "Native",
                    GameTypesIncluded = new List<string>(),
                },
                new MbObjectXmlInformation
                {
                    Id = "CraftingPieces",
                    Name = "mp_crafting_pieces",
                    ModuleName = "Native",
                    GameTypesIncluded = new List<string>(),
                },
                new MbObjectXmlInformation
                {
                    Id = "SPCultures",
                    Name = "mpcultures",
                    ModuleName = "Native",
                    GameTypesIncluded = new List<string>(),
                },
            });

            game.ObjectManager.LoadXML("SPCultures");
            game.LoadBasicFiles(true);
            return game.ObjectManager;
        }

        private class NopGame : GameType
        {
            protected override void DoLoadingForGameType(GameTypeLoadingStates gameTypeLoadingState,
                out GameTypeLoadingStates nextState)
                => nextState = GameTypeLoadingStates.None;

            public override void OnDestroy()
            {
            }
        }
    }
}