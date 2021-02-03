import ItemType from '@/models/item-type';
import Culture from '@/models/culture';

export default class ShopFilters {
  types: ItemType[];
  cultures: Culture[];
  showOwned: boolean;
}
