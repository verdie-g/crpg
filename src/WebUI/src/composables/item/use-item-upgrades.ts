import { type ItemFlat } from '@/models/item';
import { getItemUpgrades, getCompareItemsResult } from '@/services/item-service';
import { createItemIndex } from '@/services/item-search-service/indexator';
import { type AggregationConfig } from '@/models/item-search';

export const useItemUpgrades = (
  item: ItemFlat,
  cols: AggregationConfig,
  excludeBaseItem = false
) => {
  const { state } = useAsyncState(() => getItemUpgrades(item.baseId), []);

  const itemUpgrades = computed((): ItemFlat[] =>
    createItemIndex(
      excludeBaseItem ? state.value.filter(itemUpgrade => itemUpgrade.id !== item.id) : state.value
    )
  );

  const compareItemsResult = computed(() => getCompareItemsResult(itemUpgrades.value, cols));

  return {
    compareItemsResult,
    itemUpgrades,
  };
};
