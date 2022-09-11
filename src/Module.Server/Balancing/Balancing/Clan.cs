using System.Collections.Generic;

namespace Crpg.Module.Balancing
{
    public class Clan 
    {
        public int Id { get; set; }

        /// <summary>
        /// Short name of the clan.
        /// </summary>
        /// <summary>
        /// Full name of the clan.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        public IList<ClanMember> Members { get; set; } = new List<ClanMember>();
    }
}
