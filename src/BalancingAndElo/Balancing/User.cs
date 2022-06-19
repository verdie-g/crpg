using System;
using System.Collections.Generic;

namespace Crpg.BalancingAndRating.Balancing
{
    public class User 
    {
        public int Id { get; set; }

        /// <summary>
        /// The platform (e.g. Steam) used to play Bannerlord.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        public int Elo { get; set; }
        public ClanMember? ClanMembership { get; set; }
    }
}
