import { ItemType, WeaponClass, type Item } from '@/models/item';

const mockedPush = vi.fn();
const mockedUseRoute = vi.fn();
vi.mock('vue-router', () => ({
  useRoute: mockedUseRoute,
  useRouter: vi.fn().mockImplementation(() => ({
    push: mockedPush,
  })),
}));
vi.mock('@vueuse/core', () => ({
  useDebounceFn: vi.fn(fn => fn),
}));

const mockedGetWeaponClassesByItemType = vi.fn().mockReturnValue([]);
vi.mock('@/services/item-service', async () => ({
  ...(await vi.importActual<typeof import('@/services/item-service')>('@/services/item-service')),
  getWeaponClassesByItemType: mockedGetWeaponClassesByItemType,
}));

const mockedGenerateEmptyFiltersModel = vi.fn(obj =>
  Object.keys(obj).reduce((model, key) => {
    model[key] = [];
    return model;
  }, {} as Record<string, any>)
);

const mockedGetAggregationsConfig = vi.fn();
const mockedGetVisibleAggregationsConfig = vi.fn();
const mockedFilterItemsByType = vi.fn();
const mockedFilterItemsByWeaponClass = vi.fn();
const mockedGetAggregationBy = vi.fn((_arr, key) => ({ [key]: {} }));
const mockedGetScopeAggregations = vi.fn((_arr, obj) => obj);

vi.mock('@/services/item-search-service', async () => ({
  ...(await vi.importActual<typeof import('@/services/item-search-service')>(
    '@/services/item-search-service'
  )),
  generateEmptyFiltersModel: mockedGenerateEmptyFiltersModel,
  getAggregationsConfig: mockedGetAggregationsConfig,
  getVisibleAggregationsConfig: mockedGetVisibleAggregationsConfig,
  filterItemsByType: mockedFilterItemsByType,
  filterItemsByWeaponClass: mockedFilterItemsByWeaponClass,
  getAggregationBy: mockedGetAggregationBy,
  getScopeAggregations: mockedGetScopeAggregations,
}));

vi.mock('@/services/item-search-service/indexator', async () => ({
  createItemIndex: vi.fn(val => val),
}));

import { useItemsFilter } from './use-filters';

const items = [] as Item[];

describe('itemType model', () => {
  it('empty query - value should be default', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {},
    }));

    const { itemTypeModel } = useItemsFilter(items);

    expect(itemTypeModel.value).toEqual(ItemType.OneHandedWeapon);
  });

  it('with query', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {
        type: ItemType.TwoHandedWeapon,
      },
    }));

    const { itemTypeModel } = useItemsFilter(items);

    expect(itemTypeModel.value).toEqual(ItemType.TwoHandedWeapon);
  });

  describe('change', () => {
    it('query should be reset', () => {
      mockedUseRoute.mockImplementation(() => ({
        query: {
          page: 1,
          sort: 'some_asc',
          filter: {
            bestPonies: ['TwilightSparkle', 'Fluttershy'],
          },
        },
      }));

      const { itemTypeModel } = useItemsFilter(items);

      itemTypeModel.value = ItemType.BodyArmor;

      expect(mockedPush).toBeCalledWith({
        query: {
          type: ItemType.BodyArmor,
        },
      });
    });

    it('itemType has weaponClasses - by default the first one in the list', () => {
      mockedUseRoute.mockImplementation(() => ({
        query: {},
      }));
      mockedGetWeaponClassesByItemType.mockReturnValue([
        WeaponClass.TwoHandedSword,
        WeaponClass.TwoHandedAxe,
      ]);

      const { itemTypeModel } = useItemsFilter(items);

      itemTypeModel.value = ItemType.TwoHandedWeapon;

      expect(mockedPush).toBeCalledWith({
        query: {
          type: ItemType.TwoHandedWeapon,
          weaponClass: WeaponClass.TwoHandedSword,
        },
      });
    });
  });
});

describe('weaponClass model', () => {
  it('empty query - value should be default - exact', () => {
    mockedGetWeaponClassesByItemType.mockReturnValue([
      WeaponClass.TwoHandedPolearm,
      WeaponClass.OneHandedPolearm,
    ]);
    mockedUseRoute.mockImplementation(() => ({
      query: {
        type: ItemType.Polearm,
      },
    }));

    const { weaponClassModel } = useItemsFilter(items);

    expect(weaponClassModel.value).toEqual(WeaponClass.TwoHandedPolearm);
  });

  it('empty query - value should be default - null', () => {
    mockedGetWeaponClassesByItemType.mockReturnValue([]);
    mockedUseRoute.mockImplementation(() => ({
      query: {
        type: ItemType.Bow,
      },
    }));

    const { weaponClassModel } = useItemsFilter(items);

    expect(weaponClassModel.value).toEqual(null);
  });

  it('with query', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {
        type: ItemType.TwoHandedWeapon,
        weaponClass: WeaponClass.TwoHandedSword,
      },
    }));

    const { weaponClassModel } = useItemsFilter(items);

    expect(weaponClassModel.value).toEqual(WeaponClass.TwoHandedSword);
  });

  describe('change', () => {
    it('query should be reset, except itemType', () => {
      mockedUseRoute.mockImplementation(() => ({
        query: {
          page: 1,
          sort: 'some_asc',
          filter: {
            bestPonies: ['TwilightSparkle', 'Fluttershy'],
          },
          type: ItemType.Polearm,
          WeaponClass: WeaponClass.TwoHandedPolearm,
        },
      }));

      const { weaponClassModel } = useItemsFilter(items);

      weaponClassModel.value = WeaponClass.OneHandedPolearm;

      expect(mockedPush).toBeCalledWith({
        query: {
          type: ItemType.Polearm,
          weaponClass: WeaponClass.OneHandedPolearm,
        },
      });
    });

    it('empty value', () => {
      mockedUseRoute.mockImplementation(() => ({
        query: {
          type: ItemType.Polearm,
          WeaponClass: WeaponClass.TwoHandedPolearm,
        },
      }));

      const { weaponClassModel } = useItemsFilter(items);

      weaponClassModel.value = null;

      expect(mockedPush).toBeCalledWith({
        query: {
          type: ItemType.Polearm,
        },
      });
    });
  });
});

describe('filter model', () => {
  it('empty query & empty aggregations', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {},
    }));

    mockedGetAggregationsConfig.mockReturnValue({});

    const { filterModel } = useItemsFilter(items);

    expect(filterModel.value).toEqual({});

    expect(mockedGenerateEmptyFiltersModel).toBeCalledWith({});
  });

  it('empty query', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {},
    }));

    mockedGetAggregationsConfig.mockReturnValue({
      length: {},
      handling: {},
    });

    const { filterModel } = useItemsFilter(items);

    expect(filterModel.value).toEqual({
      length: [],
      handling: [],
    });

    expect(mockedGenerateEmptyFiltersModel).toBeCalledWith({
      length: {},
      handling: {},
    });
  });

  it('with query', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {
        filter: {
          length: [1, 2],
        },
      },
    }));

    mockedGetAggregationsConfig.mockReturnValue({
      length: {},
      handling: {},
    });

    const { filterModel } = useItemsFilter(items);

    expect(filterModel.value).toEqual({
      length: [1, 2],
      handling: [],
    });

    expect(mockedGenerateEmptyFiltersModel).toBeCalledWith({
      length: {},
      handling: {},
    });
  });

  it('update filter model method', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {
        page: 1,
        sort: 'some_asc',
        filter: {
          length: [1, 2],
        },
      },
    }));

    mockedGetAggregationsConfig.mockReturnValue({});

    const { updateFilter } = useItemsFilter(items);

    updateFilter('length', [1, 5]);

    expect(mockedPush).toBeCalledWith({
      query: {
        page: 1,
        sort: 'some_asc',
        filter: {
          length: [1, 5],
        },
      },
    });
  });

  it('reset filter method', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {
        page: 3,
        sort: 'some_asc',
        perPage: 25,
        type: ItemType.OneHandedWeapon,
        weaponClass: WeaponClass.OneHandedAxe,
        filter: {
          length: [1, 2],
          handling: [1, 5],
        },
        isCompareActive: true,
        compareList: ['1', '2'],
      },
    }));

    mockedGetAggregationsConfig.mockReturnValue({});

    const { resetFilters } = useItemsFilter(items);

    resetFilters();

    expect(mockedPush).toBeCalledWith({
      query: {
        sort: 'some_asc',
        perPage: 25,
        type: ItemType.OneHandedWeapon,
        weaponClass: WeaponClass.OneHandedAxe,
        isCompareActive: true,
        compareList: ['1', '2'],
      },
    });
  });
});

it('aggregations configs', () => {
  mockedUseRoute.mockImplementation(() => ({
    query: {
      type: ItemType.TwoHandedWeapon,
      weaponClass: WeaponClass.TwoHandedMace,
    },
  }));

  mockedGetAggregationsConfig.mockReturnValue({ length: {}, handling: {} });
  mockedGetVisibleAggregationsConfig.mockReturnValue({ length: {}, handling: {} });

  const { aggregationsConfig, aggregationsConfigVisible } = useItemsFilter(items);

  expect(aggregationsConfig.value).toEqual({ length: {}, handling: {} });
  expect(mockedGetAggregationsConfig).toBeCalledWith(
    ItemType.TwoHandedWeapon,
    WeaponClass.TwoHandedMace
  );

  expect(aggregationsConfigVisible.value).toEqual({ length: {}, handling: {} });
  expect(mockedGetVisibleAggregationsConfig).toBeCalledWith({ length: {}, handling: {} });
});

describe('filters & aggregations', () => {
  mockedUseRoute.mockImplementation(() => ({
    query: {
      type: ItemType.TwoHandedWeapon,
      weaponClass: WeaponClass.TwoHandedMace,
    },
  }));

  const items = [{ id: '1' }, { id: '2' }, { id: '3' }] as Item[];
  const filteredByItemTypeItems = [{ id: '1' }, { id: '2' }] as Item[];
  const filteredByWeaponClassItems = [{ id: '1' }] as Item[];

  mockedFilterItemsByType.mockReturnValue(filteredByItemTypeItems);
  mockedFilterItemsByWeaponClass.mockReturnValue(filteredByWeaponClassItems);
  mockedGetAggregationsConfig.mockReturnValue({ length: {}, handling: {} });

  it('filtered by type && weaponClass', () => {
    const { filteredByTypeFlatItems, filteredByClassFlatItems } = useItemsFilter(items);

    expect(filteredByTypeFlatItems.value).toEqual(filteredByItemTypeItems);
    expect(filteredByClassFlatItems.value).toEqual(filteredByWeaponClassItems);

    expect(mockedFilterItemsByType).toBeCalledWith(items, ItemType.TwoHandedWeapon);
    expect(mockedFilterItemsByWeaponClass).toBeCalledWith(
      filteredByItemTypeItems,
      WeaponClass.TwoHandedMace
    );
  });

  it('aggregations', () => {
    const { aggregationByType, aggregationByClass, scopeAggregations } = useItemsFilter(items);

    expect(aggregationByType.value).toEqual({ type: {} });
    expect(mockedGetAggregationBy).nthCalledWith(1, items, 'type');

    expect(aggregationByClass.value).toEqual({ weaponClass: {} });
    expect(mockedGetAggregationBy).nthCalledWith(2, filteredByItemTypeItems, 'weaponClass');
    expect(scopeAggregations.value).toEqual({ length: {}, handling: {} });

    expect(mockedGetScopeAggregations).toBeCalledWith(filteredByWeaponClassItems, {
      length: {},
      handling: {},
    });
  });
});
