using System.Reflection;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Battle;

internal class CrpgBattleSpawningBehavior : SpawningBehaviorBase
{
    private const float TotalSpawnDuration = 30f;
    private const float CavalrySpawnDelay = 10f;
    private readonly CrpgConstants _constants;
    private readonly MultiplayerRoundController? _roundController;
    private readonly HashSet<PlayerId> _notifiedPlayersAboutDelayedSpawn;
    private MissionTimer? _spawnTimer;
    private MissionTimer? _cavalrySpawnDelay;
    private bool _botsSpawned;

    public CrpgBattleSpawningBehavior(CrpgConstants constants, MultiplayerRoundController? roundController)
    {
        _constants = constants;
        _roundController = roundController;
        _notifiedPlayersAboutDelayedSpawn = new HashSet<PlayerId>();
    }

    public override void Initialize(SpawnComponent spawnComponent)
    {
        base.Initialize(spawnComponent);
        if (_roundController != null)
        {
            _roundController.OnPreparationEnded += RequestStartSpawnSession;
            _roundController.OnRoundEnding += RequestStopSpawnSession;
        }
    }

    public override void Clear()
    {
        base.Clear();
        if (_roundController != null)
        {
            _roundController.OnPreparationEnded -= RequestStartSpawnSession;
            _roundController.OnRoundEnding -= RequestStopSpawnSession;
        }
    }

    public override void OnTick(float dt)
    {
        if ((IsSpawningEnabled && IsRoundInProgress()) || _roundController == null)
        {
            SpawnAgents();
        }
    }

    public override void RequestStartSpawnSession()
    {
        base.RequestStartSpawnSession();
        _botsSpawned = false;
        _spawnTimer = new MissionTimer(TotalSpawnDuration); // Limit spawning for 30 seconds.
        _cavalrySpawnDelay = new MissionTimer(CavalrySpawnDelay); // Cav will spawn 6 seconds later.
        ResetSpawnTeams();
    }

    public override bool AllowEarlyAgentVisualsDespawning(MissionPeer missionPeer)
    {
        return false;
    }

    public bool SpawnDelayEnded()
    {
        return _cavalrySpawnDelay != null && _cavalrySpawnDelay!.Check();
    }

    protected override bool IsRoundInProgress()
    {
        return _roundController?.IsRoundInProgress ?? false;
    }

    protected override void SpawnAgents()
    {
        if (_roundController != null && _spawnTimer!.Check())
        {
            return;
        }

        if (!_botsSpawned || _roundController == null)
        {
            SpawnBotAgents();
            _botsSpawned = true;
        }

        SpawnPeerAgents();
    }

    private void ResetSpawnTeams()
    {
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            var crpgRepresentative = networkPeer.GetComponent<CrpgRepresentative>();
            if (crpgRepresentative != null)
            {
                crpgRepresentative.SpawnTeamThisRound = null;
            }
        }

        _notifiedPlayersAboutDelayedSpawn.Clear();
    }

    private void SpawnBotAgents()
    {
        BasicCultureObject cultureTeam1 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
        BasicCultureObject cultureTeam2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());
        int botsTeam1 = MultiplayerOptions.OptionType.NumberOfBotsTeam1.GetIntValue();
        int botsTeam2 = MultiplayerOptions.OptionType.NumberOfBotsTeam2.GetIntValue();

        if (botsTeam1 <= 0 && botsTeam2 <= 0)
        {
            return;
        }

        Mission.Current.AllowAiTicking = false;
        foreach (Team team in Mission.Teams)
        {
            if (Mission.AttackerTeam != team && Mission.DefenderTeam != team)
            {
                continue;
            }

            BasicCultureObject teamCulture = team == Mission.AttackerTeam ? cultureTeam1 : cultureTeam2;
            int numberOfBots = Mission.AttackerTeam == team ? botsTeam1 : botsTeam2;
            int botsAlive = team.ActiveAgents.Count(a => a.IsAIControlled && a.IsHuman);

            for (int i = 0 + botsAlive; i < numberOfBots; i += 1)
            {
                MultiplayerClassDivisions.MPHeroClass botClass = MultiplayerClassDivisions
                    .GetMPHeroClasses()
                    .GetRandomElementWithPredicate<MultiplayerClassDivisions.MPHeroClass>(x => x.Culture == teamCulture);
                BasicCharacterObject character = botClass.HeroCharacter;

                bool hasMount = character.Equipment[EquipmentIndex.Horse].Item != null;
                MatrixFrame spawnFrame = SpawnComponent.GetSpawnFrame(team, hasMount, true);
                Vec2 initialDirection = spawnFrame.rotation.f.AsVec2.Normalized();

                AgentBuildData agentBuildData = new AgentBuildData(character)
                    .Equipment(character.Equipment)
                    .TroopOrigin(new BasicBattleAgentOrigin(character))
                    .EquipmentSeed(MissionLobbyComponent.GetRandomFaceSeedForCharacter(character))
                    .Team(team)
                    .VisualsIndex(0)
                    .InitialPosition(in spawnFrame.origin)
                    .InitialDirection(in initialDirection)
                    .IsFemale(character.IsFemale)
                    .ClothingColor1(team.Side == BattleSideEnum.Attacker ? teamCulture.Color : teamCulture.ClothAlternativeColor)
                    .ClothingColor2(team.Side == BattleSideEnum.Attacker ? teamCulture.Color2 : teamCulture.ClothAlternativeColor2)
                    .NoHorses(true); // Bots with horses are just running around most of the time.

                var bodyProperties = BodyProperties.GetRandomBodyProperties(
                    character.Race,
                    character.IsFemale,
                    character.GetBodyPropertiesMin(),
                    character.GetBodyPropertiesMax(),
                    (int)agentBuildData.AgentOverridenSpawnEquipment.HairCoverType,
                    agentBuildData.AgentEquipmentSeed,
                    character.HairTags,
                    character.BeardTags,
                    character.TattooTags);
                agentBuildData.BodyProperties(bodyProperties);

                Agent agent = Mission.SpawnAgent(agentBuildData);
                agent.SetWatchState(Agent.WatchState.Alarmed);
                agent.WieldInitialWeapons();
            }
        }
    }

    private void SpawnPeerAgents()
    {
        BasicCultureObject cultureTeam1 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
        BasicCultureObject cultureTeam2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());

        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            var missionPeer = networkPeer.GetComponent<MissionPeer>();
            var crpgRepresentative = networkPeer.GetComponent<CrpgRepresentative>();
            if (!networkPeer.IsSynchronized
                || missionPeer.ControlledAgent != null
                || missionPeer.HasSpawnedAgentVisuals
                || missionPeer.Team == null
                || missionPeer.Team == Mission.SpectatorTeam
                || crpgRepresentative?.User == null
                || crpgRepresentative.SpawnTeamThisRound != null)
            {
                continue;
            }

            var characterEquipment = CreateCharacterEquipment(crpgRepresentative.User.Character.EquippedItems);
            if (!DoesEquipmentContainWeapon(characterEquipment)) // Disallow spawning without weapons.
            {
                continue;
            }

            bool hasMount = characterEquipment[EquipmentIndex.Horse].Item != null;
            // Disallow spawning cavalry before the cav spawn delay ended.
            if (hasMount && (!_cavalrySpawnDelay?.Check() ?? false))
            {
                if (_notifiedPlayersAboutDelayedSpawn.Add(networkPeer.VirtualPlayer.Id))
                {
                    GameNetwork.BeginModuleEventAsServer(networkPeer);
                    GameNetwork.WriteMessage(new CrpgNotification
                    {
                        Type = CrpgNotification.NotificationType.Notification,
                        Message = $"Cavalry will spawn in {CavalrySpawnDelay} seconds!",
                        IsMessageTextId = false,
                        SoundEvent = string.Empty,
                    });
                    GameNetwork.EndModuleEventAsServer();
                }

                continue;
            }

            BasicCultureObject teamCulture = missionPeer.Team == Mission.AttackerTeam ? cultureTeam1 : cultureTeam2;
            var peerClass = MultiplayerClassDivisions.GetMPHeroClasses().Skip(5).First();
            // var character = CreateCharacter(crpgRepresentative.User.Character, _constants);
            var characterSkills = CreateCharacterSkills(crpgRepresentative.User.Character.Characteristics);
            var character = peerClass.HeroCharacter;

            // Used to reset the selected perks for the current troop. Otherwise the player might have addional stats.
            missionPeer.SelectedTroopIndex = -1;
            MatrixFrame spawnFrame = missionPeer.GetAmountOfAgentVisualsForPeer() > 0
                ? missionPeer.GetAgentVisualForPeer(0).GetFrame()
                : SpawnComponent.GetSpawnFrame(missionPeer.Team, hasMount, true);
            Vec2 initialDirection = spawnFrame.rotation.f.AsVec2.Normalized();

            uint color1;
            uint color2;
            // Banner? banner;
            if (crpgRepresentative.Clan != null)
            {
                color1 = crpgRepresentative.Clan.PrimaryColor;
                color2 = crpgRepresentative.Clan.SecondaryColor;
                // TryParseBanner(crpgRepresentative.Clan.BannerKey, out banner);
            }
            else
            {
                color1 = missionPeer.Team == Mission.AttackerTeam
                    ? teamCulture.Color
                    : teamCulture.ClothAlternativeColor;
                color2 = missionPeer.Team == Mission.AttackerTeam
                    ? teamCulture.Color2
                    : teamCulture.ClothAlternativeColor2;
                // banner = null;
            }

            AgentBuildData agentBuildData = new AgentBuildData(character)
                .MissionPeer(missionPeer)
                .Equipment(characterEquipment)
                .TroopOrigin(new CrpgBattleAgentOrigin(character, characterSkills))
                .Team(missionPeer.Team)
                .VisualsIndex(0)
                .IsFemale(missionPeer.Peer.IsFemale)
                .ClothingColor1(color1)
                .ClothingColor2(color2)
                // .Banner(banner)
                .BodyProperties(GetBodyProperties(missionPeer, teamCulture))
                .InitialPosition(in spawnFrame.origin)
                .InitialDirection(in initialDirection);

            Agent agent = Mission.SpawnAgent(agentBuildData);
            agent.WieldInitialWeapons();

            if (_roundController != null)
            {
                missionPeer.SpawnCountThisRound += 1;
                crpgRepresentative.SpawnTeamThisRound = missionPeer.Team;
            }
        }
    }

    private BasicCharacterObject CreateCharacter(CrpgCharacter crpgCharacter, CrpgConstants constants)
    {
        var skills = CreateCharacterSkills(crpgCharacter.Characteristics);
        return CrpgCharacterObject.New(new TextObject(crpgCharacter.Name), skills, constants);
    }

#pragma warning disable SA1202 // Suppress the static warning.
    internal static CharacterSkills CreateCharacterSkills(CrpgCharacterCharacteristics characteristics)
    {
        CharacterSkills skills = new();
        skills.SetPropertyValue(CrpgSkills.Strength, characteristics.Attributes.Strength);
        skills.SetPropertyValue(CrpgSkills.Agility, characteristics.Attributes.Agility);

        skills.SetPropertyValue(CrpgSkills.IronFlesh, characteristics.Skills.IronFlesh);
        skills.SetPropertyValue(CrpgSkills.PowerStrike, characteristics.Skills.PowerStrike);
        skills.SetPropertyValue(CrpgSkills.PowerDraw, characteristics.Skills.PowerDraw);
        skills.SetPropertyValue(CrpgSkills.PowerThrow, characteristics.Skills.PowerThrow);
        skills.SetPropertyValue(DefaultSkills.Athletics, characteristics.Skills.Athletics * 20 + 2 * characteristics.Attributes.Agility);
        skills.SetPropertyValue(DefaultSkills.Riding, characteristics.Skills.Riding * 20);
        skills.SetPropertyValue(CrpgSkills.WeaponMaster, characteristics.Skills.WeaponMaster);
        skills.SetPropertyValue(CrpgSkills.MountedArchery, characteristics.Skills.MountedArchery);
        skills.SetPropertyValue(CrpgSkills.Shield, characteristics.Skills.Shield);

        skills.SetPropertyValue(DefaultSkills.OneHanded, characteristics.WeaponProficiencies.OneHanded);
        skills.SetPropertyValue(DefaultSkills.TwoHanded, characteristics.WeaponProficiencies.TwoHanded);
        skills.SetPropertyValue(DefaultSkills.Polearm, characteristics.WeaponProficiencies.Polearm);
        skills.SetPropertyValue(DefaultSkills.Bow, characteristics.WeaponProficiencies.Bow);
        skills.SetPropertyValue(DefaultSkills.Crossbow, characteristics.WeaponProficiencies.Crossbow);
        skills.SetPropertyValue(DefaultSkills.Throwing, characteristics.WeaponProficiencies.Throwing);

        return skills;
    }
#pragma warning restore SA1202

    private Equipment CreateCharacterEquipment(IList<CrpgEquippedItem> equippedItems)
    {
        Equipment equipment = new();
        /*foreach (var equippedItem in equippedItems)
        {
            var index = ItemSlotToIndex[equippedItem.Slot];
            AddEquipment(equipment, index, equippedItem.UserItem.BaseItemId);
        }*/
        AddEquipment(equipment, EquipmentIndex.Weapon0, "crpg_noble_bow");
        AddEquipment(equipment, EquipmentIndex.Weapon1, "crpg_noble_long_bow");
        AddEquipment(equipment, EquipmentIndex.Weapon2, "crpg_glen_ranger_bow.");
        AddEquipment(equipment, EquipmentIndex.Weapon3, "crpg_bodkin_arrows_a");

        return equipment;
    }

    private bool DoesEquipmentContainWeapon(Equipment equipment)
    {
        for (var i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.ExtraWeaponSlot; i += 1)
        {
            if (!equipment[i].IsEmpty)
            {
                return true;
            }
        }

        return false;
    }

    private void AddEquipment(Equipment equipments, EquipmentIndex idx, string itemId)
    {
        var itemObject = MBObjectManager.Instance.GetObject<ItemObject>(itemId);
        if (itemObject == null)
        {
            Debug.Print($"Cannot equip unknown item '{itemId}'");
            return;
        }

        EquipmentElement equipmentElement = new(itemObject);
        equipments.AddEquipmentToSlotWithoutAgent(idx, equipmentElement);
    }

    private bool TryParseBanner(string bannerKey, out Banner? banner)
    {
        try
        {
            banner = new Banner(bannerKey);
            return true;
        }
        catch
        {
            banner = null;
            return false;
        }
    }

    private static readonly Dictionary<CrpgItemSlot, EquipmentIndex> ItemSlotToIndex = new()
    {
        [CrpgItemSlot.Head] = EquipmentIndex.Head,
        [CrpgItemSlot.Shoulder] = EquipmentIndex.Cape,
        [CrpgItemSlot.Body] = EquipmentIndex.Body,
        [CrpgItemSlot.Hand] = EquipmentIndex.Gloves,
        [CrpgItemSlot.Leg] = EquipmentIndex.Leg,
        [CrpgItemSlot.MountHarness] = EquipmentIndex.HorseHarness,
        [CrpgItemSlot.Mount] = EquipmentIndex.Horse,
        [CrpgItemSlot.Weapon0] = EquipmentIndex.Weapon0,
        [CrpgItemSlot.Weapon1] = EquipmentIndex.Weapon1,
        [CrpgItemSlot.Weapon2] = EquipmentIndex.Weapon2,
        [CrpgItemSlot.Weapon3] = EquipmentIndex.Weapon3,
    };
}
