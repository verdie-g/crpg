using Crpg.Module.Api;
using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using Crpg.Module.Helpers;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Battle;

internal class CrpgBattleMissionMultiplayer : MissionMultiplayerGameModeBase
{
    private readonly ICrpgClient _crpgClient;
    private readonly CrpgConstants _constants;
    private readonly Random _random = new();

    private Dictionary<PlayerId, CrpgCharacterStatistics> _lastRoundAllTotalStats = new();

    public override bool IsGameModeHidingAllAgentVisuals => true;
    public override bool IsGameModeUsingOpposingTeams => true;
    public override bool AllowCustomPlayerBanners() => false;
    public override bool UseRoundController() => true;

    public CrpgBattleMissionMultiplayer(ICrpgClient crpgClient, CrpgConstants constants)
    {
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
        RoundController.OnPostRoundEnded += OnPostRoundEnd;
        WarmupComponent.OnWarmupEnded += OnWarmupEnding;

        AddTeams();
    }

    public override void OnRemoveBehavior()
    {
        RoundController.OnPostRoundEnded -= OnPostRoundEnd;
        RoundController.OnPreRoundEnding -= OnPreRoundEnding;
        WarmupComponent.OnWarmupEnding -= OnWarmupEnding;
        base.OnRemoveBehavior();
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

    public override bool CheckForRoundEnd()
    {
        // MultiplayerRoundController already checks the round timer.
        // TODO: check spawn time ended.
        return Mission.AttackerTeam.ActiveAgents.Count == 0 || Mission.DefenderTeam.ActiveAgents.Count == 0;
    }

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        agent.UpdateSyncHealthToAllClients(true); // Why is that needed
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
    }

    protected override void HandleLateNewClientAfterSynchronized(NetworkCommunicator networkPeer)
    {
        Mission.Current.GetMissionBehavior<MultiplayerTeamSelectComponent>().AutoAssignTeam(networkPeer);
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

    private void OnWarmupEnding()
    {
        NotificationsComponent.WarmupEnding();
    }

    private void OnPreRoundEnding()
    {
        if (Mission.AttackerTeam.ActiveAgents.Count == 0)
        {
            RoundController.RoundEndReason = RoundEndReason.SideDepleted;
            RoundController.RoundWinner = BattleSideEnum.Defender;
        }
        else if (Mission.DefenderTeam.ActiveAgents.Count == 0)
        {
            RoundController.RoundEndReason = RoundEndReason.SideDepleted;
            RoundController.RoundWinner = BattleSideEnum.Attacker;
        }
        else
        {
            RoundController.RoundEndReason = RoundEndReason.RoundTimeEnded;
            RoundController.RoundWinner = BattleSideEnum.None;
        }

        CheerForRoundEnd(RoundController.RoundWinner);
        _ = UpdateCrpgUsersAsync();
    }

    private void OnPostRoundEnd()
    {
    }

    private void CheerForRoundEnd(BattleSideEnum winnerSide)
    {
        if (winnerSide == BattleSideEnum.None)
        {
            return;
        }

        AgentVictoryLogic agentVictoryLogic = Mission.GetMissionBehavior<AgentVictoryLogic>();
        agentVictoryLogic?.SetTimersOfVictoryReactionsOnBattleEnd(winnerSide);
    }

    private async Task UpdateCrpgUsersAsync()
    {
        int ticks = ComputeTicks();

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
                RepairCost = updateResult.BrokenItems.Sum(b => b.RepairCost),
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
