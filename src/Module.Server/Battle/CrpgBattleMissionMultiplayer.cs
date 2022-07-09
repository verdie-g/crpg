using Crpg.Module.Api;
using Crpg.Module.Api.Models;
using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Battle;

internal class CrpgBattleMissionMultiplayer : MissionMultiplayerGameModeBase
{
    private readonly ICrpgClient _crpgClient;
    private CrpgBattleMissionMultiplayerClient? _client;

    public override bool IsGameModeHidingAllAgentVisuals => true;
    public override bool IsGameModeUsingOpposingTeams => true;
    public override bool AllowCustomPlayerBanners() => false;
    public override bool UseRoundController() => true;

    public CrpgBattleMissionMultiplayer(ICrpgClient crpgClient)
    {
        _crpgClient = crpgClient;
    }

    public override MissionLobbyComponent.MultiplayerGameType GetMissionType()
    {
        return MissionLobbyComponent.MultiplayerGameType.Battle;
    }

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        _client = Mission.Current.GetMissionBehavior<CrpgBattleMissionMultiplayerClient>();
    }

    public override void AfterStart()
    {
        base.AfterStart();
        RoundController.OnPreRoundEnding += OnPreRoundEnding;
        RoundController.OnPostRoundEnded += OnPostRoundEnd;

        AddTeams();
    }

    public override void OnRemoveBehavior()
    {
        RoundController.OnPostRoundEnded -= OnPostRoundEnd;
        RoundController.OnPreRoundEnding -= OnPreRoundEnding;
        base.OnRemoveBehavior();
    }

    public override void OnPeerChangedTeam(NetworkCommunicator peer, Team? oldTeam, Team newTeam)
    {
        // TODO: reward multiplier to change maybe
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

    private void AddTeams()
    {
        BasicCultureObject cultureTeam1 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
        Banner bannerTeam1 = new(cultureTeam1.BannerKey, cultureTeam1.BackgroundColor1, cultureTeam1.ForegroundColor1);
        Mission.Teams.Add(BattleSideEnum.Attacker, cultureTeam1.BackgroundColor1, cultureTeam1.ForegroundColor1, bannerTeam1, false, true);
        BasicCultureObject cultureTeam2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());
        Banner bannerTeam2 = new(cultureTeam2.BannerKey, cultureTeam2.BackgroundColor2, cultureTeam2.ForegroundColor2);
        Mission.Teams.Add(BattleSideEnum.Defender, cultureTeam2.BackgroundColor2, cultureTeam2.ForegroundColor2, bannerTeam2, false, true);
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
                BrokenItems = Array.Empty<CrpgUserBrokenItem>(), // TODO
            };

            if (crpgRepresentative.SpawnTeamThisRound != null)
            {
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

            userUpdates.Add(userUpdate);
        }

        if (userUpdates.Count == 0)
        {
            return;
        }

        // TODO: add retry mechanism (the endpoint need to be idempotent though).
        CrpgUsersUpdateResponse res;
        try
        {
            res = (await _crpgClient.UpdateUsersAsync(new CrpgGameUsersUpdateRequest { Updates = userUpdates })).Data!;
        }
        catch (Exception e)
        {
            Debug.Print("Couldn't update users: " + e);
            // TODO: send error to users.
            return;
        }

        foreach (var updateResult in res.UpdateResults)
        {
            if (!crpgRepresentativeByUserId.TryGetValue(updateResult.User.Id, out var crpgRepresentative))
            {
                Debug.Print($"Unknown user with id '{updateResult.User.Id}'");
                continue;
            }

            crpgRepresentative.User = updateResult.User;

            if (updateResult.EffectiveReward.Experience != 0 && updateResult.EffectiveReward.Gold != 0)
            {
                GameNetwork.BeginModuleEventAsServer(crpgRepresentative.GetNetworkPeer());
                GameNetwork.WriteMessage(new RewardCrpgUser { Reward = updateResult.EffectiveReward });
                GameNetwork.EndModuleEventAsServer();
            }
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
}
