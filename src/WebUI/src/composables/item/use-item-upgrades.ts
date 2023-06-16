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

  const itemUpgrades = computed((): ItemFlat[] => {
    return (
      createItemIndex(
        excludeBaseItem
          ? state.value.filter(itemUpgrade => itemUpgrade.id !== item.id)
          : state.value,
        false
      )
        // TODO: hotfix, avoid duplicate items with multiply weaponClass
        .filter(el => el.weaponClass === item.weaponClass)
    );
  });

  const compareItemsResult = computed(() => getCompareItemsResult(itemUpgrades.value, cols));

  return {
    compareItemsResult,
    itemUpgrades,
  };
};
