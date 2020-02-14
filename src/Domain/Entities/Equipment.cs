using System.Collections.Generic;

namespace Trpg.Domain.Entities
{
    public class Equipment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public EquipmentType Type { get; set; }
        // TODO: Looming type
        // TODO: armor, weight, difficulty, speed rating, length, swing, thrust, slots

        public List<UserEquipment> UserEquipments { get; set; }
    }
}