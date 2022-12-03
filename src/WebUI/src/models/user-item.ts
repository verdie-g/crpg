import Item from '@/models/item';

export default interface UserItem {
  id: number;
  baseItem: Item;
  rank: number;
  createdAt: Date;
}
