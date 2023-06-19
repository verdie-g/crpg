import { type ItemFlat } from '@/models/item';
import { type AggregationConfig } from '@/models/item-search';
import { getItemUpgrades, getRelativeEntries } from '@/services/item-service';
import { createItemIndex } from '@/services/item-search-service/indexator';

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

  const relativeEntries = computed(() =>
    getRelativeEntries(excludeBaseItem ? item : itemUpgrades.value[0], cols)
  );

  return {
    itemUpgrades,
    relativeEntries,
  };
};
