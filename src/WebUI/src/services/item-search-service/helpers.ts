import { type ItemFlat } from '@/models/item';
import { AggregationView, type FiltersModel } from '@/models/item-search';
import { aggregationsConfig } from './aggregations';
import { roundFLoat } from '@/utils/math';

export const excludeRangeFilters = (filterModel: FiltersModel<string[] | number[]>) => {
  return (Object.keys(filterModel) as [keyof ItemFlat])
    .filter(key => aggregationsConfig[key]!.view !== AggregationView.Range)
    .reduce((obj, key) => {
      obj[key] = filterModel[key] as string[];
      return obj;
    }, {} as FiltersModel<string[]>);
};

const applyRangeFilters = (
  item: ItemFlat,
  filtersModel: FiltersModel<string[] | number[]>
): boolean => {
  let result = true;

  (Object.keys(filtersModel) as [keyof ItemFlat])
    .filter(f => aggregationsConfig[f]!.view === AggregationView.Range)
    .forEach(key => {
      if (result === false) return;

      const values = filtersModel[key] as string[];

      result =
        values.length === 0
          ? true
          : roundFLoat(item[key] as number) >= Number(values[0]) &&
            roundFLoat(item[key] as number) <= Number(values[1]);
    });

  return result;
};

export const applyFilters = (
  item: ItemFlat,
  filtersModel: FiltersModel<string[] | number[]>,
  userItemsIds: string[]
) => {
  let result = applyRangeFilters(item, filtersModel);

  if (userItemsIds.includes(item.id)) {
    result = false;
  }

  return result;
};
