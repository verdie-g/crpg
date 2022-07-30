using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Battle;

internal class CrpgBattleSpawningBehavior : SpawningBehaviorBase
{
    private readonly CrpgConstants _constants;
    private readonly MultiplayerRoundController _roundController;
    private MissionTimer? _spawnTimer;
    private bool _botsSpawned;

    public CrpgBattleSpawningBehavior(CrpgConstants constants, MultiplayerRoundController roundController)
    {
        _constants = constants;
        _roundController = roundController;
    }

    public override void Initialize(SpawnComponent spawnComponent)
    {
        base.Initialize(spawnComponent);
        _roundController.OnRoundStarted += RequestStartSpawnSession;
        _roundController.OnRoundEnding += RequestStopSpawnSession;
    }

    public override void Clear()
    {
        base.Clear();
        _roundController.OnRoundStarted -= RequestStartSpawnSession;
        _roundController.OnRoundEnding -= RequestStopSpawnSession;
    }

    public override void OnTick(float dt)
    {
        if (IsSpawningEnabled && IsRoundInProgress())
        {
            SpawnAgents();
        }
    }

    public override void RequestStartSpawnSession()
    {
        base.RequestStartSpawnSession();
        _botsSpawned = false;
        _spawnTimer = new MissionTimer(30f); // Limit spawning for 30 seconds.
        ResetSpawnTeams();
    }

    public override bool AllowEarlyAgentVisualsDespawning(MissionPeer missionPeer)
    {
        return false;
    }

    protected override bool IsRoundInProgress()
    {
        return _roundController.IsRoundInProgress;
    }

    protected override void SpawnAgents()
    {
        if (_spawnTimer!.Check())
        {
            return;
        }

        if (!_botsSpawned)
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
            for (int i = 0; i < numberOfBots; i += 1)
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
                    .SpawnOnInitialPoint(true)
                    .IsFemale(character.IsFemale)
                    .ClothingColor1(team.Side == BattleSideEnum.Attacker ? teamCulture.Color : teamCulture.ClothAlternativeColor)
                    .ClothingColor2(team.Side == BattleSideEnum.Attacker ? teamCulture.Color2 : teamCulture.ClothAlternativeColor2)
                    .NoHorses(true); // Bots with horses are just running around most of the time.

                var bodyProperties = BodyProperties.GetRandomBodyProperties(
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

            BasicCultureObject teamCulture = missionPeer.Team == Mission.AttackerTeam ? cultureTeam1 : cultureTeam2;
            var peerClass = MultiplayerClassDivisions.GetMPHeroClasses().Skip(5).First();
            // var character = CreateCharacter(crpgRepresentative.User.Character, _constants);
            var characterEquipment = CreateCharacterEquipment(crpgRepresentative.User.Character.EquippedItems);
            var character = peerClass.HeroCharacter;

            bool hasMount = characterEquipment[EquipmentIndex.Horse].Item != null;
            MatrixFrame spawnFrame = missionPeer.GetAmountOfAgentVisualsForPeer() > 0
                ? missionPeer.GetAgentVisualForPeer(0).GetFrame()
                : SpawnComponent.GetSpawnFrame(missionPeer.Team, hasMount, true);
            Vec2 initialDirection = spawnFrame.rotation.f.AsVec2.Normalized();

            AgentBuildData agentBuildData = new AgentBuildData(character)
                .MissionPeer(missionPeer)
                .Equipment(characterEquipment)
                .Team(missionPeer.Team)
                .VisualsIndex(0)
                .SpawnOnInitialPoint(true)
                .IsFemale(missionPeer.Peer.IsFemale)
                .ClothingColor1(missionPeer.Team == Mission.AttackerTeam
                    ? teamCulture.Color
                    : teamCulture.ClothAlternativeColor)
                .ClothingColor2(missionPeer.Team == Mission.AttackerTeam
                    ? teamCulture.Color2
                    : teamCulture.ClothAlternativeColor2)
                .BodyProperties(GetBodyProperties(missionPeer, teamCulture))
                .InitialPosition(in spawnFrame.origin)
                .InitialDirection(in initialDirection);

            Agent agent = Mission.SpawnAgent(agentBuildData);
            agent.WieldInitialWeapons();

            missionPeer.SpawnCountThisRound += 1;
            crpgRepresentative.SpawnTeamThisRound = missionPeer.Team;
        }
    }

    private BasicCharacterObject CreateCharacter(CrpgCharacter crpgCharacter, CrpgConstants constants)
    {
        CharacterSkills skills = new();
        skills.SetPropertyValue(CrpgSkills.Strength, crpgCharacter.Statistics.Attributes.Strength);
        skills.SetPropertyValue(CrpgSkills.Agility, crpgCharacter.Statistics.Attributes.Agility);

        skills.SetPropertyValue(CrpgSkills.IronFlesh, crpgCharacter.Statistics.Skills.IronFlesh);
        skills.SetPropertyValue(CrpgSkills.PowerStrike, crpgCharacter.Statistics.Skills.PowerStrike);
        skills.SetPropertyValue(CrpgSkills.PowerDraw, crpgCharacter.Statistics.Skills.PowerDraw);
        skills.SetPropertyValue(CrpgSkills.PowerThrow, crpgCharacter.Statistics.Skills.PowerThrow);
        skills.SetPropertyValue(DefaultSkills.Athletics, crpgCharacter.Statistics.Skills.Athletics * 20 + 2 * crpgCharacter.Statistics.Attributes.Agility);
        skills.SetPropertyValue(DefaultSkills.Riding, crpgCharacter.Statistics.Skills.Riding * 20);
        skills.SetPropertyValue(CrpgSkills.WeaponMaster, crpgCharacter.Statistics.Skills.WeaponMaster);
        skills.SetPropertyValue(CrpgSkills.MountedArchery, crpgCharacter.Statistics.Skills.MountedArchery);

        skills.SetPropertyValue(DefaultSkills.OneHanded, crpgCharacter.Statistics.WeaponProficiencies.OneHanded);
        skills.SetPropertyValue(DefaultSkills.TwoHanded, crpgCharacter.Statistics.WeaponProficiencies.TwoHanded);
        skills.SetPropertyValue(DefaultSkills.Polearm, crpgCharacter.Statistics.WeaponProficiencies.Polearm);
        skills.SetPropertyValue(DefaultSkills.Bow, crpgCharacter.Statistics.WeaponProficiencies.Bow);
        skills.SetPropertyValue(DefaultSkills.Crossbow, crpgCharacter.Statistics.WeaponProficiencies.Crossbow);
        skills.SetPropertyValue(DefaultSkills.Throwing, crpgCharacter.Statistics.WeaponProficiencies.Throwing);

        return CrpgCharacterObject.New(new TextObject(crpgCharacter.Name), skills, constants);
    }

    private Equipment CreateCharacterEquipment(IList<CrpgEquippedItem> equippedItems)
    {
        Equipment equipment = new();
        foreach (var equippedItem in equippedItems)
        {
            var index = ItemSlotToIndex[equippedItem.Slot];
            AddEquipment(equipment, index, equippedItem.UserItem.BaseItem.Id);
        }

        return equipment;
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
