using Crpg.Module.Api;
using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using Crpg.Module.Helpers;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.ObjectSystem;
using TaleWorlds.PlayerServices;
using Timer = TaleWorlds.Core.Timer;

namespace Crpg.Module.Battle;

internal class CrpgBattleMissionMultiplayer : MissionMultiplayerGameModeBase
{
    private const uint FlagNeutralColor1 = 4284111450;
    private const uint FlagNeutralColor2 = uint.MaxValue;
    private const float FlagAttackRange = 6f;
    private const float FlagAttackRangeSquared = FlagAttackRange * FlagAttackRange;
    private const float MoraleGainOnTick = 0.002f;
    private const float MoraleGainMultiplierLastFlag = 1f;

    private readonly CrpgBattleMissionMultiplayerClient _battleClient;
    private readonly ICrpgClient _crpgClient;
    private readonly CrpgConstants _constants;
    private readonly Random _random = new();

    /// <summary>A number between -1.0 and 1.0. Less than 0 means the defenders are winning. Greater than 0 for attackers.</summary>
    private float _morale;
    private FlagCapturePoint[] _flags = Array.Empty<FlagCapturePoint>();
    private Team?[] _flagOwners = Array.Empty<Team>();
    private int[,] _agentCountsAroundFlags = new int[0, 0];

    /// <summary>True if captures points were removed and only one remains.</summary>
    private bool _wereFlagsRemoved;
    private Timer? _checkFlagRemovalTimer;
    private Dictionary<PlayerId, CrpgCharacterStatistics> _lastRoundAllTotalStats = new();

    public override bool IsGameModeHidingAllAgentVisuals => true;
    public override bool IsGameModeUsingOpposingTeams => true;
    public override bool AllowCustomPlayerBanners() => false;
    public override bool UseRoundController() => true;

    public CrpgBattleMissionMultiplayer(
        CrpgBattleMissionMultiplayerClient battleClient,
        ICrpgClient crpgClient,
        CrpgConstants constants)
    {
        _battleClient = battleClient;
        _crpgClient = crpgClient;
        _constants = constants;
    }

    public override MissionLobbyComponent.MultiplayerGameType GetMissionType()
    {
        return MissionLobbyComponent.MultiplayerGameType.Battle;
    }

    public override void AfterStart()
    {
        base.AfterStart();
        RoundController.OnPreRoundEnding += OnPreRoundEnding;

        AddTeams();
    }

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();

        ResetFlags();
        _morale = 0f;
        // TODO: SetTeamColorsWithAllSynched
    }

    public override void OnRemoveBehavior()
    {
        RoundController.OnPreRoundEnding -= OnPreRoundEnding;
        base.OnRemoveBehavior();
    }

    public override void OnClearScene()
    {
        ResetFlags();
        _morale = 0.0f;
        _checkFlagRemovalTimer = null;
        _wereFlagsRemoved = false;
    }

    public override bool CheckForWarmupEnd()
    {
        int playersInTeam = 0;
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            MissionPeer component = networkPeer.GetComponent<MissionPeer>();
            if (networkPeer.IsSynchronized && component?.Team != null && component.Team.Side != BattleSideEnum.None)
            {
                playersInTeam += 1;
            }
        }

        return playersInTeam >= MultiplayerOptions.OptionType.MaxNumberOfPlayers.GetIntValue();
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);
        if (MissionLobbyComponent.CurrentMultiplayerState != MissionLobbyComponent.MultiplayerGameState.Playing
            || !RoundController.IsRoundInProgress
            || !CanGameModeSystemsTickThisFrame
            || _flags.Length == 0) // Protection against scene with no flags.
        {
            return;
        }

        CheckRemovalOfFlags();
        CheckMorales();
        TickFlags();
    }

    public override bool CheckForRoundEnd()
    {
        if (!CanGameModeSystemsTickThisFrame)
        {
            return false;
        }

        if (_flags.Length != 0 && Math.Abs(_morale) >= 1.0)
        {
            if (!_wereFlagsRemoved)
            {
                return true;
            }

            var lastFlag = _flags.First(flag => !flag.IsDeactivated);
            var lastFlagOwner = GetFlagOwner(lastFlag);
            if (lastFlagOwner == null)
            {
                return true;
            }

            var winningSide = _morale > 0.0f
                ? BattleSideEnum.Attacker
                : BattleSideEnum.Defender;
            return lastFlagOwner.Side == winningSide
                   && lastFlag.IsFullyRaised
                   && GetNumberOfAttackersAroundFlag(lastFlag) == 0;
        }

        bool defenderTeamAlive = Mission.DefenderTeam.ActiveAgents.Count > 0;
        bool attackerTeamAlive = Mission.AttackerTeam.ActiveAgents.Count > 0;
        CrpgBattleSpawningBehavior spawningBehavior = (CrpgBattleSpawningBehavior)SpawnComponent.SpawningBehavior;
        return (!defenderTeamAlive || !attackerTeamAlive) && spawningBehavior.SpawnDelayEnded();
    }

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        agent.UpdateSyncHealthToAllClients(true); // Why is that needed
    }

    public override bool CheckIfOvertime()
    {
        if (!_wereFlagsRemoved)
        {
            return false;
        }

        var lastFlag = _flags.First(flag => !flag.IsDeactivated);
        var owner = GetFlagOwner(lastFlag);
        if (owner == null)
        {
            return false;
        }

        int moraleOwnerSide = owner.Side == BattleSideEnum.Defender ? -1 : 1;
        return moraleOwnerSide * _morale < 0.0 || GetNumberOfAttackersAroundFlag(lastFlag) > 0;
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
    }

    protected override void HandleNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
    {
        GameNetwork.BeginModuleEventAsServer(networkPeer);
        GameNetwork.WriteMessage(new FlagDominationMoraleChangeMessage(_morale));
        GameNetwork.EndModuleEventAsServer();
    }

    private void AddTeams()
    {
        BasicCultureObject cultureTeam1 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
        Banner bannerTeam1 = new(cultureTeam1.BannerKey, cultureTeam1.BackgroundColor1, cultureTeam1.ForegroundColor1);
        Mission.Teams.Add(BattleSideEnum.Attacker, cultureTeam1.BackgroundColor1, cultureTeam1.ForegroundColor1, bannerTeam1, false, true);
        BasicCultureObject cultureTeam2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());
        Banner bannerTeam2 = new(cultureTeam2.BannerKey, cultureTeam2.BackgroundColor2, cultureTeam2.ForegroundColor2);
        Mission.Teams.Add(BattleSideEnum.Defender, cultureTeam2.BackgroundColor2, cultureTeam2.ForegroundColor2, bannerTeam2, false, true);
    }

    private void ResetFlags()
    {
        _flags = Mission.Current.MissionObjects.FindAllWithType<FlagCapturePoint>().ToArray();
        _flagOwners = new Team[_flags.Length];
        _agentCountsAroundFlags = new int[_flags.Length, (int)BattleSideEnum.NumSides];
        foreach (var flag in _flags)
        {
            flag.ResetPointAsServer(FlagNeutralColor1, FlagNeutralColor2);
        }
    }

    private void CheckMorales()
    {
        float moraleGain = GetMoraleGain();
        if (moraleGain == 0)
        {
            return;
        }

        _morale += moraleGain;
        _morale = MBMath.ClampFloat(_morale, -1f, 1f);

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new FlagDominationMoraleChangeMessage(_morale));
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);

        _battleClient.ChangeMorale(_morale);
    }

    private float GetMoraleGain()
    {
        FlagCapturePoint[] capturedFlags = _flags
            .Where(flag => !flag.IsDeactivated && GetFlagOwner(flag) != null && flag.IsFullyRaised)
            .ToArray();
        int teamFlagsDelta = capturedFlags.Count(flag => GetFlagOwner(flag)!.Side == BattleSideEnum.Attacker)
                             - capturedFlags.Count(flag => GetFlagOwner(flag)!.Side == BattleSideEnum.Defender);
        if (teamFlagsDelta == 0)
        {
            return 0f;
        }

        float moraleMultiplier = MoraleGainOnTick * Math.Abs(teamFlagsDelta);
        float moraleGain = teamFlagsDelta <= 0
            ? MBMath.ClampFloat(-1 - _morale, -2f, -1f) * moraleMultiplier
            : MBMath.ClampFloat(1 - _morale, 1f, 2f) * moraleMultiplier;
        if (_wereFlagsRemoved) // For the last flag, the morale is moving faster.
        {
            moraleGain *= MoraleGainMultiplierLastFlag;
        }

        return moraleGain;
    }

    private void TickFlags()
    {
        foreach (var flag in _flags)
        {
            if (flag.IsDeactivated)
            {
                continue;
            }

            int[] agentCountsAroundFlag = new int[(int)BattleSideEnum.NumSides];

            Agent? closestAgentToFlag = null;
            float closestAgentDistanceToFlagSquared = 16f; // Where does this number come from?
            foreach (Agent agent in Mission.Current.GetAgentsInRange(flag.Position.AsVec2, FlagAttackRange))
            {
                if (!agent.IsActive() || !agent.IsHuman || agent.HasMount)
                {
                    continue;
                }

                agentCountsAroundFlag[(int)agent.Team.Side] += 1;
                float distanceToFlagSquared = agent.Position.DistanceSquared(flag.Position);
                if (distanceToFlagSquared <= closestAgentDistanceToFlagSquared)
                {
                    closestAgentToFlag = agent;
                    closestAgentDistanceToFlagSquared = distanceToFlagSquared;
                }
            }

            bool agentCountsAroundFlagChanged =
                agentCountsAroundFlag[(int)BattleSideEnum.Defender] != _agentCountsAroundFlags[flag.FlagIndex, (int)BattleSideEnum.Defender]
                || agentCountsAroundFlag[(int)BattleSideEnum.Attacker] != _agentCountsAroundFlags[flag.FlagIndex, (int)BattleSideEnum.Attacker];
            _agentCountsAroundFlags[flag.FlagIndex, (int)BattleSideEnum.Defender] = agentCountsAroundFlag[(int)BattleSideEnum.Defender];
            _agentCountsAroundFlags[flag.FlagIndex, (int)BattleSideEnum.Attacker] = agentCountsAroundFlag[(int)BattleSideEnum.Attacker];
            float speedMultiplier = 1f;
            if (closestAgentToFlag != null)
            {
                BattleSideEnum side = closestAgentToFlag.Team.Side;
                BattleSideEnum oppositeSide = side.GetOppositeSide();
                if (agentCountsAroundFlag[(int)oppositeSide] != 0)
                {
                    int val1 = Math.Min(agentCountsAroundFlag[(int)side], 200);
                    int val2 = Math.Min(agentCountsAroundFlag[(int)oppositeSide], 200);
                    speedMultiplier = Math.Min(1f, (MathF.Log10(val1) + 1.0f) / (2.0f * (MathF.Log10(val2) + 1.0f)) - 0.09f);
                }
            }

            var flagOwner = GetFlagOwner(flag);
            if (flagOwner == null)
            {
                if (!flag.IsContested && closestAgentToFlag != null)
                {
                    flag.SetMoveFlag(CaptureTheFlagFlagDirection.Down, speedMultiplier);
                }
                else if (closestAgentToFlag == null & flag.IsContested)
                {
                    flag.SetMoveFlag(CaptureTheFlagFlagDirection.Up, speedMultiplier);
                }
                else if (agentCountsAroundFlagChanged)
                {
                    flag.ChangeMovementSpeed(speedMultiplier);
                }
            }
            else if (closestAgentToFlag != null)
            {
                if (closestAgentToFlag.Team != flagOwner && !flag.IsContested)
                {
                    flag.SetMoveFlag(CaptureTheFlagFlagDirection.Down, speedMultiplier);
                }
                else if (closestAgentToFlag.Team == flagOwner & flag.IsContested)
                {
                    flag.SetMoveFlag(CaptureTheFlagFlagDirection.Up, speedMultiplier);
                }
                else if (agentCountsAroundFlagChanged)
                {
                    flag.ChangeMovementSpeed(speedMultiplier);
                }
            }
            else if (flag.IsContested)
            {
                flag.SetMoveFlag(CaptureTheFlagFlagDirection.Up, speedMultiplier);
            }
            else if (agentCountsAroundFlagChanged)
            {
                flag.ChangeMovementSpeed(speedMultiplier);
            }

            flag.OnAfterTick(closestAgentToFlag != null, out bool flagOwnerChanged);
            if (flagOwnerChanged)
            {
                Team team = closestAgentToFlag!.Team!;
                flag.SetTeamColorsWithAllSynched(team.Color, team.Color2);
                _flagOwners[flag.FlagIndex] = team;
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new FlagDominationCapturePointMessage(flag.FlagIndex, team));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                _battleClient.CaptureFlag(flag, team);
                NotificationsComponent.FlagXCapturedByTeamX(flag, closestAgentToFlag.Team);
                MPPerkObject.RaiseEventForAllPeers(MPPerkCondition.PerkEventFlags.FlagCapture);
            }
        }
    }

    private void CheckRemovalOfFlags()
    {
        if (_wereFlagsRemoved)
        {
            return;
        }

        _checkFlagRemovalTimer ??= new Timer(Mission.CurrentTime, CrpgBattleMissionMultiplayerClient.FlagsRemovalTime);
        if (!_checkFlagRemovalTimer.Check(Mission.CurrentTime))
        {
            return;
        }

        var flagsToRemove = _flags.ToArray();
        flagsToRemove.Shuffle();
        var flagIndexesToRemove = new HashSet<int>(flagsToRemove
            .Take(flagsToRemove.Length - 1)
            .Select(RemoveFlag));

        var remainingFlag = _flags.First(flag => !flagIndexesToRemove.Contains(flag.FlagIndex));
        NotificationsComponent.FlagXRemaining(remainingFlag);

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new FlagDominationMoraleChangeMessage(_morale));
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new FlagDominationFlagsRemovedMessage());
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);

        _wereFlagsRemoved = true;
        _battleClient.ChangeNumberOfFlags();
        Debug.Print("Flags were removed");
    }

    private Team? GetFlagOwner(FlagCapturePoint flag)
    {
        return _flagOwners[flag.FlagIndex];
    }

    private int GetNumberOfAttackersAroundFlag(FlagCapturePoint flag)
    {
        var flagOwner = GetFlagOwner(flag);
        if (flagOwner == null)
        {
            return 0;
        }

        return Mission.Current.GetAgentsInRange(flag.Position.AsVec2, FlagAttackRange)
            .Count<Agent>(a => a.IsHuman
                               && a.IsActive()
                               && a.Position.DistanceSquared(flag.Position) <= FlagAttackRangeSquared
                               && a.Team.Side != flagOwner.Side);
    }

    private int RemoveFlag(FlagCapturePoint flag)
    {
        flag.RemovePointAsServer();
        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new FlagDominationCapturePointMessage(flag.FlagIndex, null));
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        return flag.FlagIndex;
    }

    private void OnPreRoundEnding()
    {
        foreach (var flag in _flags)
        {
            if (!flag.IsDeactivated)
            {
                flag.SetMoveNone();
            }
        }

        bool timedOut = RoundController.RemainingRoundTime <= 0.0 && !CheckIfOvertime();

        BattleSideEnum winnerSide = BattleSideEnum.None;
        for (BattleSideEnum side = BattleSideEnum.Defender; side < BattleSideEnum.NumSides; side += 1)
        {
            int moraleSide = side == BattleSideEnum.Defender ? -1 : 1;
            if ((timedOut && moraleSide * _morale > 0.0f) || (!timedOut && moraleSide * _morale >= 1.0f))
            {
                winnerSide = side;
                break;
            }
        }

        CaptureTheFlagCaptureResultEnum roundResult;
        if (winnerSide != BattleSideEnum.None)
        {
            roundResult = winnerSide == BattleSideEnum.Defender
                ? CaptureTheFlagCaptureResultEnum.DefendersWin
                : CaptureTheFlagCaptureResultEnum.AttackersWin;
            RoundController.RoundWinner = winnerSide;
            RoundController.RoundEndReason = timedOut ? RoundEndReason.RoundTimeEnded : RoundEndReason.GameModeSpecificEnded;
        }
        else
        {
            bool defenderTeamAlive = Mission.DefenderTeam.ActiveAgents.Count > 0;
            bool attackerTeamAlive = Mission.AttackerTeam.ActiveAgents.Count > 0;
            if (defenderTeamAlive && attackerTeamAlive)
            {
                if (_morale > 0.0f)
                {
                    roundResult = CaptureTheFlagCaptureResultEnum.AttackersWin;
                    RoundController.RoundWinner = BattleSideEnum.Attacker;
                }
                else if (_morale < 0.0)
                {
                    roundResult = CaptureTheFlagCaptureResultEnum.DefendersWin;
                    RoundController.RoundWinner = BattleSideEnum.Defender;
                }
                else
                {
                    roundResult = CaptureTheFlagCaptureResultEnum.Draw;
                    RoundController.RoundWinner = BattleSideEnum.None;
                }

                RoundController.RoundEndReason = RoundEndReason.RoundTimeEnded;
            }
            else if (defenderTeamAlive)
            {
                roundResult = CaptureTheFlagCaptureResultEnum.DefendersWin;
                RoundController.RoundWinner = BattleSideEnum.Defender;
                RoundController.RoundEndReason = RoundEndReason.SideDepleted;
            }
            else if (attackerTeamAlive)
            {
                roundResult = CaptureTheFlagCaptureResultEnum.AttackersWin;
                RoundController.RoundWinner = BattleSideEnum.Attacker;
                RoundController.RoundEndReason = RoundEndReason.SideDepleted;
            }
            else // Everyone ded
            {
                roundResult = CaptureTheFlagCaptureResultEnum.Draw;
                RoundController.RoundWinner = BattleSideEnum.None;
                RoundController.RoundEndReason = RoundEndReason.SideDepleted;
            }
        }

        CheerForRoundEnd(roundResult);
        _ = UpdateCrpgUsersAsync();
    }

    private void CheerForRoundEnd(CaptureTheFlagCaptureResultEnum roundResult)
    {
        AgentVictoryLogic missionBehavior = Mission.GetMissionBehavior<AgentVictoryLogic>();
        if (roundResult == CaptureTheFlagCaptureResultEnum.AttackersWin)
        {
            missionBehavior.SetTimersOfVictoryReactionsOnBattleEnd(BattleSideEnum.Attacker);
        }
        else if (roundResult == CaptureTheFlagCaptureResultEnum.DefendersWin)
        {
            missionBehavior.SetTimersOfVictoryReactionsOnBattleEnd(BattleSideEnum.Defender);
        }
    }

    private async Task UpdateCrpgUsersAsync()
    {
        int ticks = ComputeTicks();
        Debug.Print($"Ticks for round with {GameNetwork.NetworkPeers.Count()}: {ticks}");

        Dictionary<int, CrpgRepresentative> crpgRepresentativeByUserId = new();
        var newRoundAllTotalStats = new Dictionary<PlayerId, CrpgCharacterStatistics>();
        List<CrpgUserUpdate> userUpdates = new();
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            var crpgRepresentative = networkPeer.GetComponent<CrpgRepresentative>();
            if (crpgRepresentative?.User == null)
            {
                continue;
            }

            crpgRepresentativeByUserId[crpgRepresentative.User.Id] = crpgRepresentative;

            CrpgUserUpdate userUpdate = new()
            {
                CharacterId = crpgRepresentative.User.Character.Id,
                Reward = new CrpgUserReward { Experience = 0, Gold = 0 },
                Statistics = new CrpgCharacterStatistics { Kills = 0, Deaths = 0, Assists = 0, PlayTime = TimeSpan.Zero },
                BrokenItems = BreakItems(crpgRepresentative),
            };

            SetReward(userUpdate, crpgRepresentative, ticks);
            SetStatistics(userUpdate, networkPeer, newRoundAllTotalStats);

            userUpdates.Add(userUpdate);
        }

        if (userUpdates.Count == 0)
        {
            return;
        }

        // Save last round stats to be able to make the difference next round.
        _lastRoundAllTotalStats = newRoundAllTotalStats;

        // TODO: add retry mechanism (the endpoint need to be idempotent though).
        CrpgUsersUpdateResponse res;
        try
        {
            res = (await _crpgClient.UpdateUsersAsync(new CrpgGameUsersUpdateRequest { Updates = userUpdates })).Data!;
            SendRewardToPeers(res.UpdateResults, crpgRepresentativeByUserId);
        }
        catch (Exception e)
        {
            Debug.Print("Couldn't update users: " + e);
            SendErrorToPeers(crpgRepresentativeByUserId);
        }
    }

    private int ComputeTicks()
    {
        float roundDuration = TimerComponent.GetCurrentTimerStartTime().ElapsedSeconds;
        if (roundDuration < 30)
        {
            return 1;
        }

        return 1 + (int)roundDuration / 60;
    }

    private void SetReward(CrpgUserUpdate userUpdate, CrpgRepresentative crpgRepresentative, int ticks)
    {
        if (crpgRepresentative.SpawnTeamThisRound == null)
        {
            return;
        }

        int totalRewardMultiplier = crpgRepresentative.RewardMultiplier * ticks;
        userUpdate.Reward = new CrpgUserReward
        {
            Experience = totalRewardMultiplier * 1000,
            Gold = totalRewardMultiplier * 50,
        };

        crpgRepresentative.RewardMultiplier = RoundController.RoundWinner == crpgRepresentative.SpawnTeamThisRound.Side
            ? Math.Min(5, crpgRepresentative.RewardMultiplier + 1)
            : 1;
    }

    private IList<CrpgUserBrokenItem> BreakItems(CrpgRepresentative crpgRepresentative)
    {
        if (crpgRepresentative.SpawnTeamThisRound == null)
        {
            return Array.Empty<CrpgUserBrokenItem>();
        }

        List<CrpgUserBrokenItem> brokenItems = new();
        foreach (var equippedItem in crpgRepresentative.User!.Character.EquippedItems)
        {
            var mbItem = Game.Current.ObjectManager.GetObject<ItemObject>(equippedItem.UserItem.BaseItemId);
            if (_random.NextDouble() >= _constants.ItemBreakChance)
            {
                continue;
            }

            int repairCost = (int)MathHelper.ApplyPolynomialFunction(mbItem.Value, _constants.ItemRepairCostCoefs);
            brokenItems.Add(new CrpgUserBrokenItem
            {
                UserItemId = equippedItem.UserItem.Id,
                RepairCost = repairCost,
            });
        }

        return brokenItems;
    }

    private void SetStatistics(CrpgUserUpdate userUpdate, NetworkCommunicator networkPeer,
        Dictionary<PlayerId, CrpgCharacterStatistics> newRoundAllTotalStats)
    {
        var missionPeer = networkPeer.GetComponent<MissionPeer>();
        var newRoundTotalStats = new CrpgCharacterStatistics
        {
            Kills = missionPeer.KillCount,
            Deaths = missionPeer.DeathCount,
            Assists = missionPeer.AssistCount,
            PlayTime = DateTime.Now - missionPeer.JoinTime,
        };

        if (_lastRoundAllTotalStats.TryGetValue(networkPeer.VirtualPlayer.Id, out var lastRoundTotalStats))
        {
            userUpdate.Statistics.Kills = newRoundTotalStats.Kills - lastRoundTotalStats.Kills;
            userUpdate.Statistics.Deaths = newRoundTotalStats.Deaths - lastRoundTotalStats.Deaths;
            userUpdate.Statistics.Assists = newRoundTotalStats.Assists - lastRoundTotalStats.Assists;
            userUpdate.Statistics.PlayTime = newRoundTotalStats.PlayTime - lastRoundTotalStats.PlayTime;
        }
        else
        {
            userUpdate.Statistics.Kills = newRoundTotalStats.Kills;
            userUpdate.Statistics.Deaths = newRoundTotalStats.Deaths;
            userUpdate.Statistics.Assists = newRoundTotalStats.Assists;
            userUpdate.Statistics.PlayTime = newRoundTotalStats.PlayTime;
        }

        newRoundAllTotalStats[networkPeer.VirtualPlayer.Id] = newRoundTotalStats;
    }

    private void SendRewardToPeers(IList<UpdateCrpgUserResult> updateResults,
        Dictionary<int, CrpgRepresentative> crpgRepresentativeByUserId)
    {
        foreach (var updateResult in updateResults)
        {
            if (!crpgRepresentativeByUserId.TryGetValue(updateResult.User.Id, out var crpgRepresentative))
            {
                Debug.Print($"Unknown user with id '{updateResult.User.Id}'");
                continue;
            }

            crpgRepresentative.User = updateResult.User;

            GameNetwork.BeginModuleEventAsServer(crpgRepresentative.GetNetworkPeer());
            GameNetwork.WriteMessage(new CrpgRewardUser
            {
                Reward = updateResult.EffectiveReward,
                RepairCost = updateResult.RepairedItems.Sum(r => r.RepairCost),
                SoldItemIds = updateResult.RepairedItems.Where(r => r.Sold).Select(r => r.ItemId).ToList(),
            });
            GameNetwork.EndModuleEventAsServer();
        }
    }

    private void SendErrorToPeers(Dictionary<int, CrpgRepresentative> crpgRepresentativeByUserId)
    {
        foreach (var crpgRepresentative in crpgRepresentativeByUserId.Values)
        {
            GameNetwork.BeginModuleEventAsServer(crpgRepresentative.GetNetworkPeer());
            GameNetwork.WriteMessage(new CrpgRewardError());
            GameNetwork.EndModuleEventAsServer();
        }
    }
}
