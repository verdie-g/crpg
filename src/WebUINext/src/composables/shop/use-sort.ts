import { type ComputedRef } from 'vue';
import { type SortingConfig, type Aggregation } from '@/models/item-search';
import { type ItemFlat } from '@/models/item';
import { omit } from '@/utils/object';
import { getSortingConfig } from '@/services/item-search-service';

const DEFAULT_SORT = 'price_desc';

export const useItemsSort = (
  aggregationsConfig: ComputedRef<Partial<Record<keyof ItemFlat, Aggregation>>>
) => {
  const route = useRoute();
  const router = useRouter();

  const sortingModel = computed({
    set(val: string) {
      router.push({
        query: {
          ...omit(route.query, ['page']), // go to 1st page TODO: keys to cfg
          sort: val,
        },
      });
    },

    get() {
      return (route.query?.sort as string) || DEFAULT_SORT; //  TODO: price_desc by default?
    },
  });

  const sortingConfig = computed(() => getSortingConfig(aggregationsConfig.value));

  // TODO: SPEC
  const getSortingConfigByField = (field: string) => {
    return Object.keys(sortingConfig.value)
      .filter(key => sortingConfig.value[key].field === field)
      .reduce((out, key) => {
        out[key] = sortingConfig.value[key];
        return out;
      }, {} as SortingConfig);
  };

  return {
    sortingModel,
    sortingConfig,
    getSortingConfigByField,
  };
};
