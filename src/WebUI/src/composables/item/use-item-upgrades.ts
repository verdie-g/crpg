import { type ItemFlat } from '@/models/item';
import { type AggregationConfig } from '@/models/item-search';
import { getItemUpgrades, getRelativeEntries } from '@/services/item-service';
import { createItemIndex } from '@/services/item-search-service/indexator';
import { clamp } from '@/utils/math';
import { useUserStore } from '@/stores/user';

export const useItemUpgrades = (
  item: ItemFlat,
  cols: AggregationConfig,
  excludeBaseItem = false
) => {
  const userStore = useUserStore();

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

  const currentItem = computed(() => itemUpgrades.value.find(iu => iu.id === item.id));

  const baseItem = computed(() => itemUpgrades.value[0]);

  const nextItem = computed(
    () => itemUpgrades.value[clamp(itemUpgrades.value.findIndex(iu => iu.id === item.id) + 1, 0, 3)]
  );

  const validation = computed(() => ({
    points: userStore.user!.heirloomPoints! > 0,
    maxRank: item.rank !== 3,
    exist: !userStore.userItems.some(
      ui => ui.item.baseId === nextItem.value?.baseId && ui.item.rank === nextItem.value?.rank
    ),
  }));

  const canUpgrade = computed(() => validation.value.points && validation.value.maxRank);

  return {
    itemUpgrades,
    relativeEntries,
    currentItem,
    baseItem,
    nextItem,
    validation,
    canUpgrade,
  };
};
