using Crpg.Domain.Entities.Users;

namespace Crpg.Domain.Entities.Items
{
    /// <summary>
    /// Item owned by a user.
    /// </summary>
    public class UserItem
    {
        public int UserId { get; set; }
        public int ItemId { get; set; }

        public User? User { get; set; }
        public Item? Item { get; set; }
    }
}
