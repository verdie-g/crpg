using Trpg.Domain.Common;

namespace Trpg.Domain.Entities
{
    public class Character : AuditableEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public int Experience { get; set; }

        public int Level { get; set; }

        // TODO: attribute + skills + weapon points
        public int? HeadEquipmentId { get; set; }
        public int? BodyEquipmentId { get; set; }
        public int? LegsEquipmentId { get; set; }
        public int? GlovesEquipmentId { get; set; }
        public int? Weapon1EquipmentId { get; set; }
        public int? Weapon2EquipmentId { get; set; }
        public int? Weapon3EquipmentId { get; set; }
        public int? Weapon4EquipmentId { get; set; }

        public User User { get; set; }
        public Equipment HeadEquipment { get; set; }
        public Equipment BodyEquipment { get; set; }
        public Equipment LegsEquipment { get; set; }
        public Equipment GlovesEquipment { get; set; }
        public Equipment Weapon1Equipment { get; set; }
        public Equipment Weapon2Equipment { get; set; }
        public Equipment Weapon3Equipment { get; set; }
        public Equipment Weapon4Equipment { get; set; }
    }
}