import UserItem from '@/models/user-item';
import ItemSlot from '@/models/item-slot';

export default interface EquippedItem {
  slot: ItemSlot;
  userItem: UserItem;
}
