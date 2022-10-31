using System.Text;
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
    private readonly CrpgConstants _constants;
    private readonly MultiplayerRoundController? _roundController;
    private readonly HashSet<PlayerId> _notifiedPlayersAboutSpawnRestriction;
    private MissionTimer? _spawnTimer;
    private MissionTimer? _cavalrySpawnDelayTimer;
    private bool _botsSpawned;

    public CrpgBattleSpawningBehavior(CrpgConstants constants, MultiplayerRoundController? roundController)
    {
        _constants = constants;
        _roundController = roundController;
        _notifiedPlayersAboutSpawnRestriction = new HashSet<PlayerId>();
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
        _cavalrySpawnDelayTimer = new MissionTimer(GetCavalrySpawnDelay()); // Cav will spawn X seconds later.
        ResetSpawnTeams();
    }

    public override bool AllowEarlyAgentVisualsDespawning(MissionPeer missionPeer)
    {
        return false;
    }

    public bool SpawnDelayEnded()
    {
        return _cavalrySpawnDelayTimer != null && _cavalrySpawnDelayTimer!.Check();
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

    /// <summary>
    /// Cav spawn delay values
    /// 10 => 7sec
    /// 30 => 9sec
    /// 60 => 13sec
    /// 90 => 17sec
    /// 120 => 22sec
    /// 150 => 26sec
    /// 165+ => 28sec.
    /// </summary>
    private int GetCavalrySpawnDelay()
    {
        int currentPlayers = Math.Max(GetCurrentPlayerCount(), 1);
        return Math.Min(28, 5 + currentPlayers / 7);
    }

    private int GetCurrentPlayerCount()
    {
        int counter = 0;
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            var missionPeer = networkPeer.GetComponent<MissionPeer>();
            if (!networkPeer.IsSynchronized || missionPeer == null || missionPeer.Team.Side == BattleSideEnum.None)
            {
                continue;
            }

            counter++;
        }

        return counter;
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

        _notifiedPlayersAboutSpawnRestriction.Clear();
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
                if (_notifiedPlayersAboutSpawnRestriction.Add(networkPeer.VirtualPlayer.Id))
                {
                    GameNetwork.BeginModuleEventAsServer(networkPeer);
                    GameNetwork.WriteMessage(new CrpgNotification
                    {
                        Type = CrpgNotification.NotificationType.Notification,
                        Message = "You should have at least one weapon equipped to spawn!",
                        IsMessageTextId = false,
                        SoundEvent = string.Empty,
                    });
                    GameNetwork.EndModuleEventAsServer();
                }

                continue;
            }

            bool hasMount = characterEquipment[EquipmentIndex.Horse].Item != null;
            // Disallow spawning cavalry before the cav spawn delay ended.
            if (hasMount && (!_cavalrySpawnDelayTimer?.Check() ?? false))
            {
                if (_notifiedPlayersAboutSpawnRestriction.Add(networkPeer.VirtualPlayer.Id))
                {
                    GameNetwork.BeginModuleEventAsServer(networkPeer);
                    GameNetwork.WriteMessage(new CrpgNotification
                    {
                        Type = CrpgNotification.NotificationType.Notification,
                        Message = $"Cavalry will spawn in {_cavalrySpawnDelayTimer?.GetTimerDuration()} seconds!",
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

            AgentBuildData agentBuildData = new AgentBuildData(character)
                .MissionPeer(missionPeer)
                .Equipment(characterEquipment)
                .TroopOrigin(new CrpgBattleAgentOrigin(character, characterSkills))
                .Team(missionPeer.Team)
                .VisualsIndex(0)
                .IsFemale(missionPeer.Peer.IsFemale)
                .BodyProperties(GetBodyProperties(missionPeer, teamCulture))
                .InitialPosition(in spawnFrame.origin)
                .InitialDirection(in initialDirection);

            if (crpgRepresentative.Clan != null)
            {
                agentBuildData.ClothingColor1(crpgRepresentative.Clan.PrimaryColor);
                agentBuildData.ClothingColor2(crpgRepresentative.Clan.SecondaryColor);
                if (TryParseBanner(crpgRepresentative.Clan.BannerKey, out var banner))
                {
                    agentBuildData.Banner(banner);
                }
            }
            else
            {
                agentBuildData.ClothingColor1(missionPeer.Team == Mission.AttackerTeam
                    ? teamCulture.Color
                    : teamCulture.ClothAlternativeColor);
                agentBuildData.ClothingColor2(missionPeer.Team == Mission.AttackerTeam
                    ? teamCulture.Color2
                    : teamCulture.ClothAlternativeColor2);
            }

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

        if (!Equipment.IsItemFitsToSlot(idx, itemObject))
        {
            Debug.Print($"Cannot equip item '{itemId} on slot {idx}");
            return;
        }

        EquipmentElement equipmentElement = new(itemObject);
        equipments.AddEquipmentToSlotWithoutAgent(idx, equipmentElement);
    }

    private bool TryParseBanner(string bannerKey, out Banner? banner)
    {
        banner = null;

        if (bannerKey.Length > _constants.ClanBannerKeyMaxLength)
        {
            return false;
        }

        string[] array = bannerKey.Split('.');

        StringBuilder fixedBannerCode = new();
        // The maximum size of the banner is Banner.BannerFullSize. But apparently negative values do not cause crashes. Anyway added some checks with tolerance to parse the banner.
        int maxX = 2 * Banner.BannerFullSize;
        int minX = -2 * Banner.BannerFullSize;
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
        for (int i = 0; i + 10 <= array.Length; i += 10)
        {
            if (int.TryParse(array[i], out int iconId))
            {
                if (!CheckIconList(iconId))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            if (!int.TryParse(array[i + 1], out int colorId1) || !BannerManager.Instance.ReadOnlyColorPalette.ContainsKey(colorId1)
                || !int.TryParse(array[i + 2], out int colorId2) || !BannerManager.Instance.ReadOnlyColorPalette.ContainsKey(colorId2)
                || !int.TryParse(array[i + 3], out int sizeX)
                || !int.TryParse(array[i + 4], out int sizeY)
                || !int.TryParse(array[i + 5], out int posX) || posX > maxX || posX < minX
                || !int.TryParse(array[i + 6], out int posY) || posY > maxY || posY < minY
                || !int.TryParse(array[i + 7], out int drawStroke) || drawStroke > 1 || drawStroke < 0
                || !int.TryParse(array[i + 8], out int mirror) || mirror > 1 || mirror < 0)
            {
                return false;
            }

            if (!int.TryParse(array[i + 9], out int rotation))
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

            fixedBannerCode.Append(iconId);
            fixedBannerCode.Append(".");
            fixedBannerCode.Append(colorId1);
            fixedBannerCode.Append(".");
            fixedBannerCode.Append(colorId2);
            fixedBannerCode.Append(".");
            fixedBannerCode.Append(sizeX);
            fixedBannerCode.Append(".");
            fixedBannerCode.Append(sizeY);
            fixedBannerCode.Append(".");
            fixedBannerCode.Append(posX);
            fixedBannerCode.Append(".");
            fixedBannerCode.Append(posY);
            fixedBannerCode.Append(".");
            fixedBannerCode.Append(drawStroke);
            fixedBannerCode.Append(".");
            fixedBannerCode.Append(mirror);
            fixedBannerCode.Append(".");
            fixedBannerCode.Append(rotation);
            fixedBannerCode.Append(".");
        }

        if (fixedBannerCode.Length == 0)
        {
            return false;
        }

        fixedBannerCode.Length -= ".".Length;

        banner = new Banner(fixedBannerCode.ToString());
        return true;
    }

    private bool CheckIconList(int id)
    {
        foreach (BannerIconGroup bannerIconGroup in BannerManager.Instance.BannerIconGroups)
        {
            if (bannerIconGroup.AllBackgrounds.ContainsKey(id) || bannerIconGroup.AllIcons.ContainsKey(id))
            {
                return true;
            }
        }

        return false;
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
        [CrpgItemSlot.WeaponExtra] = EquipmentIndex.ExtraWeaponSlot,
    };
}
