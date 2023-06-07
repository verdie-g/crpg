import itemsjs from 'itemsjs';
import { ItemType, WeaponClass, type ItemFlat } from '@/models/item';
import {
  AggregationView,
  type AggregationConfig,
  type FiltersModel,
  type SortingConfig,
} from '@/models/item-search';
import {
  aggregationsConfig,
  aggregationsKeysByItemType,
  aggregationsKeysByWeaponClass,
} from './aggregations';
import { excludeRangeFilters, applyFilters } from './helpers';

export const generateEmptyFiltersModel = (aggregations: AggregationConfig) => {
  return (Object.keys(aggregations) as [keyof ItemFlat]).reduce((model, aggKey) => {
    // model[aggKey] = []; // alwaysArray? TODO: https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/itemsjs/index.d.ts#L38

    if (aggKey === 'weaponUsage') {
      // @ts-ignore
      model[aggKey] = ['Primary'];
    } else {
      model[aggKey] = [];
    }

    return model;
  }, {} as FiltersModel<string[]>);
};

export const getAggregationsConfig = (
  itemType: ItemType,
  weaponClass: WeaponClass | null
): AggregationConfig => {
  const output: AggregationConfig = {
    // common aggregations
    modId: aggregationsConfig['modId'],
  };

  if (weaponClass !== null && weaponClass in aggregationsKeysByWeaponClass) {
    aggregationsKeysByWeaponClass[weaponClass]!.forEach(aggKey => {
      if (aggKey in aggregationsConfig) {
        output[aggKey] = aggregationsConfig[aggKey]!;
      }
    });
  } else if (itemType in aggregationsKeysByItemType) {
    aggregationsKeysByItemType[itemType]!.forEach(aggKey => {
      if (aggKey in aggregationsConfig) {
        output[aggKey] = aggregationsConfig[aggKey]!;
      }
    });
  }

  return output;
};

export const getVisibleAggregationsConfig = (aggregationsConfig: AggregationConfig) => {
  return (Object.keys(aggregationsConfig) as Array<keyof ItemFlat>)
    .filter(key => aggregationsConfig[key]?.hidden !== true)
    .reduce((obj, key) => ({ ...obj, [key]: aggregationsConfig[key] }), {} as AggregationConfig);
};

export const getSortingConfig = (aggregations: AggregationConfig): SortingConfig => {
  return (Object.keys(aggregations) as Array<keyof ItemFlat>)
    .filter(key => aggregationsConfig[key]?.view === AggregationView.Range)
    .reduce((out, agg) => {
      out[`${agg}_asc`] = {
        field: agg,
        order: 'asc',
      };

      out[`${agg}_desc`] = {
        field: agg,
        order: 'desc',
      };

      return out;
    }, {} as SortingConfig);
};

export const getAggregationBy = (items: ItemFlat[], key: keyof ItemFlat) => {
  const aggregations: Partial<Record<keyof ItemFlat, itemsjs.Aggregation>> = {
    [key]: aggregationsConfig[key],
  };

  const agg = itemsjs(items, {
    aggregations: aggregations as Record<keyof ItemFlat, itemsjs.Aggregation>,
  }).aggregation({
    name: key,
    per_page: 1000,
  });

  agg.data.buckets = sortAggregationBuckets(key, agg.data.buckets);

  return agg;
};

// TODO: unit
export const getScopeAggregations = (items: ItemFlat[], aggregationConfig: AggregationConfig) => {
  const result = itemsjs(items, {
    aggregations: aggregationConfig as Record<keyof ItemFlat, itemsjs.Aggregation>,
  }).search({
    per_page: 1,
  });

  return result.data.aggregations;
};

// TODO: FIXME: SPEC
const sortAggregationBuckets = (aggKey: keyof ItemFlat, buckets: any[]) => {
  switch (aggKey) {
    case 'type':
      return buckets.sort(
        (a, b) =>
          Object.values(ItemType).indexOf(a.key as ItemType) -
          Object.values(ItemType).indexOf(b.key as ItemType)
      );

    case 'weaponClass':
      return buckets.sort(
        (a, b) =>
          Object.values(WeaponClass).indexOf(a.key as WeaponClass) -
          Object.values(WeaponClass).indexOf(b.key as WeaponClass)
      );

    default:
      return buckets;
  }
};

export const getSearchResult = ({
  items,
  userItemsIds,
  aggregationConfig,
  sortingConfig,
  sort,
  page,
  perPage,
  query,
  filter,
}: {
  items: ItemFlat[];
  userItemsIds: string[];
  aggregationConfig: AggregationConfig;
  sortingConfig: SortingConfig;
  sort: string;
  page: number;
  perPage: number;
  query: string;
  filter: FiltersModel<string[] | number[]>;
}) => {
  const result = itemsjs(items, {
    searchableFields: ['name'],
    aggregations: aggregationConfig as Record<keyof ItemFlat, itemsjs.Aggregation>,
    sortings: sortingConfig,
  }).search({
    query,
    per_page: perPage,
    page,
    sort,
    filters: excludeRangeFilters(filter),
    filter: item => applyFilters(item, filter, userItemsIds), // there will be a more beautiful solution when migrating Orama search
  });

  // TODO: FIXME: SPEC
  (Object.keys(result.data.aggregations) as Array<keyof ItemFlat>).forEach(aggKey => {
    result.data.aggregations[aggKey].buckets = sortAggregationBuckets(
      aggKey,
      result.data.aggregations[aggKey].buckets
    );
  });

  return result;
};

export const filterItemsByType = (items: ItemFlat[], type: ItemType) =>
  items.filter(fi => (type === ItemType.Undefined ? true : fi.type === type));

export const filterItemsByWeaponClass = (items: ItemFlat[], weaponClass: WeaponClass | null) =>
  weaponClass === null ? items : items.filter(fi => fi.weaponClass === weaponClass);

export const getBucketValues = <I>(buckets: itemsjs.Buckets<I>): number[] => {
  return buckets.filter(b => b.key !== 'null').map(b => Number(b.key));
};

export const getMinRange = (buckets: number[]): number => {
  if (buckets.length === 0) return 0;
  return Math.floor(Math.min(...buckets));
};

export const getMaxRange = (buckets: number[]): number => {
  if (buckets.length === 0) return 0;
  return Math.ceil(Math.max(...buckets));
};

export const getStepRange = (values: number[]): number => {
  if (values.every(Number.isInteger)) return 1; // Ammo, stackAmount

  const [min, max] = [getMinRange(values), getMaxRange(values)];
  const diff = max - min;

  if ((values.length < 20 && diff < 10) || (values.length > 20 && diff < 5)) {
    return 0.1;
  }

  return 1;
};
