import ItemType from '@/models/item-type';
import Culture from '@/models/culture';

export default interface ShopFilters {
  type: ItemType | null;
  culture: Culture | null;
  showOwned: boolean;
  showAffordable: boolean;
  sortBy: string;
  sortDesc: boolean;
  searchQuery: string;
  itemsPerPage: number;
}
