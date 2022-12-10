using System.Numerics;
using Crpg.Module.Api.Models.Users;
using TaleWorlds.Library;

namespace Crpg.Module.Balancing;

internal static class MatchBalancingHelpers
{
    public static List<ClanGroup> SplitUsersIntoClanGroups(List<CrpgUser> users)
    {
        Dictionary<int, ClanGroup> clanGroupCreated = new();
        List<ClanGroup> clanGroups = new();

        foreach (CrpgUser user in users.OrderByDescending(u => u.ClanMembership?.ClanId ?? 0))
        {
            if (user.ClanMembership == null)
            {
                ClanGroup clanGroup = new(null);
                clanGroup.Add(user);
                clanGroups.Add(clanGroup);
            }
            else
            {
                if (clanGroupCreated.TryGetValue(user.ClanMembership.ClanId, out ClanGroup existingClanGroup))
                {
                    existingClanGroup.Add(user);
                }
                else
                {
                    ClanGroup clanGroup = new(user.ClanMembership.ClanId);
                    clanGroup.Add(user);
                    clanGroupCreated.Add(user.ClanMembership.ClanId, clanGroup);
                    clanGroups.Add(clanGroup);
                }
            }
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

    public static ClanGroupsGameMatch ConvertGameMatchToClanGroupsGameMatchList(GameMatch gameMatch)
    {
        return new ClanGroupsGameMatch
        {
            TeamA = SplitUsersIntoClanGroups(gameMatch.TeamA),
            TeamB = SplitUsersIntoClanGroups(gameMatch.TeamB),
            Waiting = SplitUsersIntoClanGroups(gameMatch.Waiting),
        };
    }

    public static List<CrpgUser> FindCrpgUsersToSwap(float targetRating, List<CrpgUser> teamToSelectFrom, float sizeOffset)
    {
        List<CrpgUser> team = teamToSelectFrom.ToList();
        List<CrpgUser> usersToSwap = new();
        Vector2 usersToSwapVector = new(0, 0);
        float targetSizeScaling = targetRating / (1 + sizeOffset); // used to make the targeted vector diagonal
        for (int i = 0; i < teamToSelectFrom.Count; i++)
        {
            CrpgUser bestUserToAdd = team.First();
            Vector2 bestUserToAddVector = new(targetSizeScaling, bestUserToAdd.Character.Rating.Value);
            Vector2 objectiveVector = new(targetSizeScaling * (1 + sizeOffset), targetRating);
            if (objectiveVector.Length() == 0f)
            {
                break;
            }

            foreach (CrpgUser user in team)
            {
                Vector2 userVector = new(targetSizeScaling, user.Character.Rating.Value);

                if ((usersToSwapVector + userVector - objectiveVector).Length() < (usersToSwapVector + bestUserToAddVector - objectiveVector).Length())
                {
                    bestUserToAdd = user;
                    bestUserToAddVector = new(targetSizeScaling, bestUserToAdd.Character.Rating.Value);
                }
            }

            if ((usersToSwapVector + bestUserToAddVector - objectiveVector).Length() < (usersToSwapVector - objectiveVector).Length())
            {
                team.Remove(bestUserToAdd);
                usersToSwap.Add(bestUserToAdd);
                usersToSwapVector = new(usersToSwap.Count * targetSizeScaling, usersToSwap.Sum(u => u.Character.Rating.Value));
            }
            else
            {
                team.Remove(bestUserToAdd);
            }
        }

        return usersToSwap;
    }

    public static List<ClanGroup> FindAClanGroupToSwapUsing(float targetRating, float targetSize, List<ClanGroup> teamToSelectFrom, float sizeOffset, bool useAngle)
    {
        List<ClanGroup> team = teamToSelectFrom.ToList();
        List<ClanGroup> clanGroupsToSwap = new();
        float targetSizeScaling = targetRating / (targetSize + sizeOffset); // used to make the targeted vector diagonal
        Vector2 objectiveVector = new(targetSizeScaling * (targetSize + sizeOffset), targetRating);
        for (int i = 0; i < teamToSelectFrom.Count; i++)
        {
            ClanGroup bestClanGroupToAdd = team.First();
            Vector2 bestClanGroupToAddVector = ClanGroupRescaledVector(targetSizeScaling, bestClanGroupToAdd);
            Vector2 clanGroupsToSwapVector = ClanGroupsRescaledVector(targetSizeScaling, clanGroupsToSwap);

            if (objectiveVector.Length() == 0f)
            {
                break;
            }

            foreach (ClanGroup clanGroup in team)
            {
                Vector2 clanGroupVector = ClanGroupRescaledVector(targetSizeScaling, clanGroup);
                if (useAngle)
                {
                    if (
                        Math.Abs(Vector2Angles(clanGroupVector + clanGroupsToSwapVector) - Vector2Angles(objectiveVector)) < Math.Abs(Vector2Angles(bestClanGroupToAddVector + clanGroupsToSwapVector) - Vector2Angles(objectiveVector)))
                    {
                        bestClanGroupToAdd = clanGroup;
                        bestClanGroupToAddVector = ClanGroupRescaledVector(targetSizeScaling, bestClanGroupToAdd);
                    }
                }
                else
                {
                    if ((clanGroupsToSwapVector + clanGroupVector - objectiveVector).Length() < (clanGroupsToSwapVector + bestClanGroupToAddVector - objectiveVector).Length())
                    {
                        bestClanGroupToAdd = clanGroup;
                        bestClanGroupToAddVector = ClanGroupRescaledVector(targetSizeScaling, bestClanGroupToAdd);
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
        Vector2 vector = new(clanGroup.Size * scaler, clanGroup.RatingPsum());
        return vector;
    }

    private static Vector2 ClanGroupsRescaledVector(float scaler, List<ClanGroup> clanGroups)
    {
        Vector2 vector = new(clanGroups.Sum(c => c.Size) * scaler, RatingHelpers.ClanGroupsPowerSum(clanGroups));
        return vector;
    }

    private static float Vector2Angles(Vector2 v1)
    {
        if (v1.X == 0 && v1.Y == 0)
        {
            return 0;
        }

        return (float)Math.Atan2(v1.Y, v1.X);
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
