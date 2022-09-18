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
    private readonly MultiplayerRoundController? _roundController;
    private MissionTimer? _spawnTimer;
    private bool _botsSpawned;

    public CrpgBattleSpawningBehavior(CrpgConstants constants, MultiplayerRoundController? roundController)
    {
        _constants = constants;
        _roundController = roundController;
    }

    public override void Initialize(SpawnComponent spawnComponent)
    {
        base.Initialize(spawnComponent);
        if (_roundController != null)
        {
            _roundController.OnRoundStarted += RequestStartSpawnSession;
            _roundController.OnRoundEnding += RequestStopSpawnSession;
        }
    }

    public override void Clear()
    {
        base.Clear();
        if (_roundController != null)
        {
            _roundController.OnRoundStarted -= RequestStartSpawnSession;
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
        _spawnTimer = new MissionTimer(30f); // Limit spawning for 30 seconds.
        ResetSpawnTeams();
    }

    public override bool AllowEarlyAgentVisualsDespawning(MissionPeer missionPeer)
    {
        return false;
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

            BasicCultureObject teamCulture = missionPeer.Team == Mission.AttackerTeam ? cultureTeam1 : cultureTeam2;
            var peerClass = MultiplayerClassDivisions.GetMPHeroClasses().Skip(5).First();
            // var character = CreateCharacter(crpgRepresentative.User.Character, _constants);
            var characterSkills = CreateCharacterSkills(crpgRepresentative.User.Character.Characteristics);
            var characterEquipment = CreateCharacterEquipment(crpgRepresentative.User.Character.EquippedItems);
            var character = peerClass.HeroCharacter;

            bool hasMount = characterEquipment[EquipmentIndex.Horse].Item != null;
            MatrixFrame spawnFrame = missionPeer.GetAmountOfAgentVisualsForPeer() > 0
                ? missionPeer.GetAgentVisualForPeer(0).GetFrame()
                : SpawnComponent.GetSpawnFrame(missionPeer.Team, hasMount, true);
            Vec2 initialDirection = spawnFrame.rotation.f.AsVec2.Normalized();

            uint color1;
            uint color2;
            if (crpgRepresentative.Clan != null && TryParseBanner(crpgRepresentative.Clan.BannerKey, out var banner))
            {
                color1 = crpgRepresentative.Clan.PrimaryColor;
                color2 = crpgRepresentative.Clan.SecondaryColor;
            }
            else
            {
                banner = Banner.CreateRandomClanBanner();
                color1 = banner.GetPrimaryColor();
                color2 = banner.GetSecondaryColor();
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
                .Banner(banner)
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
        foreach (var equippedItem in equippedItems)
        {
            var index = ItemSlotToIndex[equippedItem.Slot];
            AddEquipment(equipment, index, equippedItem.UserItem.BaseItemId);
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


    private string? CheckIconList(int id)
    {
        foreach (BannerIconGroup bannerIconGroup in BannerManager.Instance.BannerIconGroups)
        {
            if (bannerIconGroup.AllBackgrounds.ContainsKey(id))
            {
                return bannerIconGroup.AllBackgrounds[id];
            }
            else if (bannerIconGroup.AllIcons.ContainsKey(id))
            {
                return bannerIconGroup.AllIcons[id].MaterialName;
            }
        }

        return null;
    }

    private bool TryParseBanner(string bannerKey, out Banner? banner)
    {
        banner = null;
        string[] array = bannerKey.Split('.');

        string fixedBannerCode = string.Empty;
        // The maximum size of the banner is Banner.BannerFullSize. But apparently negative values do not cause crashes. Anyway added some checks with tolerance to parse the banner.
        int maxX = 2 * (Banner.BannerFullSize / 2);
        int minX = 2 * -1 * (Banner.BannerFullSize / 2);
        int maxY = maxX;
        int minY = minX;

        /*
         * Format values seperated by dots (.)
         * Icons / Colors found inside of the banner_icons.xml
         * --------
         * iconId
         * colorId
         * colorId2
         * sizeX
         * sizeY
         * posX  (total canvas size is Banner.BannerFullSize but being out of these doesn't seem to cause any issues)
         * posY
         * stroke (0 or 1)
         * mirror (0 or 1)
         * rotation (0-359)
         */

        int iconCounter = 0;
        int num = 0;
        while (num + 10 <= array.Length)
        {
            // 10 is the icon limit. If you try more then 10 icons you won't even spawn.. if you have like 20+ the server might crash.
            if (iconCounter == 10)
            {
                return false;
            }

            if (int.TryParse(array[num], out int iconId))
            {
                if (CheckIconList(iconId) == null)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            if (!int.TryParse(array[num + 1], out int colorId1) || !BannerManager.Instance.ReadOnlyColorPalette.ContainsKey(colorId1)
            || !int.TryParse(array[num + 2], out int colorId2) || !BannerManager.Instance.ReadOnlyColorPalette.ContainsKey(colorId2)
            || !int.TryParse(array[num + 3], out int sizeX)
            || !int.TryParse(array[num + 4], out int sizeY)
            || !int.TryParse(array[num + 5], out int posX) || posX > maxX || posX < minX
            || !int.TryParse(array[num + 6], out int posY) || posY > maxY || posY < minY
            || !int.TryParse(array[num + 7], out int drawStroke) || drawStroke > 1 || drawStroke < 0
            || !int.TryParse(array[num + 8], out int mirrior) || mirrior > 1 || mirrior < 0)
            {
                return false;
            }

            if (!int.TryParse(array[num + 9], out int rotation))
            {
                return false;
            }
            else
            {
                rotation %= 360;
                if (rotation < 0)
                {
                    rotation += 360;
                }
            }

            fixedBannerCode += $"{iconId}.{colorId1}.{colorId2}.{sizeX}.{sizeY}.{posX}.{posY}.{drawStroke}.{mirrior}.{rotation}.";
            num += 10;
            iconCounter++;
        }

        if (fixedBannerCode[fixedBannerCode.Length - 1] == '.')
        {
            fixedBannerCode = fixedBannerCode.Substring(0, fixedBannerCode.Length - 1);
        }

        banner = new Banner(fixedBannerCode);
        return true;
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
