using System.Xml.Linq;
using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Modes.Dtv;

internal class CrpgDtvSpawningBehavior : CrpgSpawningBehaviorBase
{
    public bool BotsSpawned { get; set; }

    private const float TotalSpawnDuration = 15f;
    private readonly XDocument? _dtvData;
    private readonly MultiplayerRoundController _roundController;
    private readonly HashSet<PlayerId> _notifiedPlayersAboutSpawnRestriction;

    public int Wave { get; set; }
    public int Round { get; set; }
    private MissionTimer? _spawnTimer;
    private MissionTimer? _cavalrySpawnDelayTimer;
    private bool _viscountSpawned;

    public CrpgDtvSpawningBehavior(CrpgConstants constants, MultiplayerRoundController roundController)
        : base(constants)
    {
        _roundController = roundController;
        _notifiedPlayersAboutSpawnRestriction = new HashSet<PlayerId>();
        _dtvData = new XDocument();
        _dtvData = XDocument.Load(ModuleHelper.GetXmlPath("Crpg", "dtv_data"));
    }

    public override void Initialize(SpawnComponent spawnComponent)
    {
        base.Initialize(spawnComponent);
        _roundController.OnPreparationEnded += RequestStartSpawnSession;
        _roundController.OnRoundEnding += RequestStopSpawnSession;
        Wave = 1;
        Round = 1;
    }

    public override void Clear()
    {
        base.Clear();
        _roundController.OnPreparationEnded -= RequestStartSpawnSession;
        _roundController.OnRoundEnding -= RequestStopSpawnSession;
    }

    public override void OnTick(float dt)
    {
        if (!BotsSpawned && _viscountSpawned)
        {
            SpawnAttackingBots(Wave, Round);
            BotsSpawned = true;
        }

        if (!IsSpawningEnabled)
        {
            return;
        }

        if (_spawnTimer!.Check())
        {
            return;
        }

        if (!_viscountSpawned)
        {
            SpawnViscount();
            _viscountSpawned = true;
        }

        SpawnAgents();
    }

    public override void RequestStartSpawnSession()
    {
        base.RequestStartSpawnSession();
        _viscountSpawned = false;
        BotsSpawned = false;
        _spawnTimer = new MissionTimer(TotalSpawnDuration); // Limit spawning within timer
        _cavalrySpawnDelayTimer = new MissionTimer(GetCavalrySpawnDelay()); // Cav will spawn X seconds later.
        _notifiedPlayersAboutSpawnRestriction.Clear();
    }

    public void RequestNewWaveSpawnSession()
    {
        Debug.Print($"Requesting spawn session for new wave");
        base.RequestStartSpawnSession();
        _spawnTimer = new MissionTimer(TotalSpawnDuration); // Limit spawning within timer
        _cavalrySpawnDelayTimer = new MissionTimer(GetCavalrySpawnDelay()); // Cav will spawn X seconds later.
        _notifiedPlayersAboutSpawnRestriction.Clear();
    }

    public bool SpawnDelayEnded()
    {
        return _cavalrySpawnDelayTimer != null && _cavalrySpawnDelayTimer!.Check();
    }

    protected void SpawnViscount()
    {
        Debug.Print("Attempting to spawn viscount");
        BasicCultureObject teamCulture = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());
        Mission.Current.AllowAiTicking = false;
        Team team = Mission.DefenderTeam; // viscount is a defender
        MultiplayerClassDivisions.MPHeroClass botClass = MultiplayerClassDivisions
            .GetMPHeroClasses()
            .GetRandomElementWithPredicate<MultiplayerClassDivisions.MPHeroClass>(x => x.StringId.StartsWith("crpg_dtv_viscount"));
        BasicCharacterObject character = botClass.HeroCharacter;

        GameEntity? viscountSpawnPoint = Mission.Current.Scene.FindEntityWithTag("viscount");
        MatrixFrame spawnFrame = GetviscountSpawnFrame(team, viscountSpawnPoint);
        Vec2 initialDirection = spawnFrame.rotation.f.AsVec2.Normalized();
        AgentBuildData agentBuildData = new AgentBuildData(character)
            .Equipment(character.Equipment)
            .TroopOrigin(new BasicBattleAgentOrigin(character))
            .EquipmentSeed(MissionLobbyComponent.GetRandomFaceSeedForCharacter(character))
            .Team(team)
            .VisualsIndex(0)
            .InitialPosition(in spawnFrame.origin)
            .InitialDirection(in initialDirection)
            .IsFemale(false)
            .ClothingColor1(teamCulture.ClothAlternativeColor)
            .ClothingColor2(teamCulture.ClothAlternativeColor2);

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
        agent.AIStateFlags = Agent.AIStateFlag.Alarmed;
        agent.SetTargetPosition(new Vec2(spawnFrame.origin.x, spawnFrame.origin.y)); // stops viscount from being bumped
        agent.WieldInitialWeapons();
        Debug.Print("Spawned viscount");
    }

    protected void SpawnAttackingBots(int currentWave, int currentRound)
    {
        Debug.Print($"Attempting to spawn attacking bots for Wave: {Wave} Round:{Round}");
        BasicCultureObject teamCulture = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
        Team team = Mission.AttackerTeam;
        int numberOfBots = 0;
        string botDivisionID = "crpg_dtv_";

        if (_dtvData is not null)
        {
            var waveValue = _dtvData.Descendants("Round")
                .Where(round => (int)round.Attribute("number") == currentRound)
                .Elements($"Wave{currentWave}EnemyCount")
                .FirstOrDefault()?.Value;

            Debug.Print($"Number of bots this wave: {waveValue}");
            if (waveValue != null)
            {
                numberOfBots = int.Parse(waveValue);
            }

            var divisionID = _dtvData.Descendants("Round")
                .Where(round => (int)round.Attribute("number") == currentRound)
                .Elements("ClassDivisionId")
                .FirstOrDefault()?.Value;

            Debug.Print($"Type of bot this wave: {divisionID}");
            if (divisionID != null)
            {
                botDivisionID = divisionID;
            }
        }

        Mission.Current.AllowAiTicking = false;
        int botsAlive = team.ActiveAgents.Count(a => a.IsAIControlled && a.IsHuman);

        for (int i = 0 + botsAlive; i < numberOfBots; i += 1)
        {
            MultiplayerClassDivisions.MPHeroClass botClass = MultiplayerClassDivisions
                .GetMPHeroClasses()
                .GetRandomElementWithPredicate<MultiplayerClassDivisions.MPHeroClass>(x => x.StringId.StartsWith(botDivisionID));
            BasicCharacterObject character = botClass.HeroCharacter;
            Debug.Print($"Attempting to spawn {character.Name}");

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
                .ClothingColor1(teamCulture.Color)
                .ClothingColor2(teamCulture.Color2);

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

    protected override bool IsRoundInProgress()
    {
        return _roundController.IsRoundInProgress;
    }

    protected override bool IsPlayerAllowedToSpawn(NetworkCommunicator networkPeer)
    {
        var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
        var missionPeer = networkPeer.GetComponent<MissionPeer>();
        if (crpgPeer?.User == null
            || missionPeer == null
            || missionPeer.HasSpawnedAgentVisuals)
        {
            return false;
        }

        var characterEquipment = CreateCharacterEquipment(crpgPeer.User.Character.EquippedItems);
        if (!DoesEquipmentContainWeapon(characterEquipment)) // Disallow spawning without weapons.
        {
            if (_notifiedPlayersAboutSpawnRestriction.Add(networkPeer.VirtualPlayer.Id))
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new CrpgNotificationId
                {
                    Type = CrpgNotificationType.Announcement,
                    TextId = "str_kick_reason",
                    TextVariation = "no_weapon",
                    SoundEvent = string.Empty,
                });
                GameNetwork.EndModuleEventAsServer();
            }

            return false;
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
                    Type = CrpgNotificationType.Notification,
                    Message = $"Cavalry will spawn in {_cavalrySpawnDelayTimer?.GetTimerDuration()} seconds!",
                    SoundEvent = string.Empty,
                });
                GameNetwork.EndModuleEventAsServer();
            }

            return false;
        }

        return true;
    }

    protected override void OnPeerSpawned(MissionPeer missionPeer)
    {
        base.OnPeerSpawned(missionPeer);
        missionPeer.SpawnCountThisRound += 1;
    }

    private MatrixFrame GetviscountSpawnFrame(Team team, GameEntity spawnPoint)
    {
        if (spawnPoint != null)
        {
            MatrixFrame globalFrame = spawnPoint.GetGlobalFrame();
            globalFrame.rotation.OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
            return globalFrame;
        }
        else
        {
            return SpawnComponent.GetSpawnFrame(team, false, true);
        }
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
            if (!networkPeer.IsSynchronized
                || missionPeer == null
                || missionPeer.Team == null
                || missionPeer.Team.Side == BattleSideEnum.None)
            {
                continue;
            }

            counter++;
        }

        return counter;
    }
}
