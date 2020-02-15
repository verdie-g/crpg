namespace Trpg.Web.Models
{
    public class UpdateCharacterEquipmentsRequest
    {
        public int? HeadEquipmentId { get; set; }
        public int? BodyEquipmentId { get; set; }
        public int? LegsEquipmentId { get; set; }
        public int? GlovesEquipmentId { get; set; }
        public int? Weapon1EquipmentId { get; set; }
        public int? Weapon2EquipmentId { get; set; }
        public int? Weapon3EquipmentId { get; set; }
        public int? Weapon4EquipmentId { get; set; }
    }
}