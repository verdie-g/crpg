using System.Collections.Generic;

namespace Trpg.Domain.Entities
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public ItemType Type { get; set; }
        // TODO: Looming type
        // TODO: armor, weight, difficulty, speed rating, length, swing, thrust, slots

        public List<UserItem> UserItems { get; set; }
    }
}