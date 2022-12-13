using System.Numerics;
using Crpg.Module.Api.Models.Users;
using TaleWorlds.Library;

namespace Crpg.Module.Balancing;

internal static class MatchBalancingHelpers
{
    public static GameMatch RegroupClans(GameMatch gameMatch)
    {
        int teamASize = gameMatch.TeamA.Count;
        int teamBSize = gameMatch.TeamB.Count;
        var newGameMatch = GroupTeamsByClan(gameMatch);
        Dictionary<int, (int teamAClanCount, int teamBClanCount)> teamCountForEachClan = new();
        foreach (ClanGroup c in newGameMatch.TeamA)
        {
            if (c.ClanId != null)
            {
                teamCountForEachClan.Add((int)c.ClanId, (c.Size, 0));
            }
        }

        foreach (ClanGroup c in newGameMatch.TeamB)
        {
            (int, int) clanCounts;
            if (c.ClanId == null)
            {
                continue;
            }
            else if (!teamCountForEachClan.TryGetValue((int)c.ClanId, out clanCounts))
            {
                teamCountForEachClan.Add((int)c.ClanId, (0, c.Size));
            }

            clanCounts = (clanCounts.Item1, c.Size);
            teamCountForEachClan[(int)c.ClanId] = clanCounts;
        }

        foreach (ClanGroup c in newGameMatch.Waiting)
        {
            (int, int) clanCounts;
            if (c.ClanId == null)
            {
                if (teamASize < teamBSize)
                {
                    newGameMatch.TeamA.Add(c);
                    teamASize += 1;
                    continue;
                }
                else
                {
                    newGameMatch.TeamB.Add(c);
                    teamBSize += 1;
                    continue;
                }
            }

            if (!teamCountForEachClan.TryGetValue((int)c.ClanId, out clanCounts))
            {
                if (teamASize < teamBSize)
                {
                    newGameMatch.TeamA.Add(c);
                    teamASize += c.Size;
                    continue;
                }
                else
                {
                    newGameMatch.TeamB.Add(c);
                    teamBSize += c.Size;
                    continue;
                }
            }

            if (clanCounts.Item1 > clanCounts.Item2)
            {
                newGameMatch.TeamA.Add(c);
                teamASize += c.Size;
                clanCounts = (clanCounts.Item1 + c.Size, clanCounts.Item2);
                teamCountForEachClan[(int)c.ClanId] = clanCounts;
            }
            else
            {
                newGameMatch.TeamB.Add(c);
                teamBSize += c.Size;
                clanCounts = (clanCounts.Item1, clanCounts.Item2 + c.Size);
                teamCountForEachClan[(int)c.ClanId] = clanCounts;
            }
        }

        newGameMatch.Waiting.Clear();
        var team = newGameMatch.TeamA.ToList(); // foreach does not like pulling the rug under its own feet
        foreach (ClanGroup c in team)
        {
            (int, int) clanCounts;
            if (c.ClanId == null)
            {
                continue;
            }

            if (!teamCountForEachClan.TryGetValue((int)c.ClanId, out clanCounts))
            {
                Debug.Print($"WARNING at this point of gameMatchRegroupClans the clan {c.ClanId} should already be in the dictionary");
                continue;
            }

            if (clanCounts.Item1 > clanCounts.Item2)
            {
                if (clanCounts.Item2 > 0)
                {
                    var clanGroupToMove = newGameMatch.TeamB.Find(clangroup => clangroup.ClanId == c.ClanId);
                    teamASize += clanGroupToMove.Size;
                    teamBSize -= clanGroupToMove.Size;
                    newGameMatch.TeamB.Remove(clanGroupToMove);
                    newGameMatch.TeamA.Add(clanGroupToMove);
                }
            }
            else
            {
                if (clanCounts.Item1 > 0)
                {
                    var clanGroupToMove = newGameMatch.TeamA.Find(clangroup => clangroup.ClanId == c.ClanId);
                    teamBSize += clanGroupToMove.Size;
                    teamASize -= clanGroupToMove.Size;
                    newGameMatch.TeamA.Remove(clanGroupToMove);
                    newGameMatch.TeamB.Add(clanGroupToMove);
                }
            }
        }

        return ClanGroupsGameMatchIntoGameMatch(newGameMatch);
    }

    public static List<ClanGroup> SplitUsersIntoClanGroups(List<CrpgUser> users)
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

    public static List<CrpgUser> JoinClanGroupsIntoUsers(List<ClanGroup> clanGroups)
    {
        List<CrpgUser> users = new();

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

    public static float ClanGroupsRating(List<ClanGroup> clanGroups)
    {
        return clanGroups.Sum(c => c.RatingPsum());
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

    public static List<CrpgUser> FindCrpgUsersToSwap(float targetRating, List<CrpgUser> teamToSelectFrom, float desiredSize, float sizeScaler)
    {
        List<CrpgUser> team = teamToSelectFrom.ToList();
        List<CrpgUser> usersToSwap = new();
        Vector2 usersToSwapVector = new(0, 0);
        for (int i = 0; i < teamToSelectFrom.Count; i++)
        {
            CrpgUser bestUserToAdd = team.First();
            Vector2 bestUserToAddVector = new(sizeScaler, bestUserToAdd.Character.Rating.Value);
            Vector2 objectiveVector = new(sizeScaler * desiredSize, targetRating);
            if (objectiveVector.Length() == 0f)
            {
                break;
            }

            foreach (CrpgUser user in team)
            {
                Vector2 userVector = new(sizeScaler, user.Character.Rating.Value);

                if ((usersToSwapVector + userVector - objectiveVector).Length() < (usersToSwapVector + bestUserToAddVector - objectiveVector).Length())
                {
                    bestUserToAdd = user;
                    bestUserToAddVector = new(sizeScaler, bestUserToAdd.Character.Rating.Value);
                }
            }

            if ((usersToSwapVector + bestUserToAddVector - objectiveVector).Length() < (usersToSwapVector - objectiveVector).Length())
            {
                team.Remove(bestUserToAdd);
                usersToSwap.Add(bestUserToAdd);
                usersToSwapVector = new(usersToSwap.Count * sizeScaler, usersToSwap.Sum(u => u.Character.Rating.Value));
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
    /// <param name="teamToSelectFrom">Team to select the clan groups from.</param>
    /// <param name="useAngle">Use the angle method.</param>
    public static List<ClanGroup> FindAClanGroupToSwapUsing(float targetRating, float targetSize, float sizeScaler,
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

    public static void DumpTeams(GameMatch gameMatch)
    {
        Debug.Print("-----------------------");
        Debug.Print("Team A");
        foreach (CrpgUser u in gameMatch.TeamA)
        {
            Debug.Print($"{u.Character.Name} :  {u.Character.Rating.Value}");
        }

        Debug.Print("-----------------------");
        Debug.Print("Team B");
        foreach (CrpgUser u in gameMatch.TeamB)
        {
            Debug.Print($"{u.Character.Name} :  {u.Character.Rating.Value}");
        }

        Debug.Print("-----------------------");
        Debug.Print("WaitingToJoin");
        foreach (CrpgUser u in gameMatch.Waiting)
        {
            Debug.Print($"{u.Character.Name} :  {u.Character.Rating.Value}");
        }

        Debug.Print("-----------------------");
    }

    public static void DumpTeamsStatus(GameMatch gameMatch)
    {
        Debug.Print("--------------------------------------------");
        Debug.Print($"Team A Count {gameMatch.TeamA.Count} Rating: {RatingHelpers.ComputeTeamRatingPowerSum(gameMatch.TeamA)}");
        Debug.Print($"Team B Count {gameMatch.TeamB.Count} Rating: {RatingHelpers.ComputeTeamRatingPowerSum(gameMatch.TeamB)}");
        Debug.Print($"Waiting Team Count {gameMatch.Waiting.Count} Rating: {RatingHelpers.ComputeTeamRatingPowerSum(gameMatch.Waiting)}");
        Debug.Print("--------------------------------------------");
    }

    private static Vector2 ClanGroupRescaledVector(float scaler, ClanGroup clanGroup)
    {
        return new Vector2(clanGroup.Size * scaler, clanGroup.RatingPsum());
    }

    private static Vector2 ClanGroupsRescaledVector(float scaler, List<ClanGroup> clanGroups)
    {
        return new Vector2(clanGroups.Sum(c => c.Size) * scaler, RatingHelpers.ClanGroupsPowerSum(clanGroups));
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
