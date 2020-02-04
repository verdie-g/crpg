namespace Trpg.Domain.Entities
{
    public class UserEquipment
    {
        public int UserId { get; set; }
        public int EquipmentId { get; set; }

        public User User { get; set; }
        public Equipment Equipment { get; set; }
    }
}