using System;
using System.Collections.Generic;

namespace Crpg.BalancingAndRating.Balancing
{
    public class GameMatch
    {
        public List<User> TeamA { get; set; } = new List<User>();
        public List<User> TeamB { get; set; } = new List<User>();
        public List<User> Waiting { get; set; } = new List<User>();
    }
}
