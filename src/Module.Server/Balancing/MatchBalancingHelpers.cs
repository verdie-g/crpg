using System.Numerics;
using TaleWorlds.Library;

namespace Crpg.Module.Balancing;

internal static class MatchBalancingHelpers
{
    /// <summary>
    /// In the worse scenario, the balancer can split clans. This method rejoins them so the caller can find other
    /// strategies to balance the teams. It's usually called after the end of the round when new players have join
    /// the game and can help to have a balance without splitting clans.
    /// </summary>
    public static GameMatch RejoinClans(GameMatch gameMatch)
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
                    var clanGroupToMove = newGameMatch.TeamB.First(g => g.ClanId == clanGroup.ClanId);
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
                    var clanGroupToMove = newGameMatch.TeamA.First(g => g.ClanId == clanGroup.ClanId);
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

    public static List<ClanGroup> SplitUsersIntoClanGroups(List<WeightedCrpgUser> users)
    {
        Dictionary<int, ClanGroup> clanGroupsByClanId = new();
        List<ClanGroup> clanGroups = new();

        foreach (WeightedCrpgUser user in users.OrderByDescending(u => u.ClanId ?? 0))
        {
            ClanGroup? clanGroup;
            if (user.ClanId == null)
            {
                clanGroup = new(null);
                clanGroups.Add(clanGroup);
            }
            else if (!clanGroupsByClanId.TryGetValue(user.ClanId.Value, out clanGroup))
            {
                clanGroup = new(user.ClanId);
                clanGroups.Add(clanGroup);
                clanGroupsByClanId[user.ClanId.Value] = clanGroup;
            }

            clanGroup.Add(user);
        }

        return clanGroups;
    }

    public static List<WeightedCrpgUser> JoinClanGroupsIntoUsers(List<ClanGroup> clanGroups)
    {
        List<WeightedCrpgUser> users = new();

        foreach (ClanGroup clanGroup in clanGroups)
        {
            users.AddRange(clanGroup.Members);
        }

        return users;
    }

    public static int ClanGroupsSize(List<ClanGroup> clanGroups)
    {
        return clanGroups.Sum(c => c.Size);
    }

    public static float ClanGroupsWeight(List<ClanGroup> clanGroups)
    {
        return clanGroups.Sum(c => c.Weight());
    }

    public static ClanGroupsGameMatch GroupTeamsByClan(GameMatch gameMatch)
    {
        return new ClanGroupsGameMatch
        {
            TeamA = SplitUsersIntoClanGroups(gameMatch.TeamA),
            TeamB = SplitUsersIntoClanGroups(gameMatch.TeamB),
            Waiting = SplitUsersIntoClanGroups(gameMatch.Waiting),
        };
    }

    public static GameMatch ClanGroupsGameMatchIntoGameMatch(ClanGroupsGameMatch clanGroupsGameMatch)
    {
        return new GameMatch
        {
            TeamA = JoinClanGroupsIntoUsers(clanGroupsGameMatch.TeamA),
            TeamB = JoinClanGroupsIntoUsers(clanGroupsGameMatch.TeamB),
            Waiting = JoinClanGroupsIntoUsers(clanGroupsGameMatch.Waiting),
        };
    }

    public static List<WeightedCrpgUser> FindWeightedCrpgUsersToSwap(float targetWeight, List<WeightedCrpgUser> teamToSelectFrom, List<WeightedCrpgUser> teamToSwapInto, float desiredSize, float sizeScaler)
    {
        List<WeightedCrpgUser> teamToSelectFromCopy = teamToSelectFrom.ToList();
        List<WeightedCrpgUser> teamToSwapIntoCopy = teamToSwapInto.ToList();
        List<WeightedCrpgUser> usersToSwap = new();
        Vector2 usersToSwapVector = new(0, 0);
        for (int i = 0; i < teamToSelectFrom.Count; i++)
        {
            WeightedCrpgUser bestUserToAdd = teamToSelectFromCopy.First();
            Vector2 bestUserToAddVector = new(sizeScaler, ComputeMoveWeightHalfDifference(teamToSelectFromCopy, teamToSwapIntoCopy, bestUserToAdd));
            Vector2 objectiveVector = new(sizeScaler * desiredSize, targetWeight);
            if (objectiveVector.Length() == 0f)
            {
                break;
            }

            foreach (WeightedCrpgUser user in teamToSelectFromCopy)
            {
                Vector2 userVector = new(sizeScaler, ComputeMoveWeightHalfDifference(teamToSelectFromCopy, teamToSwapIntoCopy, user));

                if ((usersToSwapVector + userVector - objectiveVector).Length() < (usersToSwapVector + bestUserToAddVector - objectiveVector).Length())
                {
                    bestUserToAdd = user;
                    bestUserToAddVector = new(sizeScaler, bestUserToAdd.Weight);
                }
            }

            if ((usersToSwapVector + bestUserToAddVector - objectiveVector).Length() < (usersToSwapVector - objectiveVector).Length())
            {
                teamToSelectFromCopy.Remove(bestUserToAdd);
                usersToSwap.Add(bestUserToAdd);
                usersToSwapVector = new(usersToSwap.Count * sizeScaler, usersToSwap.Sum(u => u.Weight));
            }
            else
            {
                teamToSelectFromCopy.Remove(bestUserToAdd);
            }
        }

        return usersToSwap;
    }

    /// <summary>
    /// Given one clan group find the best list of clan groups to pair with.
    /// </summary>
    /// <param name="targetWeight">Target weight for the returned clan groups.</param>
    /// <param name="targetSize">Target size for the returned clan groups.</param>
    /// <param name="sizeScaler">Size scaler.</param>
    /// <param name="teamToSelectFrom">Team to select the clan groups from.</param>
    /// <param name="useAngle">Use the angle method.</param>
    public static List<ClanGroup> FindAClanGroupToSwapUsing(float targetWeight, float targetSize, float sizeScaler,
        List<ClanGroup> teamToSelectFrom, bool useAngle)
    {
        // Rescaling X dimension to the Y scale to make it as important.
        Vector2 objectiveVector = new(sizeScaler * targetSize, targetWeight);
        float objectiveVectorDirection = Vector2Angles(objectiveVector);

        List<ClanGroup> team = teamToSelectFrom.ToList();
        List<ClanGroup> clanGroupsToSwap = new();
        for (int i = 0; i < team.Count; i++)
        {
            ClanGroup bestClanGroupToAdd = team.First();
            // TODO: clan groups weights could be computed once.
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
    /// Compute the user theoritical wait when you move him out of his clanGroup into the other team. The other team may have a users of the same clan
    /// </summary>
    public static float ComputeMoveWeightHalfDifference(List<WeightedCrpgUser> teamToSwapFrom, List<WeightedCrpgUser> teamToSwapInto, WeightedCrpgUser? userToMove)
    {
        // Can be optimized as we don't need to recompute the clangroups that did not change. Would make the method a bit more complex though
        // Usually if i remove X from team A to give X to team B , 2X should be equal to the diff in order to bridge the gap.
        // Here even though i remove X from Team A , i'm not giving X to team B because of clangroup penalty i'm giving Y. This Method computes (X + Y) /2
        if (userToMove == null)
        {
            return 0;
        }

        List<ClanGroup> teamAClanGroups = SplitUsersIntoClanGroups(teamToSwapFrom);
        List<ClanGroup> teamBClanGroups = SplitUsersIntoClanGroups(teamToSwapInto);
        List<WeightedCrpgUser> newTeamA = teamToSwapFrom.ToList(); // ToList To do a deep copy of the list and not Impact the original one
        List<WeightedCrpgUser> newTeamB = teamToSwapInto.ToList();
        newTeamA.Remove(userToMove);
        newTeamB.Add(userToMove);
        List<ClanGroup> newteamToSwapFromClanGroups = SplitUsersIntoClanGroups(newTeamA);
        List<ClanGroup> newteamToSwapIntoClanGroups = SplitUsersIntoClanGroups(newTeamB);
        return (teamAClanGroups.Sum(c => c.Weight()) - newteamToSwapFromClanGroups.Sum(c => c.Weight()) + newteamToSwapIntoClanGroups.Sum(c => c.Weight()) - teamBClanGroups.Sum(c => c.Weight())) / 2f;
    }

    public static float ComputeTeamDiffAfterSwap(List<WeightedCrpgUser> teamToSwapFrom, List<WeightedCrpgUser> teamToSwapInto, List<WeightedCrpgUser> usersToMoveFromTeam1, List<WeightedCrpgUser> usersToMoveFromTeam2)
    {
        teamToSwapFrom = teamToSwapFrom.Except(usersToMoveFromTeam1).ToList();
        teamToSwapInto = teamToSwapInto.Except(usersToMoveFromTeam2).ToList();
        teamToSwapFrom.AddRange(usersToMoveFromTeam2);
        teamToSwapInto.AddRange(usersToMoveFromTeam1);
        List<ClanGroup> newteamToSwapFromClanGroups = SplitUsersIntoClanGroups(teamToSwapFrom);
        List<ClanGroup> newteamToSwapIntoClanGroups = SplitUsersIntoClanGroups(teamToSwapInto);
        return newteamToSwapFromClanGroups.Sum(c => c.Weight()) - newteamToSwapIntoClanGroups.Sum(c => c.Weight());
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
    public static PartitioningResult<T> Heuristic<T>(T[] elements, float[] weights, int numParts, bool preSorted = false)
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

        var partitions = new TaleWorlds.Library.PriorityQueue<float, PartitionNode<T>>();
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

    public static void DumpTeams(GameMatch gameMatch)
    {
        Debug.Print("-----------------------");
        Debug.Print("Team A");
        foreach (WeightedCrpgUser u in gameMatch.TeamA)
        {
            Debug.Print($"{u.User.Name} :  {u.Weight}");
        }

        Debug.Print("-----------------------");
        Debug.Print("Team B");
        foreach (WeightedCrpgUser u in gameMatch.TeamB)
        {
            Debug.Print($"{u.User.Name} :  {u.Weight}");
        }

        Debug.Print("-----------------------");
        Debug.Print("WaitingToJoin");
        foreach (WeightedCrpgUser u in gameMatch.Waiting)
        {
            Debug.Print($"{u.User.Name} :  {u.Weight}");
        }

        Debug.Print("-----------------------");
    }

    public static void DumpTeamsStatus(GameMatch gameMatch)
    {
        Debug.Print("--------------------------------------------");
        Debug.Print($"Team A Count {gameMatch.TeamA.Count} Weight: {WeightHelpers.ComputeTeamWeight(gameMatch.TeamA)}");
        Debug.Print($"Team B Count {gameMatch.TeamB.Count} Weight: {WeightHelpers.ComputeTeamWeight(gameMatch.TeamB)}");
        Debug.Print($"Waiting Team Count {gameMatch.Waiting.Count} Weight: {WeightHelpers.ComputeTeamWeight(gameMatch.Waiting)}");
        Debug.Print("--------------------------------------------");
    }

    private static Vector2 ClanGroupRescaledVector(float scaler, ClanGroup clanGroup)
    {
        return new Vector2(clanGroup.Size * scaler, clanGroup.Weight());
    }

    private static Vector2 ClanGroupsRescaledVector(float scaler, List<ClanGroup> clanGroups)
    {
        return new Vector2(clanGroups.Sum(c => c.Size) * scaler, WeightHelpers.ClanGroupsWeightSum(clanGroups));
    }

    private static float Vector2Angles(Vector2 v)
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
