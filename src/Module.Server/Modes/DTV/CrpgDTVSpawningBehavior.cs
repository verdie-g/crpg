﻿using System.Xml;
using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Modes.DTV;

internal class CrpgDTVSpawningBehavior : CrpgSpawningBehaviorBase
{
    public bool BotsSpawned { get; set; }

    private const float TotalSpawnDuration = 15f;

    private readonly XmlDocument? _dtvData;
    private readonly XmlNodeList _rounds;
    private readonly MultiplayerRoundController _roundController;
    private readonly HashSet<PlayerId> _notifiedPlayersAboutSpawnRestriction;
    private MissionTimer? _spawnTimer;
    private MissionTimer? _cavalrySpawnDelayTimer;
    private bool _virginSpawned;

    public CrpgDTVSpawningBehavior(CrpgConstants constants, MultiplayerRoundController roundController)
        : base(constants)
    {
        _roundController = roundController;
        _notifiedPlayersAboutSpawnRestriction = new HashSet<PlayerId>();
        _dtvData = new XmlDocument();
        _dtvData.Load(ModuleHelper.GetXmlPath("Crpg", "dtv_data"));
        _rounds = _dtvData.SelectNodes("//Round");
    }

    public override void Initialize(SpawnComponent spawnComponent)
    {
        base.Initialize(spawnComponent);
        _roundController.OnPreparationEnded += RequestStartSpawnSession;
        _roundController.OnRoundEnding += RequestStopSpawnSession;
    }

    public override void Clear()
    {
        base.Clear();
        _roundController.OnPreparationEnded -= RequestStartSpawnSession;
        _roundController.OnRoundEnding -= RequestStopSpawnSession;
    }

    public override void OnTick(float dt)
    {
        if (!BotsSpawned && _virginSpawned)
        {
            Debug.Print("Attempting to spawn attacking bots!");
            SpawnAttackingBots(1, 2);
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

        if (!_virginSpawned)
        {
            SpawnVirgin();
            _virginSpawned = true;
        }
        SpawnAgents();
    }

    public override void RequestStartSpawnSession()
    {
        base.RequestStartSpawnSession();
        _virginSpawned = false;
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

    protected void SpawnVirgin()
    {
        Debug.Print("Attempting to spawn Virgin");
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

            BasicCultureObject teamCulture;
            int numberOfBots;
            if (team.Side != BattleSideEnum.Attacker)
            {
                teamCulture = cultureTeam2;
                numberOfBots = MultiplayerOptions.OptionType.NumberOfBotsTeam2.GetIntValue();
            }
            else // The virgin has to be a defender
            {
                continue;
            }

            int numberOfPlayers = GameNetwork.NetworkPeers.Count(p => p.IsSynchronized && p.GetComponent<MissionPeer>()?.Team == team);
            int botsAlive = team.ActiveAgents.Count(a => a.IsAIControlled && a.IsHuman);

            for (int i = 0 + botsAlive; i < numberOfBots; i += 1)
            {
                MultiplayerClassDivisions.MPHeroClass botClass = MultiplayerClassDivisions
                    .GetMPHeroClasses()
                    .GetRandomElementWithPredicate<MultiplayerClassDivisions.MPHeroClass>(x => x.StringId.StartsWith("crpg_dtv_virgin"));
                BasicCharacterObject character = botClass.HeroCharacter;

                MatrixFrame spawnFrame = SpawnComponent.GetSpawnFrame(team, false, true);
                Vec2 initialDirection = spawnFrame.rotation.f.AsVec2.Normalized();

                AgentBuildData agentBuildData = new AgentBuildData(character)
                    .Equipment(character.Equipment)
                    .TroopOrigin(new BasicBattleAgentOrigin(character))
                    .EquipmentSeed(MissionLobbyComponent.GetRandomFaceSeedForCharacter(character))
                    .Team(team)
                    .VisualsIndex(0)
                    .InitialPosition(in spawnFrame.origin)
                    .InitialDirection(in initialDirection)
                    .IsFemale(true)
                    .ClothingColor1(team.Side == BattleSideEnum.Attacker ? teamCulture.Color : teamCulture.ClothAlternativeColor)
                    .ClothingColor2(team.Side == BattleSideEnum.Attacker ? teamCulture.Color2 : teamCulture.ClothAlternativeColor2);

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
                agent.AIStateFlags = Agent.AIStateFlag.Paused; // TODO: prevent bumping / pushing
                agent.WieldInitialWeapons();
                Debug.Print("Spawned Virgin");
            }
        }
    }

    protected void SpawnAttackingBots(int wave, int round)
    {
        BasicCultureObject cultureTeam1 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
        BasicCultureObject cultureTeam2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());
        int botsTeam1 = MultiplayerOptions.OptionType.NumberOfBotsTeam1.GetIntValue();
        int botsTeam2 = MultiplayerOptions.OptionType.NumberOfBotsTeam2.GetIntValue();
        int numberOfBots = 0;
        foreach (XmlNode? roundNode in _rounds)
        {
            if (roundNode != null && int.Parse(roundNode.Attributes["number"].Value) == round)
            {
                Debug.Print($"Found XML NODE WITH NUMBER VALUE OF: {roundNode.Attributes["number"].Value}");
/*                Debug.Print($"Found XML NODE WITH wave1 count OF: {roundNode["wave1"].InnerText}");
                numberOfBots = int.Parse(roundNode["wave1"].InnerText);*/
            }
        }

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

            BasicCultureObject teamCulture;
            
            if (team.Side == BattleSideEnum.Attacker)
            {
                teamCulture = cultureTeam1;
                //numberOfBots = MultiplayerOptions.OptionType.NumberOfBotsTeam1.GetIntValue();
            }
            else // Bots are attackers
            {
                continue;
            }

            int botsAlive = team.ActiveAgents.Count(a => a.IsAIControlled && a.IsHuman);

            for (int i = 0 + botsAlive; i < numberOfBots; i += 1)
            {
                MultiplayerClassDivisions.MPHeroClass botClass = MultiplayerClassDivisions
                    .GetMPHeroClasses()
                    .GetRandomElementWithPredicate<MultiplayerClassDivisions.MPHeroClass>(x => x.StringId.StartsWith("crpg_dtv_virgin"));
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
                    .ClothingColor1(team.Side == BattleSideEnum.Attacker ? teamCulture.Color : teamCulture.ClothAlternativeColor)
                    .ClothingColor2(team.Side == BattleSideEnum.Attacker ? teamCulture.Color2 : teamCulture.ClothAlternativeColor2);

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
