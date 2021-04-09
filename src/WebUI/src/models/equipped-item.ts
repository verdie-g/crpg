import Item from '@/models/item';
import ItemSlot from '@/models/item-slot';

export default interface EquippedItem {
  item: Item;
  slot: ItemSlot;
}
