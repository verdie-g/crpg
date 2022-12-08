using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Crpg.Module.Balancing;
using TaleWorlds.MountAndBlade;
using Crpg.Module.Api.Models.Users;
using TaleWorlds.LinQuick;

namespace Crpg.Module.Balancing
{
    internal static class MatchBalancingHelpers
    {
        internal static List<ClanGroup> ConvertCrpgUserListToClanGroups(List<CrpgUser> userList)
        {
            Dictionary<int, ClanGroup> ClanGroupCreated = new();
            List<int> isClanGroupCreated = new();
            List<ClanGroup> clanGroups = new();  
           // List<CrpgUser> noClan = userList.Select()

            foreach (CrpgUser player in userList.OrderByDescending(u => u.ClanMembership?.ClanId ?? 0))
            {
                if (player.ClanMembership == null)
                {
                    ClanGroup clangroup = new(null);
                    clangroup.Add(player);
                    clanGroups.Add(clangroup);
                }
                else
                {
                    if (ClanGroupCreated.TryGetValue(player.ClanMembership.ClanId, out ClanGroup existingClanGroup))
                    {
                        existingClanGroup.Add(player);
                    }
                    else
                    {
                        ClanGroup clanGroup = new(player.ClanMembership.ClanId);
                        clanGroup.Add(player);
                        ClanGroupCreated.Add(player.ClanMembership.ClanId, clanGroup);
                        clanGroups.Add(clanGroup);
                    }
                }
            }

            return clanGroups;
        }

        internal static List<CrpgUser> ConvertClanGroupsToCrpgUserList(List<ClanGroup> clanGroups)
        {
            List<CrpgUser> users = new();

            foreach (ClanGroup clanGroup in clanGroups)
            {
                users.AddRange(clanGroup.MemberList());
            }

            return users;
        }
        internal static int TeamSizeDifference(GameMatch gameMatch)
        {
            return gameMatch.TeamA.Count - gameMatch.TeamB.Count;
        }
        internal static int ClanGroupsSize(List<ClanGroup> clanGroups)
        {
            return clanGroups.Sum(c => c.Size());
        }
        internal static float ClanGroupsRating(List<ClanGroup> clanGroups)
        {
            return clanGroups.Sum(c => c.RatingPsum());
        }
        internal static ClanGroupsGameMatch ConvertGameMatchToClanGroupsGameMatchList(GameMatch gameMatch)
        {
            ClanGroupsGameMatch clanGroupsGameMatch = new();
            clanGroupsGameMatch.TeamA = ConvertCrpgUserListToClanGroups(gameMatch.TeamA);
            clanGroupsGameMatch.TeamB = ConvertCrpgUserListToClanGroups(gameMatch.TeamB);
            clanGroupsGameMatch.Waiting = ConvertCrpgUserListToClanGroups(gameMatch.Waiting);
            return clanGroupsGameMatch;
        }

        internal static GameMatch ConvertClanGroupsGameMatchToGameMatchList(ClanGroupsGameMatch clanGroupsgameMatch)
        {
            GameMatch gameMatch = new();
            gameMatch.TeamA = ConvertClanGroupsToCrpgUserList(clanGroupsgameMatch.TeamA);
            gameMatch.TeamB = ConvertClanGroupsToCrpgUserList(clanGroupsgameMatch.TeamB);
            gameMatch.Waiting = ConvertClanGroupsToCrpgUserList(clanGroupsgameMatch.Waiting);
            return gameMatch;
        }

        internal static CrpgUser FindACrpgUserToSwap(float targetRating, List<CrpgUser> teamtoSelectFrom)
        {
            List<CrpgUser> team = teamtoSelectFrom;
            CrpgUser bestCrpgUserToSwap = team.First();
            for (int i = 0; i < teamtoSelectFrom.Count; i++)
            {
                foreach (CrpgUser user in team)
                {
                        if (Math.Abs(user.Character.Rating.Value - targetRating) < Math.Abs(bestCrpgUserToSwap.Character.Rating.Value - targetRating))
                        {
                        bestCrpgUserToSwap = user;
                        }
                }
            }
            return bestCrpgUserToSwap;
        }

        internal static List<ClanGroup> FindAClanGroupToSwapUsing(float targetRating, float targetSize, List<ClanGroup> teamtoSelectFrom, float sizeOffset, bool useAngle)
        {
            List<ClanGroup> team = teamtoSelectFrom.ToList();
            List<ClanGroup> clanGroupsToSwap = new List<ClanGroup>();
            float targetSizeScaling = targetRating / (targetSize + sizeOffset); // used to make the targeted vector diagonal
            for (int i = 0; i < teamtoSelectFrom.Count; i++)
            {

                ClanGroup bestClanGroupToAdd = team.First();
                Vector2 bestClanGroupToAddVector = ClanGroupRescaledVector(targetSizeScaling, bestClanGroupToAdd);
                Vector2 clanGroupsToSwapVector = ClanGroupsRescaledVector(targetSizeScaling, clanGroupsToSwap);
                Vector2 objectiveVector = new( targetSizeScaling * (targetSize + sizeOffset - clanGroupsToSwap.Sum( x => x.Size() ) ), targetRating - clanGroupsToSwap.Sum(x => x.RatingPsum() ) );
                if (objectiveVector.Length() == 0f)
                {
                    break;
                }

                foreach (ClanGroup clanGroup in team)
                {
                    bestClanGroupToAddVector = ClanGroupRescaledVector(targetSizeScaling, bestClanGroupToAdd);
                    Vector2 clanGroupVector = ClanGroupRescaledVector(targetSizeScaling, clanGroup);
                    if (useAngle)
                    {
                        if (
                        Math.Abs(Vector2Angles(clanGroupVector) - Vector2Angles(objectiveVector)) < Math.Abs(Vector2Angles(bestClanGroupToAddVector) - Vector2Angles(objectiveVector)))
                        {
                        bestClanGroupToAdd = clanGroup;
                        }
                    }
                    else
                    {
                        if ((clanGroupsToSwapVector + clanGroupVector - objectiveVector).Length() < (clanGroupsToSwapVector + bestClanGroupToAddVector - objectiveVector).Length())
                        {
                            bestClanGroupToAdd = clanGroup;
                        }
                    }
                }
                if ((clanGroupsToSwapVector + bestClanGroupToAddVector - objectiveVector).Length() < (clanGroupsToSwapVector + bestClanGroupToAddVector - -objectiveVector).Length())
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

        internal static Vector2 ClanGroupRescaledVector(float scaler, ClanGroup clanGroup)
        {
            Vector2 vector = new(clanGroup.Size() * scaler, clanGroup.RatingPMean());
            return vector;
        }
        internal static Vector2 ClanGroupsRescaledVector(float scaler, List<ClanGroup> clanGroups)
        {
            Vector2 vector = new(clanGroups.Sum(c => c.Size()) * scaler, RatingHelpers.ClanGroupsPowerSum(clanGroups));
            return vector;
        }

        internal static float Vector2Angles(Vector2 v1)
        {
            if (v1.X==0&& v1.Y==0)
            {
                return 0;
            }
            return (float)Math.Atan2(v1.Y, v1.X);
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

        internal static PartitioningResult<T> Heuristic<T>(T[] elements, double[] weights, int numParts, bool preSorted = false)
        {
            if (numParts <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numParts), $"{numParts} must be positive");
            }

            if (weights.Length == 0)
            {
                return new PartitioningResult<T>(
                    Enumerable.Repeat(new List<T>(), numParts).ToArray(),
                    Enumerable.Repeat(0d, numParts).ToArray());
            }

            var indexSortingMap = Enumerable.Range(0, weights.Length).ToArray();
            if (!preSorted)
            {
                // if elements is not sorted , sort them , but using indexSortingMap to remember their original position.
                Array.Sort(weights, indexSortingMap);
                weights = weights.Reverse().ToArray();
                indexSortingMap = indexSortingMap.Reverse().ToArray();
            }

            var partitions = new PriorityQueue<PartitionNode<T>>();
            // iteration on weights
            for (var i = 0; i < weights.Length; i++)
            {
                // number is current weight
                var number = weights[i];
                // Initialization of the Array of List
                var thisPartition = new List<T>[numParts];
                // initialisation of each list of the array except the last one
                for (var n = 0; n < numParts - 1; n++)
                {
                    thisPartition[n] = new List<T>();
                }
                // last cell is a list that contains the biggest remaining element in the for loop
                thisPartition[numParts - 1] = new List<T> { elements[indexSortingMap[i]] };
                // thisSum is an array of double. The size of the Array is the number of partitions
                var thisSum = new double[numParts];
                // Last cell of the array contains current weight
                thisSum[numParts - 1] = number;
                //this Node has the array of list associated with the array this sum.
                var thisNode = new PartitionNode<T>(thisPartition, thisSum);
                // this enqueue one partition for each element.
                partitions.Enqueue(thisNode, -(float)number);
            }
            // checked this part doing the algo by hand , this witchcraft works.
            for (var i = 0; i < weights.Length - 1; i++)
            {
                var node1 = partitions.Dequeue();
                var node2 = partitions.Dequeue();
                var newPartition = new List<T>[numParts];
                var newSizes = new double[numParts];
                for (var k = 0; k < numParts; k++)
                {
                    newSizes[k] = node1.Sizes[k] + node2.Sizes[numParts - k - 1];
                    node1.Partition[k].AddRange(node2.Partition[numParts - k - 1]);
                    newPartition[k] = node1.Partition[k];
                }

                Array.Sort(newSizes, newPartition);
                var newNode = new PartitionNode<T>(newPartition, newSizes);
                var diff = newSizes[numParts - 1] - newSizes[0];
                partitions.Enqueue(newNode, -(float)diff);
            }

            var node = partitions.Dequeue();
            return new PartitioningResult<T>(node.Partition, node.Sizes);
        }

        private class PartitionNode<T>
        {
            internal PartitionNode(List<T>[] partition, double[] sizes)
            {
                Partition = partition;
                Sizes = sizes;
            }

            internal List<T>[] Partition { get; }
            internal double[] Sizes { get; }
        }

    }
}

