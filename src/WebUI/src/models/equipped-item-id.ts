import ItemSlot from '@/models/item-slot';

export default interface EquippedItemId {
  itemId: number | null;
  slot: ItemSlot;
}
