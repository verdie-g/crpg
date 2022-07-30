import ItemSlot from '@/models/item-slot';

export default interface EquippedItemId {
  slot: ItemSlot;
  userItemId: number | null;
}
