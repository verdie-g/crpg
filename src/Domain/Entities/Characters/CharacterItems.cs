using System;
using System.Collections.Generic;
using Crpg.Domain.Entities.Items;

namespace Crpg.Domain.Entities.Characters
{
    /// <summary>
    /// Item set of a character.
    /// </summary>
    public class CharacterItems : ICloneable
    {
        public int? HeadItemId { get; set; }
        public int? ShoulderItemId { get; set; }
        public int? BodyItemId { get; set; }
        public int? HandItemId { get; set; }
        public int? LegItemId { get; set; }
        public int? MountHarnessItemId { get; set; }
        public int? MountItemId { get; set; }
        public int? Weapon1ItemId { get; set; }
        public int? Weapon2ItemId { get; set; }
        public int? Weapon3ItemId { get; set; }
        public int? Weapon4ItemId { get; set; }
        public bool AutoRepair { get; set; }

        public Item? HeadItem { get; set; }
        public Item? ShoulderItem { get; set; }
        public Item? BodyItem { get; set; }
        public Item? HandItem { get; set; }
        public Item? LegItem { get; set; }
        public Item? MountHarnessItem { get; set; }
        public Item? MountItem { get; set; }
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

            if (ShoulderItem != null)
            {
                yield return (ItemSlot.Shoulder, ShoulderItem);
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

            if (MountHarnessItem != null)
            {
                yield return (ItemSlot.MountHarness, MountHarnessItem);
            }

            if (MountItem != null)
            {
                yield return (ItemSlot.Mount, MountItem);
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

        public object Clone()
        {
            return new CharacterItems
            {
                HeadItemId = HeadItemId,
                ShoulderItemId = ShoulderItemId,
                BodyItemId = BodyItemId,
                HandItemId = HandItemId,
                LegItemId = LegItemId,
                MountHarnessItemId = MountHarnessItemId,
                MountItemId = MountItemId,
                Weapon1ItemId = Weapon1ItemId,
                Weapon2ItemId = Weapon2ItemId,
                Weapon3ItemId = Weapon3ItemId,
                Weapon4ItemId = Weapon4ItemId,
                AutoRepair = AutoRepair,
                HeadItem = HeadItem,
                ShoulderItem = ShoulderItem,
                BodyItem = BodyItem,
                HandItem = HandItem,
                LegItem = LegItem,
                MountHarnessItem = MountHarnessItem,
                MountItem = MountItem,
                Weapon1Item = Weapon1Item,
                Weapon2Item = Weapon2Item,
                Weapon3Item = Weapon3Item,
                Weapon4Item = Weapon4Item,
            };
        }
    }
}
