import { type ItemFlat } from '@/models/item';
import { type FiltersModel } from '@/models/item-search';
import { excludeRangeFilters, applyRangeFilters } from '@/services/item-search-service/helpers';

it.each<[FiltersModel<number[] | string[]>, FiltersModel<number[] | string[]>]>([
  [{ price: [0, 1] }, {}],
  [{ weaponClass: ['test'] }, { weaponClass: ['test'] }],
])('excludeRangeFilters - model: %j', (filtersModel, expectation) => {
  expect(excludeRangeFilters(filtersModel)).toEqual(expectation);
});

it.each<[FiltersModel<number[] | string[]>, boolean]>([
  [{ price: [999, 1001], tier: [6, 9] }, true],
  [{ price: [999, 1001], tier: [9, 9] }, false],
  [{ price: [1001, 1002], tier: [9, 9] }, false],
  [{ price: [999, 1002], tier: [] }, true],
  [{ price: [0, 1000], weaponClass: ['oneHandedSword'] }, true],
])('applyRangeFilters - filterModel: %j', (filterModel, expectation) => {
  expect(applyRangeFilters({ price: 1000, tier: 8.45 } as ItemFlat, filterModel)).toEqual(
    expectation
  );
});
