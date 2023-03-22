import { type FiltersModel } from '@/models/item-search';
import { Item, ItemType, WeaponClass, type ItemFlat } from '@/models/item';

import { createItemIndex } from '@/services/item-search-service/indexator';
import {
  filterItemsByType,
  filterItemsByWeaponClass,
  generateEmptyFiltersModel,
  getAggregationBy,
  getAggregationsConfig,
  getVisibleAggregationsConfig,
  getScopeAggregations,
} from '@/services/item-search-service';
import { pick } from '@/utils/object';
import { getWeaponClassesByItemType } from '@/services/item-service';

export const useItemsFilter = (items: Item[]) => {
  const route = useRoute();
  const router = useRouter();

  const itemTypeModel = computed({
    set(val: ItemType) {
      const weaponClasses = getWeaponClassesByItemType(val);

      router.push({
        query: {
          type: val,
          ...(weaponClasses.length !== 0 && {
            weaponClass: weaponClasses[0],
          }),
        },
      });
    },

    get() {
      return (route.query?.type as ItemType) || ItemType.OneHandedWeapon;
    },
  });

  const weaponClassModel = computed({
    set(val: WeaponClass | null) {
      router.push({
        query: {
          type: itemTypeModel.value,
          weaponClass: val === null ? undefined : val,
        },
      });
    },

    get() {
      if (route.query?.weaponClass) return route.query.weaponClass as WeaponClass;

      const weaponClasses = getWeaponClassesByItemType(itemTypeModel.value);
      return weaponClasses.length !== 0 ? weaponClasses[0] : null;
    },
  });

  const filterModel = computed({
    set(val: FiltersModel<string[] | number[]>) {
      router.push({
        query: {
          ...route.query,
          // @ts-ignore TODO:
          filter: val,
        },
      });
    },

    get() {
      return {
        ...generateEmptyFiltersModel(aggregationsConfig.value),
        // @ts-ignore TODO:
        ...('filter' in route.query && (route.query.filter as FiltersModel)),
      };
    },
  });

  const updateFilter = (key: keyof ItemFlat, val: string | string[] | number | number[]) => {
    router.push({
      query: {
        ...route.query,
        // @ts-ignore TODO:
        filter: { ...filterModel.value, [key]: val },
      },
    });
  };

  const resetFilters = () => {
    router.push({
      query: {
        ...pick(route.query, [
          'type',
          'weaponClass',
          'sort',
          'perPage',

          // TODO: need to be purged?
          'compareMode',
          'compareList',
        ]), // TODO: keys to config?
      },
    });
  };

  const flatItems = computed((): ItemFlat[] => createItemIndex(items, true));

  const aggregationsConfig = computed(() =>
    getAggregationsConfig(itemTypeModel.value, weaponClassModel.value)
  );

  const aggregationsConfigVisible = computed(() =>
    getVisibleAggregationsConfig(aggregationsConfig.value)
  );

  const filteredByTypeFlatItems = computed((): ItemFlat[] =>
    filterItemsByType(flatItems.value, itemTypeModel.value)
  );

  const filteredByClassFlatItems = computed((): ItemFlat[] =>
    filterItemsByWeaponClass(filteredByTypeFlatItems.value, weaponClassModel.value)
  );

  const aggregationByType = computed(() => getAggregationBy(flatItems.value, 'type'));
  const aggregationByClass = computed(() =>
    getAggregationBy(filteredByTypeFlatItems.value, 'weaponClass')
  );

  // needed for the range slider to work normal.
  const scopeAggregations = computed(() =>
    getScopeAggregations(filteredByClassFlatItems.value, aggregationsConfig.value)
  );

  return {
    itemTypeModel,
    weaponClassModel,
    filterModel,
    updateFilter,

    resetFilters,

    aggregationsConfig,
    aggregationsConfigVisible,

    filteredByTypeFlatItems,
    filteredByClassFlatItems,

    aggregationByType,
    aggregationByClass,
    scopeAggregations,
  };
};
