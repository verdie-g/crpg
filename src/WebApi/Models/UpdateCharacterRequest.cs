namespace Trpg.WebApi.Models
{
    public class UpdateCharacterRequest
    {
        public string Name { get; set; }
        public int? HeadEquipmentId { get; set; }
        public int? BodyEquipmentId { get; set; }
        public int? LegsEquipmentId { get; set; }
        public int? GlovesEquipmentId { get; set; }
        public int? WeaponEquipmentId { get; set; }
    }
}