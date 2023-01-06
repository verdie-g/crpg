using System.Numerics;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Helpers;
using TaleWorlds.Library;

namespace Crpg.Module.Balancing;

internal class MatchBalancer
{
    public const float PowerParameter = 1f;
    public const int MaximumNumberOfSwaps = 20; // upperbound to limit number of swaps. Numberofswaps is often<3

    public GameMatch NaiveCaptainBalancing(GameMatch gameMatch)
    {
        List<CrpgUser> allCrpgUsers = new();
        allCrpgUsers.AddRange(gameMatch.TeamA);
        allCrpgUsers.AddRange(gameMatch.TeamB);
        allCrpgUsers.AddRange(gameMatch.Waiting);
        GameMatch returnedGameMatch = new();
        bool teamA = true;
        foreach (CrpgUser user in allCrpgUsers.OrderByDescending(u => u.Character.Rating.GetWorkingRating()))
        {
            if (teamA)
            {
                returnedGameMatch.TeamA.Add(user);
                teamA = !teamA;
            }
            else
            {
                returnedGameMatch.TeamB.Add(user);
                teamA = !teamA;
            }
        }

        return returnedGameMatch;
    }

    public GameMatch BannerBalancingWithEdgeCases(GameMatch gameMatch, bool firstBalance = true)
    {
        DumpTeamsStatus(gameMatch);
        Debug.Print(nameof(BannerBalancingWithEdgeCases));

        GameMatch balancedBannerGameMatch;
        // This is the path we take when team were randomly assigned.
        // we do not care of completely scrambling the teams since no round was played yet
        if (firstBalance)
        {
            Debug.Print("--------------------------------------------");
            Debug.Print("Now splitting the clan groups between the two team");
            balancedBannerGameMatch = KkMakeTeamOfSimilarSizesWithoutSplittingClanGroups(gameMatch);
        }

        // in this path , the teams already played at least one round , so we do not want to scramble the teams.
        else
        {
            Debug.Print("--------------------------------------------");
            Debug.Print("moving clanmates players to their clanmates teams , and moving players isolated from their clan to the opposite team.");
            balancedBannerGameMatch = RejoinClans(gameMatch);
        }

        DumpTeamsStatus(balancedBannerGameMatch);

        if (IsBalanceGoodEnough(balancedBannerGameMatch, maxSizeRatio: 0.75f, maxDifference: 10f, percentageDifference: 0.05f))
        {
            Debug.Print("Balance was good enough. We're not balancing this round");
            return balancedBannerGameMatch;
        }

        Debug.Print("Banner balancing now");
        balancedBannerGameMatch = BalanceTeamOfSimilarSizes(balancedBannerGameMatch, bannerBalance: true, 0.025f);
        Debug.Print("Banner balancing done");
        DumpTeamsStatus(balancedBannerGameMatch);

        if (IsBalanceGoodEnough(balancedBannerGameMatch, maxSizeRatio: 0.75f, maxDifference: 10f, percentageDifference: 0.10f))
        {
            Debug.Print("Balance is acceptable");
        }
        else
        {
            // This are failcases in case bannerbalance was not enough
            Debug.Print("Balance is unnacceptable");
            balancedBannerGameMatch = BalanceTeamOfSimilarSizes(balancedBannerGameMatch, bannerBalance: false, 0.10f);

            if (IsBalanceGoodEnough(balancedBannerGameMatch, maxSizeRatio: 0.75f, maxDifference: 10f, percentageDifference: 0.15f))
            {
                // A few swaps solved the problem. Most of the clangroups are intact
                Debug.Print("Balance is now Acceptable");
            }
            else
            {
                // A few swaps were not enough. Swaps are a form of gradient descent. Sometimes there are local extremas that are not global extremas
                // Here we completely abandon banner balance by completely reshuffling the card then redoing swaps
                Debug.Print("Swaps were not enough. This should really not happen often");
                DumpTeams(balancedBannerGameMatch);
                Debug.Print("NaiveCaptainBalancing + Balancing Without BannerGrouping");
                balancedBannerGameMatch = NaiveCaptainBalancing(balancedBannerGameMatch);
                balancedBannerGameMatch = BalanceTeamOfSimilarSizes(balancedBannerGameMatch, false, 0.001f);
            }
        }

        DumpTeamsStatus(balancedBannerGameMatch);
        return balancedBannerGameMatch;
    }

    public GameMatch KkMakeTeamOfSimilarSizesWithoutSplittingClanGroups(GameMatch gameMatch)
    {
        List<CrpgUser> allUsers = new();
        allUsers.AddRange(gameMatch.TeamA);
        allUsers.AddRange(gameMatch.TeamB);
        allUsers.AddRange(gameMatch.Waiting);

        var clanGroups = SplitUsersIntoClanGroups(allUsers);
        ClanGroup[] clanGroupsArray = clanGroups.ToArray();
        float[] clanGroupSizes = new float[clanGroups.Count];
        for (int i = 0; i < clanGroupsArray.Length; i++)
        {
            clanGroupSizes[i] = clanGroups[i].Size;
        }

        // The value 2 means we're splitting clan groups into two teams
        var partition = Heuristic(clanGroupsArray, clanGroupSizes, 2, preSorted: false);
        return new GameMatch
        {
            TeamA = JoinClanGroupsIntoUsers(partition.Partition[0].ToList()),
            TeamB = JoinClanGroupsIntoUsers(partition.Partition[1].ToList()),
            Waiting = new List<CrpgUser>(),
        };
    }

    public GameMatch BalanceTeamOfSimilarSizes(GameMatch gameMatch, bool bannerBalance, float threshold)
    {
        // Rescaling X dimension to the Y scale to make it as important.
        float sizeScaler = (gameMatch.TeamA.Sum(u => Math.Abs(u.Character.Rating.GetWorkingRating())) +
                            gameMatch.TeamB.Sum(u => Math.Abs(u.Character.Rating.GetWorkingRating()))) / (gameMatch.TeamA.Count + gameMatch.TeamB.Count);
        int i = 0;
        for (; i < MaximumNumberOfSwaps; i++)
        {
            if (IsBalanceGoodEnough(gameMatch, maxSizeRatio: 0.75f, maxDifference: 10f, percentageDifference: threshold))
            {
                Debug.Print("Teams are of similar sizes and similar ratings");
                break;
            }

            if (bannerBalance)
            {
                if (!FindAndSwapClanGroups(gameMatch, sizeScaler))
                {
                    Debug.Print("No more swap with banner balance available");
                    break;
                }
            }
            else
            {
                if (!FindAndSwapUsers(gameMatch, sizeScaler))
                {
                    Debug.Print("No more swap without banner balance available");
                    break;
                }
            }
        }

        Debug.Print($"Made {i} swaps '{(bannerBalance ? "using banner balance" : "without banner balance")}'");

        return gameMatch;
    }

    /// <summary>
    /// In the worse scenario, the balancer can split clans. This method rejoins them so the caller can find other
    /// strategies to balance the teams. It's usually called after the end of the round when new players have join
    /// the game and can help to have a balance without splitting clans.
    /// </summary>
    internal static GameMatch RejoinClans(GameMatch gameMatch)
    {
        int teamASize = gameMatch.TeamA.Count;
        int teamBSize = gameMatch.TeamB.Count;
        var newGameMatch = GroupTeamsByClan(gameMatch);

        Dictionary<int, (int teamACount, int teamBCount)> clanMemberCounts = new();
        foreach (ClanGroup clanGroup in newGameMatch.TeamA)
        {
            if (clanGroup.ClanId == null)
            {
                continue;
            }

            clanMemberCounts[clanGroup.ClanId.Value] = (clanGroup.Size, 0);
        }

        foreach (ClanGroup clanGroup in newGameMatch.TeamB)
        {
            if (clanGroup.ClanId == null)
            {
                continue;
            }

            clanMemberCounts.TryGetValue(clanGroup.ClanId.Value, out var clanCounts);
            clanMemberCounts[clanGroup.ClanId.Value] = (clanCounts.teamACount, clanCounts.teamACount + 1);
        }

        foreach (ClanGroup clanGroup in newGameMatch.Waiting)
        {
            if (clanGroup.ClanId == null)
            {
                if (teamASize < teamBSize)
                {
                    newGameMatch.TeamA.Add(clanGroup);
                    teamASize += 1;
                }
                else
                {
                    newGameMatch.TeamB.Add(clanGroup);
                    teamBSize += 1;
                }

                continue;
            }

            if (!clanMemberCounts.TryGetValue(clanGroup.ClanId.Value, out var clanCounts))
            {
                if (teamASize < teamBSize)
                {
                    newGameMatch.TeamA.Add(clanGroup);
                    teamASize += clanGroup.Size;
                }
                else
                {
                    newGameMatch.TeamB.Add(clanGroup);
                    teamBSize += clanGroup.Size;
                }

                continue;
            }

            if (clanCounts.teamACount > clanCounts.teamBCount)
            {
                newGameMatch.TeamA.Add(clanGroup);
                teamASize += clanGroup.Size;
                clanMemberCounts[clanGroup.ClanId.Value] = (clanCounts.teamACount + clanGroup.Size, clanCounts.teamBCount);
            }
            else
            {
                newGameMatch.TeamB.Add(clanGroup);
                teamBSize += clanGroup.Size;
                clanMemberCounts[(int)clanGroup.ClanId] = (clanCounts.teamACount, clanCounts.teamBCount + clanGroup.Size);
            }
        }

        newGameMatch.Waiting.Clear();
        foreach (ClanGroup clanGroup in newGameMatch.TeamA.ToArray()) // foreach does not like pulling the rug under its own feet
        {
            if (clanGroup.ClanId == null)
            {
                continue;
            }

            if (!clanMemberCounts.TryGetValue(clanGroup.ClanId.Value, out var clanCounts))
            {
                Debug.Print($"WARNING at this point of gameMatchRegroupClans the clan {clanGroup.ClanId} should already be in the dictionary");
                continue;
            }

            if (clanCounts.teamACount > clanCounts.teamBCount)
            {
                if (clanCounts.teamBCount > 0)
                {
                    var clanGroupToMove = newGameMatch.TeamB.Find(g => g.ClanId == clanGroup.ClanId);
                    newGameMatch.TeamA.Add(clanGroupToMove);
                    teamASize += clanGroupToMove.Size;
                    newGameMatch.TeamB.Remove(clanGroupToMove);
                    teamBSize -= clanGroupToMove.Size;
                    clanMemberCounts[clanGroup.ClanId.Value] = (clanCounts.teamACount + clanGroupToMove.Size, clanCounts.teamBCount - clanGroupToMove.Size);
                }
            }
            else
            {
                if (clanCounts.teamACount > 0)
                {
                    var clanGroupToMove = newGameMatch.TeamA.Find(g => g.ClanId == clanGroup.ClanId);
                    newGameMatch.TeamA.Remove(clanGroupToMove);
                    teamASize -= clanGroupToMove.Size;
                    newGameMatch.TeamB.Add(clanGroupToMove);
                    teamBSize += clanGroupToMove.Size;
                    clanMemberCounts[clanGroup.ClanId.Value] = (clanCounts.teamACount - clanGroupToMove.Size, clanCounts.teamBCount + clanGroupToMove.Size);
                }
            }
        }

        return ClanGroupsGameMatchIntoGameMatch(newGameMatch);
    }

    internal static List<ClanGroup> SplitUsersIntoClanGroups(List<CrpgUser> users)
    {
        Dictionary<int, ClanGroup> clanGroupsByClanId = new();
        List<ClanGroup> clanGroups = new();

        foreach (CrpgUser user in users.OrderByDescending(u => u.ClanMembership?.ClanId ?? 0))
        {
            ClanGroup clanGroup;
            if (user.ClanMembership == null)
            {
                clanGroup = new(null);
                clanGroups.Add(clanGroup);
            }
            else if (!clanGroupsByClanId.TryGetValue(user.ClanMembership.ClanId, out clanGroup))
            {
                clanGroup = new(user.ClanMembership.ClanId);
                clanGroups.Add(clanGroup);
                clanGroupsByClanId[user.ClanMembership.ClanId] = clanGroup;
            }

            clanGroup.Add(user);
        }

        return clanGroups;
    }

    internal static List<CrpgUser> JoinClanGroupsIntoUsers(List<ClanGroup> clanGroups)
    {
        List<CrpgUser> users = new();

        foreach (ClanGroup clanGroup in clanGroups)
        {
            users.AddRange(clanGroup.Members);
        }

        return users;
    }

    internal static void DumpTeams(GameMatch gameMatch)
    {
        Debug.Print("-----------------------");
        Debug.Print("Team A");
        foreach (CrpgUser u in gameMatch.TeamA)
        {
            Debug.Print($"{u.Character.Name} :  {u.Character.Rating.GetWorkingRating()}");
        }

        Debug.Print("-----------------------");
        Debug.Print("Team B");
        foreach (CrpgUser u in gameMatch.TeamB)
        {
            Debug.Print($"{u.Character.Name} :  {u.Character.Rating.GetWorkingRating()}");
        }

        Debug.Print("-----------------------");
        Debug.Print("WaitingToJoin");
        foreach (CrpgUser u in gameMatch.Waiting)
        {
            Debug.Print($"{u.Character.Name} :  {u.Character.Rating.GetWorkingRating()}");
        }

        Debug.Print("-----------------------");
    }

    internal static void DumpTeamsStatus(GameMatch gameMatch)
    {
        Debug.Print("--------------------------------------------");
        Debug.Print($"Team A Count {gameMatch.TeamA.Count} WorkingRating: {RatingHelpers.ComputeTeamRatingPowerSum(gameMatch.TeamA)} GlickoRating: {RatingHelpers.ComputeTeamGlickoRatingPowerSum(gameMatch.TeamB)}");
        Debug.Print($"Team B Count {gameMatch.TeamB.Count} WorkingRating: {RatingHelpers.ComputeTeamRatingPowerSum(gameMatch.TeamB)} GlickoRating: {RatingHelpers.ComputeTeamGlickoRatingPowerSum(gameMatch.TeamB)}");
        Debug.Print($"Waiting Team Count {gameMatch.Waiting.Count} Rating: {RatingHelpers.ComputeTeamRatingPowerSum(gameMatch.Waiting)}");
        Debug.Print("--------------------------------------------");
    }

    private static ClanGroupsGameMatch GroupTeamsByClan(GameMatch gameMatch)
    {
        return new ClanGroupsGameMatch
        {
            TeamA = SplitUsersIntoClanGroups(gameMatch.TeamA),
            TeamB = SplitUsersIntoClanGroups(gameMatch.TeamB),
            Waiting = SplitUsersIntoClanGroups(gameMatch.Waiting),
        };
    }

    private static GameMatch ClanGroupsGameMatchIntoGameMatch(ClanGroupsGameMatch clanGroupsGameMatch)
    {
        return new GameMatch
        {
            TeamA = JoinClanGroupsIntoUsers(clanGroupsGameMatch.TeamA),
            TeamB = JoinClanGroupsIntoUsers(clanGroupsGameMatch.TeamB),
            Waiting = JoinClanGroupsIntoUsers(clanGroupsGameMatch.Waiting),
        };
    }

    /// <summary>
    /// Looks for a single clan group in the team that has the least players and swaps it with a list of clan groups
    /// from the other team. The swap is done to minimize the difference in both sizes and ratings between the twom teams.
    /// </summary>
    /// <returns>False if it found no suitable swaps.</returns>
    private bool FindAndSwapClanGroups(GameMatch gameMatch, float sizeScaler)
    {
        ClanGroupsGameMatch clanGroupGameMatch = GroupTeamsByClan(gameMatch);

        List<CrpgUser> weakTeam;
        List<CrpgUser> strongTeam;
        List<ClanGroup> weakClanGroupsTeam;
        List<ClanGroup> strongClanGroupsTeam;
        if (RatingHelpers.ComputeTeamRatingDifference(gameMatch) < 0)
        {
            weakTeam = gameMatch.TeamA;
            strongTeam = gameMatch.TeamB;
            weakClanGroupsTeam = clanGroupGameMatch.TeamA;
            strongClanGroupsTeam = clanGroupGameMatch.TeamB;
        }
        else
        {
            weakTeam = gameMatch.TeamB;
            strongTeam = gameMatch.TeamA;
            weakClanGroupsTeam = clanGroupGameMatch.TeamB;
            strongClanGroupsTeam = clanGroupGameMatch.TeamA;
        }

        int userCountDifference = weakClanGroupsTeam.Sum(c => c.Size) - strongClanGroupsTeam.Sum(c => c.Size);
        bool swappingFromWeakTeam = userCountDifference <= 0; // We are swapping from the team with the least player.
        userCountDifference = Math.Abs(userCountDifference);

        float teamRatingDiff = Math.Abs(RatingHelpers.ComputeTeamRatingDifference(gameMatch));
        weakClanGroupsTeam = weakClanGroupsTeam.OrderBy(c => c.RatingPMean()).ToList();
        strongClanGroupsTeam = strongClanGroupsTeam.OrderBy(c => c.RatingPMean()).ToList();
        var clanGroupsToSwapUsingAngleTuple = FindBestClanGroupsSwap(weakClanGroupsTeam, strongClanGroupsTeam, teamRatingDiff / 2f, userCountDifference / 2, true, swappingFromWeakTeam, sizeScaler);
        var clanGroupsToSwapUsingDistanceTuple = FindBestClanGroupsSwap(weakClanGroupsTeam, strongClanGroupsTeam, teamRatingDiff / 2f, userCountDifference / 2, false, swappingFromWeakTeam, sizeScaler);

        ClanGroup clanGroupToSwap1;
        List<ClanGroup> clanGroupsToSwap2;
        if (clanGroupsToSwapUsingAngleTuple.distanceToTarget < clanGroupsToSwapUsingDistanceTuple.distanceToTarget)
        {
            Debug.Print($"{nameof(FindAndSwapClanGroups)}: angle method won");
            clanGroupToSwap1 = clanGroupsToSwapUsingAngleTuple.clanGrouptoSwap1;
            clanGroupsToSwap2 = clanGroupsToSwapUsingAngleTuple.clanGroupsToSwap2;
        }
        else
        {
            Debug.Print($"{nameof(FindAndSwapClanGroups)}: distance method won");
            clanGroupToSwap1 = clanGroupsToSwapUsingDistanceTuple.clanGrouptoSwap1;
            clanGroupsToSwap2 = clanGroupsToSwapUsingDistanceTuple.clanGroupsToSwap2;
        }

        if (!IsSwapValid(strongTeam, weakTeam, swappingFromWeakTeam, clanGroupToSwap1.Size,
                clanGroupToSwap1.RatingPsum(), clanGroupsToSwap2.Sum(c => c.Size),
                ClanGroupsPowerSum(clanGroupsToSwap2), sizeScaler))
        {
            return false;
        }

        (List<CrpgUser> teamToSwapFrom, List<CrpgUser> teamToSwapInto) = swappingFromWeakTeam
            ? (weakTeam, strongTeam)
            : (strongTeam, weakTeam);

        foreach (var clanGroup in clanGroupsToSwap2)
        {
            foreach (CrpgUser user in clanGroup.Members)
            {
                teamToSwapInto.Remove(user);
                teamToSwapFrom.Add(user);
            }
        }

        foreach (CrpgUser user in clanGroupToSwap1.Members)
        {
            teamToSwapFrom.Remove(user);
            teamToSwapInto.Add(user);
        }

        return true;
    }

    private bool FindAndSwapUsers(GameMatch gameMatch, float sizeScaler)
    {
        (List<CrpgUser> weakTeam, List<CrpgUser> strongTeam) = RatingHelpers.ComputeTeamRatingDifference(gameMatch) < 0
            ? (gameMatch.TeamA, gameMatch.TeamB)
            : (gameMatch.TeamB, gameMatch.TeamA);
        int userCountDifference = weakTeam.Count - strongTeam.Count;
        bool swappingFromWeakTeam = userCountDifference <= 0;
        float sizeOffset = Math.Abs(userCountDifference);

        (List<CrpgUser> teamToSwapFrom, List<CrpgUser> teamToSwapTo) = swappingFromWeakTeam
            ? (weakTeam, strongTeam)
            : (strongTeam, weakTeam);
        double teamRatingDiff = Math.Abs(RatingHelpers.ComputeTeamRatingDifference(gameMatch));
        // here the crpgUserToSwap can be null if the the team that has the least players is empty.
        CrpgUser? bestCrpgUserToSwap1 = swappingFromWeakTeam
            ? weakTeam.OrderBy(c => c.Character.Rating.GetWorkingRating()).FirstOrDefault()
            : strongTeam.OrderBy(c => c.Character.Rating.GetWorkingRating()).LastOrDefault();
        // These calculation are made to account for both the case where we are swapping a user with a list of user , or swapping no one with a list of users.
        float bestCrpgUserToSwap1Rating = bestCrpgUserToSwap1 != null ? bestCrpgUserToSwap1.Character.Rating.GetWorkingRating() : 0;
        int bestCrpgUserToSwap1Count = bestCrpgUserToSwap1 != null ? 1 : 0;
        double targetRating = swappingFromWeakTeam
            ? bestCrpgUserToSwap1Rating + Math.Abs(teamRatingDiff) / 2f
            : bestCrpgUserToSwap1Rating - Math.Abs(teamRatingDiff) / 2f;
        List<CrpgUser> bestCrpgUsersToSwap2 = FindCrpgUsersToSwap((float)targetRating, teamToSwapTo, sizeOffset / 2f, sizeScaler);

        // the pair difference (strong - weak) needs to be close to TargetVector
        Vector2 targetVector = new(sizeOffset * sizeScaler, (float)teamRatingDiff / 2f);
        Vector2 bestPairVector = new(
            (bestCrpgUsersToSwap2.Count - 1) * sizeScaler,
            Math.Abs(bestCrpgUserToSwap1Rating - bestCrpgUsersToSwap2.Sum(u => u.Character.Rating.GetWorkingRating())));

        foreach (var user in teamToSwapFrom)
        {
            targetRating = swappingFromWeakTeam
                ? user.Character.Rating.GetWorkingRating() + Math.Abs(teamRatingDiff) / 2f
                : user.Character.Rating.GetWorkingRating() - Math.Abs(teamRatingDiff) / 2f;
            List<CrpgUser> potentialCrpgUsersToSwap = FindCrpgUsersToSwap((float)targetRating, teamToSwapTo, bestCrpgUserToSwap1Count + sizeOffset / 2f, sizeScaler);
            Vector2 potentialPairVector = new(
                (potentialCrpgUsersToSwap.Count - 1) * sizeScaler,
                Math.Abs(user.Character.Rating.GetWorkingRating() - potentialCrpgUsersToSwap.Sum(u => u.Character.Rating.GetWorkingRating())));
            if ((targetVector - potentialPairVector).Length() < (targetVector - bestPairVector).Length())
            {
                bestCrpgUserToSwap1 = user;
                bestCrpgUsersToSwap2 = potentialCrpgUsersToSwap;
                bestPairVector = potentialPairVector;
            }
        }

        if (!IsSwapValid(strongTeam, weakTeam, swappingFromWeakTeam, bestCrpgUserToSwap1Count,
                bestCrpgUserToSwap1Rating, bestCrpgUsersToSwap2.Count,
                bestCrpgUsersToSwap2.Sum(u => u.Character.Rating.GetWorkingRating()), sizeScaler))
        {
            return false;
        }

        if (swappingFromWeakTeam)
        {
            foreach (CrpgUser user in bestCrpgUsersToSwap2)
            {
                weakTeam.Add(user);
                strongTeam.Remove(user);
            }

            // null if the swap is just moving players from one team to another
            if (bestCrpgUserToSwap1 != null)
            {
                strongTeam.Add(bestCrpgUserToSwap1);
                weakTeam.Remove(bestCrpgUserToSwap1);
            }
        }
        else
        {
            foreach (CrpgUser user in bestCrpgUsersToSwap2)
            {
                weakTeam.Remove(user);
                strongTeam.Add(user);
            }

            // null when the team that has the least player is null
            if (bestCrpgUserToSwap1 != null)
            {
                strongTeam.Remove(bestCrpgUserToSwap1);
                weakTeam.Add(bestCrpgUserToSwap1);
            }
        }

        return true;
    }

    /// <summary>
    /// Find a clan group from the team with that has the least player and swap it with a list of clan groups from the
    /// opposite team in order to minimize rating and size differences.
    ///
    /// The task at hand is complex because it requires us to balance teams while making sure both ratings and size
    /// are similar. To do this, we convert this to geometrical problem.
    ///
    /// A clan group is now a vector of dimension 2 => (size, rating).
    /// </summary>
    /// <param name="weakClanGroupsTeam">Weak team.</param>
    /// <param name="strongClanGroupsTeam">Strong team.</param>
    /// <param name="halfRatingDifference">Half of the current rating absolute difference between the teams.</param>
    /// <param name="targetSwapSizeDifference">The target size difference between the two members of the swap.</param>
    /// <param name="usingAngle">If the angle method should be used.</param>
    /// <param name="swappingFromWeakTeam">Has the weak team the least players.</param>
    /// <param name="sizeScaler">Size scaler.</param>
    /// <returns>The swap to perform.</returns>
    private (ClanGroup clanGrouptoSwap1, List<ClanGroup> clanGroupsToSwap2, float distanceToTarget) FindBestClanGroupsSwap(
        List<ClanGroup> weakClanGroupsTeam, List<ClanGroup> strongClanGroupsTeam, float halfRatingDifference,
        int targetSwapSizeDifference, bool usingAngle, bool swappingFromWeakTeam, float sizeScaler)
    {
        (List<ClanGroup> teamToSwapFrom, List<ClanGroup> teamToSwapInto) = swappingFromWeakTeam
            ? (weakClanGroupsTeam, strongClanGroupsTeam)
            : (strongClanGroupsTeam, weakClanGroupsTeam);
        Vector2 targetVector = new(targetSwapSizeDifference * sizeScaler, halfRatingDifference);
        // If the team that has the least players is empty, we will do the swap with an empty clan group. A bit weird
        // but very practical for the rest of the algorithm to avoid null checks.
        ClanGroup emptyClanGroup = new(null);
        ClanGroup weakClanGroupToSwap = weakClanGroupsTeam.FirstOrDefault() ?? emptyClanGroup;
        ClanGroup strongClanGroupToSwap = strongClanGroupsTeam.LastOrDefault() ?? emptyClanGroup;

        // Initializing a first pair to compare afterward with other pairs
        float bestClanGroupToSwapTargetRating = swappingFromWeakTeam
            ? weakClanGroupToSwap.RatingPsum() + halfRatingDifference
            : strongClanGroupToSwap.RatingPsum() - halfRatingDifference;

        ClanGroup bestClanGroupToSwapSource = swappingFromWeakTeam ? weakClanGroupToSwap : strongClanGroupToSwap;

        List<ClanGroup> bestClanGroupToSwapDestination = FindAClanGroupToSwapUsing(
            bestClanGroupToSwapTargetRating,
            bestClanGroupToSwapSource.Size + Math.Abs(targetSwapSizeDifference),
            sizeScaler,
            teamToSwapInto,
            usingAngle);

        Vector2 bestSwapVector = new(
            (ClanGroupsSize(bestClanGroupToSwapDestination) - bestClanGroupToSwapSource.Size) * sizeScaler,
            Math.Abs(bestClanGroupToSwapSource.RatingPsum() - ClanGroupsRating(bestClanGroupToSwapDestination)));
        float distanceToTargetVector = (targetVector - bestSwapVector).Length();

        foreach (ClanGroup clanGroup in teamToSwapFrom)
        {
            // c is the potential first member of the pair (potentialClanGrouptoSwap1)
            // we compute below what's the target rating for the second member of the pair
            float potentialClanGroupToSwapTargetRating = swappingFromWeakTeam
                ? clanGroup.RatingPsum() + halfRatingDifference
                : clanGroup.RatingPsum() - halfRatingDifference;
            // potential second member of the pair
            List<ClanGroup> potentialClanGroupToSwap = FindAClanGroupToSwapUsing(
                potentialClanGroupToSwapTargetRating,
                sizeScaler,
                clanGroup.Size + Math.Abs(targetSwapSizeDifference),
                teamToSwapInto,
                usingAngle);
            Vector2 potentialSwapVector = new(
                (ClanGroupsSize(potentialClanGroupToSwap) - clanGroup.Size) * sizeScaler,
                Math.Abs(clanGroup.RatingPsum() - ClanGroupsRating(potentialClanGroupToSwap)));
            if ((targetVector - potentialSwapVector).Length() < (targetVector - bestSwapVector).Length())
            {
                bestClanGroupToSwapSource = clanGroup;
                bestClanGroupToSwapDestination = potentialClanGroupToSwap;
                bestSwapVector = potentialSwapVector;
                distanceToTargetVector = (targetVector - potentialSwapVector).Length();
            }
        }

        return (bestClanGroupToSwapSource, bestClanGroupToSwapDestination, distanceToTargetVector);
    }

    private bool IsSwapValid(List<CrpgUser> strongTeam, List<CrpgUser> weakTeam, bool swappingFromWeakTeam,
        int sourceGroupSize, float sourceGroupRating, int destinationGroupSize, float destinationGroupRating,
        float sizeScaler)
    {
        float newTeamRatingDiff = swappingFromWeakTeam
            ? strongTeam.Sum(u => u.Character.Rating.GetWorkingRating()) + 2f * sourceGroupRating - 2f * destinationGroupRating - weakTeam.Sum(u => u.Character.Rating.GetWorkingRating())
            : strongTeam.Sum(u => u.Character.Rating.GetWorkingRating()) - 2f * sourceGroupRating + 2f * destinationGroupRating - weakTeam.Sum(u => u.Character.Rating.GetWorkingRating());
        float newTeamSizeDiff = swappingFromWeakTeam
            ? strongTeam.Count + 2 * sourceGroupSize - 2f * destinationGroupSize - weakTeam.Count
            : strongTeam.Count - 2 * sourceGroupSize + 2f * destinationGroupSize - weakTeam.Count;

        Vector2 oldDifferenceVector = new((strongTeam.Count - weakTeam.Count) * sizeScaler,
            strongTeam.Sum(u => u.Character.Rating.GetWorkingRating()) - weakTeam.Sum(u => u.Character.Rating.GetWorkingRating()));
        Vector2 newDifferenceVector = new(newTeamSizeDiff * sizeScaler, newTeamRatingDiff);
        return newDifferenceVector.Length() < oldDifferenceVector.Length();
    }

    private bool IsRatingRatioAcceptable(GameMatch gameMatch, float percentageDifference)
    {
        double ratingRatio = Math.Abs(
            (RatingHelpers.ComputeTeamRatingPowerSum(gameMatch.TeamB) - RatingHelpers.ComputeTeamRatingPowerSum(gameMatch.TeamA))
            / gameMatch.TeamA.Sum(u => Math.Abs(u.Character.Rating.GetWorkingRating())));
        return MathHelper.Within((float)ratingRatio, 0f, percentageDifference);
    }

    private bool IsTeamSizeDifferenceAcceptable(GameMatch gameMatch, float maxSizeRatio, float maxDifference)
    {
        bool tooMuchSizeRatioDifference = !MathHelper.Within(
            gameMatch.TeamA.Count / (float)gameMatch.TeamB.Count,
            maxSizeRatio,
            1f / maxSizeRatio);
        bool sizeDifferenceGreaterThanThreshold = Math.Abs(gameMatch.TeamA.Count - gameMatch.TeamB.Count) > maxDifference;
        bool differenceOfOnlyOne = MathHelper.Within(gameMatch.TeamA.Count - gameMatch.TeamB.Count, -1, 1);
        return (!tooMuchSizeRatioDifference && !sizeDifferenceGreaterThanThreshold) || differenceOfOnlyOne;
    }

    private bool IsBalanceGoodEnough(GameMatch gameMatch, float maxSizeRatio, float maxDifference, float percentageDifference)
    {
        return IsTeamSizeDifferenceAcceptable(gameMatch, maxSizeRatio, maxDifference) && IsRatingRatioAcceptable(gameMatch, percentageDifference);
    }

    private int ClanGroupsSize(List<ClanGroup> clanGroups)
    {
        return clanGroups.Sum(c => c.Size);
    }

    private float ClanGroupsRating(List<ClanGroup> clanGroups)
    {
        return clanGroups.Sum(c => c.RatingPsum());
    }

    private List<CrpgUser> FindCrpgUsersToSwap(float targetRating, List<CrpgUser> teamToSelectFrom, float desiredSize, float sizeScaler)
    {
        List<CrpgUser> team = teamToSelectFrom.ToList();
        List<CrpgUser> usersToSwap = new();
        Vector2 usersToSwapVector = new(0, 0);
        for (int i = 0; i < teamToSelectFrom.Count; i++)
        {
            CrpgUser bestUserToAdd = team.First();
            Vector2 bestUserToAddVector = new(sizeScaler, bestUserToAdd.Character.Rating.GetWorkingRating());
            Vector2 objectiveVector = new(sizeScaler * desiredSize, targetRating);
            if (objectiveVector.Length() == 0f)
            {
                break;
            }

            foreach (CrpgUser user in team)
            {
                Vector2 userVector = new(sizeScaler, user.Character.Rating.GetWorkingRating());

                if ((usersToSwapVector + userVector - objectiveVector).Length() < (usersToSwapVector + bestUserToAddVector - objectiveVector).Length())
                {
                    bestUserToAdd = user;
                    bestUserToAddVector = new(sizeScaler, bestUserToAdd.Character.Rating.GetWorkingRating());
                }
            }

            if ((usersToSwapVector + bestUserToAddVector - objectiveVector).Length() < (usersToSwapVector - objectiveVector).Length())
            {
                team.Remove(bestUserToAdd);
                usersToSwap.Add(bestUserToAdd);
                usersToSwapVector = new(usersToSwap.Count * sizeScaler, usersToSwap.Sum(u => u.Character.Rating.GetWorkingRating()));
            }
            else
            {
                team.Remove(bestUserToAdd);
            }
        }

        return usersToSwap;
    }

    /// <summary>
    /// Given one clan group find the best list of clan groups to pair with.
    /// </summary>
    /// <param name="targetRating">Target rating for the returned clan groups.</param>
    /// <param name="targetSize">Target size for the returned clan groups.</param>
    /// <param name="sizeScaler">Size scaler.</param>
    /// <param name="teamToSelectFrom">Team to select the clan groups from.</param>
    /// <param name="useAngle">Use the angle method.</param>
    private List<ClanGroup> FindAClanGroupToSwapUsing(float targetRating, float targetSize, float sizeScaler,
        List<ClanGroup> teamToSelectFrom, bool useAngle)
    {
        // Rescaling X dimension to the Y scale to make it as important.
        Vector2 objectiveVector = new(sizeScaler * targetSize, targetRating);
        float objectiveVectorDirection = Vector2Angles(objectiveVector);

        List<ClanGroup> team = teamToSelectFrom.ToList();
        List<ClanGroup> clanGroupsToSwap = new();
        for (int i = 0; i < team.Count; i++)
        {
            ClanGroup bestClanGroupToAdd = team.First();
            // TODO: clan groups ratings could be computed once.
            Vector2 bestClanGroupToAddVector = ClanGroupRescaledVector(sizeScaler, bestClanGroupToAdd);
            Vector2 clanGroupsToSwapVector = ClanGroupsRescaledVector(sizeScaler, clanGroupsToSwap);

            if (objectiveVector.Length() == 0f)
            {
                break;
            }

            foreach (ClanGroup clanGroup in team)
            {
                Vector2 clanGroupVector = ClanGroupRescaledVector(sizeScaler, clanGroup);
                if (useAngle)
                {
                    if (Math.Abs(Vector2Angles(clanGroupVector + clanGroupsToSwapVector) - objectiveVectorDirection) < Math.Abs(Vector2Angles(bestClanGroupToAddVector + clanGroupsToSwapVector) - objectiveVectorDirection))
                    {
                        bestClanGroupToAdd = clanGroup;
                        bestClanGroupToAddVector = ClanGroupRescaledVector(sizeScaler, bestClanGroupToAdd);
                    }
                }
                else
                {
                    if ((clanGroupsToSwapVector + clanGroupVector - objectiveVector).Length() < (clanGroupsToSwapVector + bestClanGroupToAddVector - objectiveVector).Length())
                    {
                        bestClanGroupToAdd = clanGroup;
                        bestClanGroupToAddVector = ClanGroupRescaledVector(sizeScaler, bestClanGroupToAdd);
                    }
                }
            }

            if ((clanGroupsToSwapVector + bestClanGroupToAddVector - objectiveVector).Length() < (clanGroupsToSwapVector - objectiveVector).Length())
            {
                team.Remove(bestClanGroupToAdd);
                clanGroupsToSwap.Add(bestClanGroupToAdd);
            }
            else
            {
                team.Remove(bestClanGroupToAdd);
            }
        }

        return clanGroupsToSwap;
    }

    /// <summary>
    /// Solves the partition problem using the Karmarkar--Karp algorithm.
    /// </summary>
    /// <param name="elements">The elements to partition into parts of similar weight.</param>
    /// <param name="weights">The weights of the elements, assumed to be equal in count to <paramref name="elements"/>.</param>
    /// <param name="numParts">The number of desired parts.</param>
    /// <param name="preSorted">Set to <see langword="true" /> to save time when the input weights are
    /// already sorted in descending order.</param>
    /// <returns>The partition as a <see cref="PartitioningResult{T}"/>.</returns>
    private PartitioningResult<T> Heuristic<T>(T[] elements, float[] weights, int numParts, bool preSorted = false)
    {
        if (numParts <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(numParts), $"{numParts} must be positive");
        }

        if (weights.Length == 0)
        {
            return new PartitioningResult<T>(
                Enumerable.Repeat(new List<T>(), numParts).ToArray(),
                Enumerable.Repeat(0f, numParts).ToArray());
        }

        int[] indexSortingMap = Enumerable.Range(0, weights.Length).ToArray();
        if (!preSorted)
        {
            // if elements is not sorted , sort them , but using indexSortingMap to remember their original position.
            Array.Sort(weights, indexSortingMap);
            weights = weights.Reverse().ToArray();
            indexSortingMap = indexSortingMap.Reverse().ToArray();
        }

        var partitions = new PriorityQueue<float, PartitionNode<T>>();
        // iteration on weights
        for (int i = 0; i < weights.Length; i++)
        {
            // number is current weight
            float number = weights[i];
            // Initialization of the Array of List
            var thisPartition = new List<T>[numParts];
            // initialisation of each list of the array except the last one
            for (int n = 0; n < numParts - 1; n++)
            {
                thisPartition[n] = new List<T>();
            }

            // last cell is a list that contains the biggest remaining element in the for loop
            thisPartition[numParts - 1] = new List<T> { elements[indexSortingMap[i]] };
            // thisSum is an array of double. The size of the Array is the number of partitions
            float[] thisSum = new float[numParts];
            // Last cell of the array contains current weight
            thisSum[numParts - 1] = number;
            // this Node has the array of list associated with the array this sum.
            var thisNode = new PartitionNode<T>(thisPartition, thisSum);
            // this enqueue one partition for each element.
            partitions.Enqueue(-number, thisNode);
        }

        // checked this part doing the algo by hand , this witchcraft works.
        for (int i = 0; i < weights.Length - 1; i++)
        {
            var node1 = partitions.Dequeue().Value;
            var node2 = partitions.Dequeue().Value;
            var newPartition = new List<T>[numParts];
            float[] newSizes = new float[numParts];
            for (int k = 0; k < numParts; k++)
            {
                newSizes[k] = node1.Sizes[k] + node2.Sizes[numParts - k - 1];
                node1.Partition[k].AddRange(node2.Partition[numParts - k - 1]);
                newPartition[k] = node1.Partition[k];
            }

            Array.Sort(newSizes, newPartition);
            var newNode = new PartitionNode<T>(newPartition, newSizes);
            double diff = newSizes[numParts - 1] - newSizes[0];
            partitions.Enqueue(-(float)diff, newNode);
        }

        var node = partitions.Dequeue().Value;
        return new PartitioningResult<T>(node.Partition, node.Sizes);
    }

    private float ClanGroupsPowerSum(List<ClanGroup> clanGroups)
    {
        return RatingHelpers.ComputeTeamRatingPowerSum(JoinClanGroupsIntoUsers(clanGroups));
    }

    private Vector2 ClanGroupRescaledVector(float scaler, ClanGroup clanGroup)
    {
        return new Vector2(clanGroup.Size * scaler, clanGroup.RatingPsum());
    }

    private Vector2 ClanGroupsRescaledVector(float scaler, List<ClanGroup> clanGroups)
    {
        return new Vector2(clanGroups.Sum(c => c.Size) * scaler, ClanGroupsPowerSum(clanGroups));
    }

    private float Vector2Angles(Vector2 v)
    {
        if (v == Vector2.Zero)
        {
            return 0;
        }

        return (float)Math.Atan2(v.Y, v.X);
    }

    private class PartitionNode<T>
    {
        internal PartitionNode(List<T>[] partition, float[] sizes)
        {
            Partition = partition;
            Sizes = sizes;
        }

        public List<T>[] Partition { get; }
        public float[] Sizes { get; }
    }
}
