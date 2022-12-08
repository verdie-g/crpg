using System;
using System.Collections.Generic;
using Crpg.Module.Api.Models.Users;

namespace Crpg.Module.Balancing
{
    internal class GameMatch
    {
        internal List<CrpgUser> TeamA { get; set; } = new List<CrpgUser>();
        internal List<CrpgUser> TeamB { get; set; } = new List<CrpgUser>();
        internal List<CrpgUser> Waiting { get; set; } = new List<CrpgUser>();
    }
    internal class ClanGroupsGameMatch
    {
        internal List<ClanGroup> TeamA { get; set; } = new List<ClanGroup>();
        internal List<ClanGroup> TeamB { get; set; } = new List<ClanGroup>();
        internal List<ClanGroup> Waiting { get; set; } = new List<ClanGroup>();
    }
}
