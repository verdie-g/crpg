namespace Crpg.Module.Balancing
{
    /// <summary>
    /// Represents a clan membership for a user.
    /// </summary>
    public class ClanMember
    {
        public int UserId { get; set; }
        public int ClanId { get; set; }
        public ClanMemberRole Role { get; set; }

        public User? User { get; set; }
        public Clan? Clan { get; set; }
    }
}
