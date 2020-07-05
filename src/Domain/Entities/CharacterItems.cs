using System.Collections.Generic;

namespace Crpg.Domain.Entities
{
    public class CharacterItems
    {
        public int? HeadItemId { get; set; }
        public int? CapeItemId { get; set; }
        public int? BodyItemId { get; set; }
        public int? HandItemId { get; set; }
        public int? LegItemId { get; set; }
        public int? HorseHarnessItemId { get; set; }
        public int? HorseItemId { get; set; }
        public int? Weapon1ItemId { get; set; }
        public int? Weapon2ItemId { get; set; }
        public int? Weapon3ItemId { get; set; }
        public int? Weapon4ItemId { get; set; }
        public bool AutoRepair { get; set; }

        public Item? HeadItem { get; set; }
        public Item? CapeItem { get; set; }
        public Item? BodyItem { get; set; }
        public Item? HandItem { get; set; }
        public Item? LegItem { get; set; }
        public Item? HorseHarnessItem { get; set; }
        public Item? HorseItem { get; set; }
        public Item? Weapon1Item { get; set; }
        public Item? Weapon2Item { get; set; }
        public Item? Weapon3Item { get; set; }
        public Item? Weapon4Item { get; set; }

        public IEnumerable<(ItemSlot slot, Item item)> ItemSlotPairs()
        {
            if (HeadItem != null)
            {
                yield return (ItemSlot.Head, HeadItem);
            }

            if (CapeItem != null)
            {
                yield return (ItemSlot.Cape, CapeItem);
            }

            if (BodyItem != null)
            {
                yield return (ItemSlot.Body, BodyItem);
            }

            if (HandItem != null)
            {
                yield return (ItemSlot.Hand, HandItem);
            }

            if (LegItem != null)
            {
                yield return (ItemSlot.Leg, LegItem);
            }

            if (HorseHarnessItem != null)
            {
                yield return (ItemSlot.HorseHarness, HorseHarnessItem);
            }

            if (HorseItem != null)
            {
                yield return (ItemSlot.Horse, HorseItem);
            }

            if (Weapon1Item != null)
            {
                yield return (ItemSlot.Weapon1, Weapon1Item);
            }

            if (Weapon2Item != null)
            {
                yield return (ItemSlot.Weapon2, Weapon2Item);
            }

            if (Weapon3Item != null)
            {
                yield return (ItemSlot.Weapon3, Weapon3Item);
            }

            if (Weapon4Item != null)
            {
                yield return (ItemSlot.Weapon4, Weapon4Item);
            }
        }
    }
}
