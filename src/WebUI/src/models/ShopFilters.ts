import ItemType from '@/models/item-type';
import Culture from '@/models/culture';

export default class ShopFilters {
  type: ItemType | null;
  culture: Culture | null;
  showOwned: boolean;
}
