using System;
using System.Collections.Generic;

namespace Crpg.Module.Balancing
{
    public class GameMatch
    {
        public List<User> TeamA { get; set; } = new List<User>();
        public List<User> TeamB { get; set; } = new List<User>();
        public List<User> Waiting { get; set; } = new List<User>();
    }
    public class ClanGroupsGameMatch
    {
        public List<ClanGroup> TeamA { get; set; } = new List<ClanGroup>();
        public List<ClanGroup> TeamB { get; set; } = new List<ClanGroup>();
        public List<ClanGroup> Waiting { get; set; } = new List<ClanGroup>();
    }
}
