namespace Trpg.Web.Models
{
    public class UpdateCharacterItemsRequest
    {
        public int? HeadItemId { get; set; }
        public int? BodyItemId { get; set; }
        public int? LegsItemId { get; set; }
        public int? GlovesItemId { get; set; }
        public int? Weapon1ItemId { get; set; }
        public int? Weapon2ItemId { get; set; }
        public int? Weapon3ItemId { get; set; }
        public int? Weapon4ItemId { get; set; }
    }
}