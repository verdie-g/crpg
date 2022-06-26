using Crpg.Module.Common;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Battle;

internal class CrpgBattleSpawningBehavior : SpawningBehaviorBase
{
    private readonly MultiplayerRoundController _roundController;
    private bool _botsSpawned;

    public CrpgBattleSpawningBehavior(MultiplayerRoundController roundController)
    {
        _roundController = roundController;
    }

    public override void Initialize(SpawnComponent spawnComponent)
    {
        base.Initialize(spawnComponent);
        _roundController.OnRoundStarted += RequestStartSpawnSession;
        _roundController.OnRoundEnding += RequestStopSpawnSession;
        _roundController.EnableEquipmentUpdate();
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

        base.OnTick(dt);
    }

    public override void RequestStartSpawnSession()
    {
        base.RequestStartSpawnSession();
        _botsSpawned = false;
    }

    public override bool AllowEarlyAgentVisualsDespawning(MissionPeer missionPeer)
    {
        if (!_roundController.IsRoundInProgress)
        {
            return false;
        }

        if (!missionPeer.HasSpawnTimerExpired && missionPeer.SpawnTimer.Check(Mission.Current.CurrentTime))
        {
            missionPeer.HasSpawnTimerExpired = true;
        }

        return missionPeer.HasSpawnTimerExpired;
    }

    protected override bool IsRoundInProgress()
    {
        return _roundController.IsRoundInProgress;
    }

    protected override void SpawnAgents()
    {
        if (!_botsSpawned)
        {
            SpawnBotAgents();
            _botsSpawned = true;
        }

        SpawnPeerAgents();
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

                bool hasMount = character.Equipment[EquipmentIndex.ArmorItemEndSlot].Item != null;
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
                    .ClothingColor2(team.Side == BattleSideEnum.Attacker ? teamCulture.Color2 : teamCulture.ClothAlternativeColor2);

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
            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            if (!networkPeer.IsSynchronized
                || missionPeer.ControlledAgent != null
                || missionPeer.HasSpawnedAgentVisuals
                || missionPeer.Team == null
                || missionPeer.Team == Mission.SpectatorTeam
                || !missionPeer.SpawnTimer.Check(Mission.CurrentTime)
                || crpgPeer == null
                || crpgPeer.User == null)
            {
                continue;
            }

            BasicCultureObject teamCulture = missionPeer.Team == Mission.AttackerTeam ? cultureTeam1 : cultureTeam2;
            // for now everyone will have the same hero class
            MultiplayerClassDivisions.MPHeroClass peerClass = MultiplayerClassDivisions.GetMPHeroClasses().Skip(1).First();
            BasicCharacterObject heroCharacter = peerClass.HeroCharacter;
            MPPerkObject.MPOnSpawnPerkHandler spawnPerkHandler = MPPerkObject.GetOnSpawnPerkHandler(missionPeer);
            Equipment equipment = heroCharacter.Equipment.Clone();
            var alternativeEquipments = spawnPerkHandler?.GetAlternativeEquipments(true);
            if (alternativeEquipments != null)
            {
                foreach ((EquipmentIndex, EquipmentElement) tuple in alternativeEquipments)
                {
                    equipment[tuple.Item1] = tuple.Item2;
                }
            }

            bool hasMount = equipment[EquipmentIndex.ArmorItemEndSlot].Item != null;
            MatrixFrame spawnFrame = missionPeer.GetAmountOfAgentVisualsForPeer() > 0
                ? missionPeer.GetAgentVisualForPeer(0).GetFrame()
                : SpawnComponent.GetSpawnFrame(missionPeer.Team, hasMount, true);
            Vec2 initialDirection = spawnFrame.rotation.f.AsVec2.Normalized();

            AgentBuildData agentBuildData = new AgentBuildData(heroCharacter)
                .MissionPeer(missionPeer)
                .Equipment(equipment)
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

            if (GameMode.ShouldSpawnVisualsForServer(networkPeer))
            {
                AgentVisualSpawnComponent.SpawnAgentVisualsForPeer(missionPeer, agentBuildData);
            }

            GameMode.HandleAgentVisualSpawning(networkPeer, agentBuildData, useCosmetics: false);
            // This line makes the agent spawn in battle instead of spawning in the class loadout thing.
            SpawnComponent.SetEarlyAgentVisualsDespawning(missionPeer);
        }
    }
}
