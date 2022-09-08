﻿using System;
using System.Collections.Generic;
using System.Linq;
using Crpg.BalancingAndRating.Balancing;



namespace Crpg.BalancingAndRating.Balancing
{
    public static class MatchBalancingHelpers
    {
        public static List<ClanGroup> ConvertUserListToClanGroups(List<User> userList)
        {
            List<int> isClanGroupCreated = new();
            List<ClanGroup> clanGroups = new();
           // List<User> noClan = userList.Select()

            foreach (User player in userList.OrderByDescending(u => u.ClanMembership?.ClanId ?? 0))
            {
                if (player.ClanMembership == null)
                {
                    ClanGroup clangroup = new();
                    clangroup.Add(player);
                    clanGroups.Add(clangroup);
                }
                else
                {
                    if (isClanGroupCreated.Contains(player.ClanMembership.ClanId))
                    {
                        clanGroups.Find(clangroup => clangroup.ClanID() == player.ClanMembership.ClanId)!.Add(player);
                    }
                    else
                    {
                        ClanGroup clangroup = new();
                        clangroup.Add(player);
                        isClanGroupCreated.Add(player.ClanMembership.ClanId);
                        clanGroups.Add(clangroup);
                    }
                }
            }

            return clanGroups;
        }

        public static List<User> ConvertClanGroupsToUserList(List<ClanGroup> clanGroups)
        {
            List<User> users = new();

            foreach (ClanGroup clanGroup in clanGroups)
            {
                users.AddRange(clanGroup.MemberList());
            }

            return users;
        }
        public static int TeamSizeDifference(GameMatch gameMatch)
        {
            return gameMatch.TeamA.Count - gameMatch.TeamB.Count;
        }

        public static ClanGroupsGameMatch ConvertGameMatchToClanGroupsGameMatchList(GameMatch gameMatch)
        {
            ClanGroupsGameMatch clanGroupsGameMatch = new();
            clanGroupsGameMatch.TeamA = ConvertUserListToClanGroups(gameMatch.TeamA);
            clanGroupsGameMatch.TeamB = ConvertUserListToClanGroups(gameMatch.TeamB);
            clanGroupsGameMatch.Waiting = ConvertUserListToClanGroups(gameMatch.Waiting);
            return clanGroupsGameMatch;
        }

        public static GameMatch ConvertClanGroupsGameMatchToGameMatchList(ClanGroupsGameMatch clanGroupsgameMatch)
        {
            GameMatch gameMatch = new();
            gameMatch.TeamA = ConvertClanGroupsToUserList(clanGroupsgameMatch.TeamA);
            gameMatch.TeamB = ConvertClanGroupsToUserList(clanGroupsgameMatch.TeamB);
            gameMatch.Waiting = ConvertClanGroupsToUserList(clanGroupsgameMatch.Waiting);
            return gameMatch;
        }
        public static int UserCountInClanGroupList(List<ClanGroup> clanGroups)
        {
            int count = 0;
            foreach (ClanGroup clanGroup in clanGroups)
            {
                count += clanGroup.Size();
            }
            return count;
        }
        public static ClanGroup FindClanGroupByRatingBelowSize(List<ClanGroup> clanGroups, double desiredLevel, double maxSize)
        {
         int index=clanGroups.BinarySearch()
        }
        public static List<ClanGroup> FindSuitableSwap(ClanGroup weakClanGroup, List<ClanGroup> strongTeam ,double diff)
        {
            List<ClanGroup> selectedClanGroups = new();
            sele
            return selectedClanGroups;
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

        public static PartitioningResult<T> Heuristic<T>(T[] elements, double[] weights, int numParts, bool preSorted = false)
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

            var partitions = new PriorityQueue<PartitionNode<T> , double>(weights.Length);
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
            public PartitionNode(List<T>[] partition, double[] sizes)
            {
                Partition = partition;
                Sizes = sizes;
            }

            public List<T>[] Partition { get; }
            public double[] Sizes { get; }
        }

    }
}

