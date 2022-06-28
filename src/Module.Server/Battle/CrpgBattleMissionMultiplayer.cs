using Crpg.Module.Api;
using Crpg.Module.Api.Models;
using Crpg.Module.Common;
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
        Dictionary<int, CrpgPeer> crpgPeerByUserId = new();
        List<CrpgUserUpdate> userUpdates = new();
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            var missionPeer = networkPeer.GetComponent<MissionPeer>();
            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            if (missionPeer == null || crpgPeer == null || crpgPeer.User == null)
            {
                continue;
            }

            crpgPeerByUserId[crpgPeer.User.Id] = crpgPeer;

            CrpgUserUpdate userUpdate = new()
            {
                CharacterId = crpgPeer.User.Character.Id,
                Reward = new CrpgUserReward { Experience = 0, Gold = 0 },
                BrokenItems = Array.Empty<CrpgUserBrokenItem>(), // TODO
            };

            // TODO: check in which team the player has spawned instead of checking the current team.
            if (missionPeer.SpawnCountThisRound > 0 && missionPeer.Team != null && missionPeer.Team.Side != BattleSideEnum.None)
            {
                // TODO: only give reward to users that spawned during the round.
                if (RoundController.RoundWinner == missionPeer.Team.Side)
                {
                    userUpdate.Reward = new CrpgUserReward
                    {
                        Experience = 0,
                        Gold = 0,
                    };
                    crpgPeer.RewardMultiplier = Math.Min(5, crpgPeer.RewardMultiplier + 1);
                }
                else
                {
                    userUpdate.Reward = new CrpgUserReward
                    {
                        Experience = 0,
                        Gold = 0,
                    };
                    crpgPeer.RewardMultiplier = 1;
                }
            }

            userUpdates.Add(userUpdate);
        }

        // TODO: add retry mechanism (the endpoint need to be idempotent though).
        CrpgUsersUpdateResponse res;
        try
        {
            res = (await _crpgClient.UpdateUsersAsync(new CrpgGameUsersUpdateRequest { Updates = userUpdates })).Data!;
        }
        catch (Exception e)
        {
            Debug.PrintError($"Couldn't update users: {e.Message}", e.StackTrace);
            // TODO: send error to users.
            return;
        }

        foreach (var updateResult in res.UpdateResults)
        {
            if (!crpgPeerByUserId.TryGetValue(updateResult.User.Id, out var crpgPeer))
            {
                Debug.PrintWarning($"Unknown user with id '{updateResult.User.Id}'");
                continue;
            }

            crpgPeer.User = updateResult.User;
            // TODO: send reward info to user.
        }
    }
}
