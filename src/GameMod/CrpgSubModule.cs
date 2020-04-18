using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using Newtonsoft.Json;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using Module = TaleWorlds.MountAndBlade.Module;
using Path = System.IO.Path;
using Steamworks;

namespace Crpg.GameMod
{
    public class CrpgSubModule : MBSubModuleBase
    {
        [DllImport("Rgl.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "?toggle_imgui_console_visibility@rglCommand_line_manager@@QEAAXXZ")]
        public static extern void Toggle_imgui_console_visibility(UIntPtr x);


        private const string OutputPath = "../../Items";

        public override void BeginGameStart(Game game)
        {
            InformationManager.DisplayMessage(new InformationMessage("BeginGameStart"));
            base.BeginGameStart(game);
        }
        public override void OnMultiplayerGameStart(Game game, object starterObject)
        {
            InformationManager.DisplayMessage(new InformationMessage("OnMultiplayerGameStart"));
            base.OnMultiplayerGameStart(game, starterObject);
            //game.AddGameHandler<OfflineMultiplayerGameHandler>();
        }
        public override void OnGameInitializationFinished(Game game)
        {
            InformationManager.DisplayMessage(new InformationMessage("OnGameInitializationFinished"));
            base.OnGameInitializationFinished(game);
        }

        

        public override void OnMissionBehaviourInitialize(Mission mission)
        {
            InformationManager.DisplayMessage(new InformationMessage("OnMissionBehaviourInitialize"));
            string name = SteamFriends.GetPersonaName();
            InformationManager.DisplayMessage(new InformationMessage("OnAgentCreated" + name));
            string steamid = SteamUser.GetSteamID().ToString();
            InformationManager.DisplayMessage(new InformationMessage("OnAgentCreated" + steamid));
            base.OnMissionBehaviourInitialize(mission);
            //mission.AddMissionBehaviour(new MissionComponent());
        }
        protected override void OnSubModuleLoad()
        {
            InformationManager.DisplayMessage(new InformationMessage("OnSubModuleLoad"));
            Module.CurrentModule.AddMultiplayerGameMode(new CrpgBattleMissionBasedMultiplayerGamemode("ClassicBattle"));
            InformationManager.DisplayMessage(new InformationMessage("ClassicBattle"));

            Module.CurrentModule.GetMultiplayerGameTypes().First(x => x.GameType == "ClassicBattle").Scenes.Add("mp_skirmish_map_002f");
            base.OnSubModuleLoad();

            /*Module.CurrentModule.AddInitialStateOption(new InitialStateOption("Dump Items", new TextObject("Dump Items"), 9990, () =>
            {
                DumpItems();
                InformationManager.DisplayMessage(new InformationMessage("Exporting items to " + Path.GetFullPath(OutputPath)));
            }, false));*/
        }

       /* public static void DumpItems()
        {
            var mbItems = DeserializeMbItems("../../Modules/Native/ModuleData/mpitems.xml")
                .Where(i => i.ItemType != ItemObject.ItemTypeEnum.Shield && i.ItemType != ItemObject.ItemTypeEnum.HandArmor)
                .ToArray();
            var crpgItems = mbItems.Select(MbToCrpgItem).OrderBy(i => i.Value);

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
                crpgItem.HeadArmor = mbItem.ArmorComponent.HeadArmor;
                crpgItem.BodyArmor = mbItem.ArmorComponent.BodyArmor;
                crpgItem.ArmArmor = mbItem.ArmorComponent.ArmArmor;
                crpgItem.LegArmor = mbItem.ArmorComponent.LegArmor;
            }

            if (mbItem.HorseComponent != null)
            {
                crpgItem.BodyLength = mbItem.HorseComponent.BodyLength;
                crpgItem.ChargeDamage = mbItem.HorseComponent.ChargeDamage;
                crpgItem.Maneuver = mbItem.HorseComponent.Maneuver;
                crpgItem.Speed = mbItem.HorseComponent.Speed;
                crpgItem.HitPoints = 200 + mbItem.HorseComponent.HitPoints + mbItem.HorseComponent.HitPointBonus;
            }

            if (mbItem.WeaponComponent != null)
            {
                crpgItem.ThrustDamageType = MbToCrpgDamageType(mbItem.Weapons[0].ThrustDamageType);
                crpgItem.SwingDamageType = MbToCrpgDamageType(mbItem.Weapons[0].SwingDamageType);
                crpgItem.Accuracy = mbItem.Weapons[0].Accuracy;
                crpgItem.MissileSpeed = mbItem.Weapons[0].MissileSpeed;
                crpgItem.StackAmount = mbItem.Weapons[0].MaxDataValue;
                crpgItem.WeaponLength = mbItem.Weapons[0].WeaponLength;
                crpgItem.BodyArmor = mbItem.Weapons[0].BodyArmor;

                if (crpgItem.ThrustDamageType != null)
                {
                    crpgItem.PrimaryThrustDamage = mbItem.Weapons[0].ThrustDamage;
                    crpgItem.PrimaryThrustSpeed = mbItem.Weapons[0].ThrustSpeed;
                }

                if (crpgItem.SwingDamageType != null)
                {
                    crpgItem.PrimarySwingDamage = mbItem.Weapons[0].SwingDamage;
                    crpgItem.PrimarySwingSpeed = mbItem.Weapons[0].SwingSpeed;
                }

                crpgItem.PrimaryHandling = mbItem.Weapons[0].Handling;
                crpgItem.PrimaryWeaponFlags = (ulong?)mbItem.Weapons[0].WeaponFlags;

                if (mbItem.Weapons.Count > 1)
                {
                    if (crpgItem.ThrustDamageType != null)
                    {
                        crpgItem.SecondaryThrustDamage = mbItem.Weapons[1].ThrustDamage;
                        crpgItem.SecondaryThrustSpeed = mbItem.Weapons[1].ThrustSpeed;
                    }

                    if (crpgItem.SwingDamageType != null)
                    {
                        crpgItem.SecondarySwingDamage = mbItem.Weapons[1].SwingDamage;
                        crpgItem.SecondarySwingSpeed = mbItem.Weapons[1].SwingSpeed;
                    }

                    crpgItem.SecondaryHandling = mbItem.Weapons[1].Handling;
                    crpgItem.SecondaryWeaponFlags = (ulong?)mbItem.Weapons[1].WeaponFlags;
                }
            }

            return crpgItem;
        }

        private static int MbToCrpgItemType(ItemObject.ItemTypeEnum t)
        {
            return t switch
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
                _ => throw new ArgumentOutOfRangeException(nameof(t), t, null),
            };
        }

        private static int? MbToCrpgDamageType(DamageTypes t)
        {
            return t == DamageTypes.Invalid ? null : (int?)t;
        }

        private static IEnumerable<ItemObject> DeserializeMbItems(string path)
        {
            var game = new Game(new MultiplayerGame(), new MultiplayerGameManager());
            game.Initialize();

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
            });

            using var s = new StreamWriter(Path.Combine(outputPath, "mpitems.json"));
            serializer.Serialize(s, items);
        }

        private static void GenerateItemsThumbnail(IEnumerable<ItemObject> mbItems, string outputPath)
        {
            foreach (var mbItem in mbItems)
            {
                // Texture.SaveToFile doesn't accept absolute paths
                TableauCacheManager.Current.BeginCreateItemTexture(mbItem, texture =>
                    texture.SaveToFile(Path.Combine(outputPath, mbItem.StringId + ".png")));
            }
        }*/
    }
}
