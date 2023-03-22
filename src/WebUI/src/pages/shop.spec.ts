import { createTestingPinia } from '@pinia/testing';
import { ItemType, WeaponClass } from '@/models/item';
import { type UserItem } from '@/models/user';

import { mountWithRouter } from '@/__test__/unit/utils';
import mockItems from '@/__mocks__/items.json';

const mockGetItems = vi.fn().mockResolvedValue(mockItems);
const mockGetCompareItemsResult = vi.fn().mockReturnValue({});
vi.mock('@/services/item-service', () => ({
  getItems: mockGetItems,
  getCompareItemsResult: mockGetCompareItemsResult,
}));

const mockGetSearchResult = vi.fn().mockReturnValue({
  data: {
    aggregations: {},
    items: [],
  },
  pagination: {
    page: 1,
    per_page: 1,
    total: 1,
  },
});
vi.mock('@/services/item-search-service', () => ({
  getSearchResult: mockGetSearchResult,
}));

const aggregationConfig = { handling: {}, price: {} };
const filterModel = { handling: [], price: [] };
const mockAggregationsConfig = vi.fn().mockReturnValue(computed(() => aggregationConfig))();
const mockResetFilter = vi.fn();
const mockUseItemsFilter = vi.fn().mockImplementation(() => ({
  itemTypeModel: computed(() => ItemType.OneHandedWeapon),
  weaponClassModel: computed(() => WeaponClass.OneHandedSword),
  nameModel: computed(() => ''),
  filterModel: computed(() => filterModel),
  updateFilterModel: vi.fn(),
  resetFilters: mockResetFilter,
  filteredByClassFlatItems: computed(() => []),
  aggregationsConfig: mockAggregationsConfig,
  aggregationsConfigVisible: computed(() => ({})),
  aggregationByType: computed(() => ({ data: { buckets: [] } })),
  aggregationByClass: computed(() => ({ data: { buckets: [] } })),
  scopeAggregations: computed(() => ({})),
}));
vi.mock('@/composables/shop/use-filters', () => ({
  useItemsFilter: mockUseItemsFilter,
}));

const page = 1;
const perPage = 15;
const perPageConfig = [10, 15, 20];
const mockUsePagination = vi.fn().mockImplementation(() => ({
  pageModel: computed(() => page),
  perPageModel: computed(() => perPage),
  perPageConfig: computed(() => perPageConfig),
}));
vi.mock('@/composables/use-pagination', () => ({
  usePagination: mockUsePagination,
}));

const sortingConfig = {
  price_desc: {
    field: 'price_desc',
    order: 'desc',
  },
  price_asc: {
    field: 'price_asc',
    order: 'asc',
  },
};
const mockUseItemsSort = vi.fn().mockImplementation(() => ({
  sortingModel: computed(() => 'price_desc'),
  sortingConfig: computed(() => sortingConfig),
}));
vi.mock('@/composables/shop/use-sort', () => ({
  useItemsSort: mockUseItemsSort,
}));

const mockToggleCompareMode = vi.fn();
const mockToggleToCompareList = vi.fn();
const mockUseItemsCompare = vi.fn().mockImplementation(() => ({
  compareMode: computed(() => false),
  toggleCompareMode: mockToggleCompareMode,
  compareList: computed(() => []),
  toggleToCompareList: mockToggleToCompareList,
}));
vi.mock('@/composables/shop/use-compare', () => ({
  useItemsCompare: mockUseItemsCompare,
}));

import { useUserStore } from '@/stores/user';
import Page from './shop.vue';

const userStore = useUserStore(createTestingPinia());

const routes = [
  {
    name: 'shop',
    path: '/shop',
    component: Page,
  },
];
const route = {
  name: 'shop',
  query: {},
};
const mountOptions = {
  global: {
    stubs: {
      ShopTypeSelect: true,
      ShopGridFilters: true,
      ShopGridItem: true,
      OPagination: true,
    },
  },
};

beforeEach(() => {
  userStore.$reset();
});

it('default state - empty query string', async () => {
  await mountWithRouter(mountOptions, routes, route);

  expect(mockGetItems).toBeCalled();
  expect(userStore.fetchUserItems).toBeCalled();

  expect(mockUseItemsFilter).toBeCalledWith(mockItems);
  expect(mockUseItemsSort).toBeCalledWith(mockAggregationsConfig);
  expect(mockUsePagination).toBeCalled();
  expect(mockUseItemsCompare).toBeCalled();

  expect(mockGetSearchResult).toBeCalledWith({
    items: [],
    aggregationConfig: aggregationConfig,
    sortingConfig: sortingConfig,
    sort: 'price_desc',
    page: page,
    perPage: perPage,
    query: '',
    filter: filterModel,
  });
  expect(mockGetCompareItemsResult).not.toBeCalled();
});

it('Reset filters', async () => {
  const { wrapper } = await mountWithRouter(mountOptions, routes, route);

  await wrapper.find('[data-aq-shop-handler="reset-filters"]').trigger('click');

  expect(mockResetFilter).toBeCalled();
});

describe('Toggle compare mode', () => {
  it('nothing to compare, the button is unavailable ;(', async () => {
    const originalMock = mockUseItemsCompare.getMockImplementation()!();
    mockUseItemsCompare.mockImplementation(() => ({
      ...originalMock,
      compareList: computed(() => []),
    }));

    const { wrapper } = await mountWithRouter(mountOptions, routes, route);
    expect(wrapper.find('[data-aq-shop-handler="toggle-compare-mode"]').exists()).toBeFalsy();
  });

  it('there are a couple of items for comparison :)', async () => {
    const originalMock = mockUseItemsCompare.getMockImplementation()!();
    mockUseItemsCompare.mockImplementation(() => ({
      ...originalMock,
      compareList: computed(() => ['1', '2']),
    }));

    const { wrapper } = await mountWithRouter(mountOptions, routes, route);

    await wrapper.find('[data-aq-shop-handler="toggle-compare-mode"]').trigger('click');

    expect(mockToggleCompareMode).toBeCalled();
  });
});

describe('shop item', () => {
  mockGetSearchResult.mockReturnValue({
    data: {
      aggregations: {},
      items: [{ id: '1' }, { id: '2' }],
    },
    pagination: {
      page: 1,
      per_page: 1,
      total: 2,
    },
  });

  it('emit - buy', async () => {
    const { wrapper } = await mountWithRouter(mountOptions, routes, route);

    const itemsComponents = wrapper.findAllComponents({ name: 'ShopGridItem' });

    itemsComponents.at(1)!.vm.$emit('buy');

    expect(userStore.buyItem).toBeCalledWith('2');
  });

  it('emit - select ', async () => {
    const { wrapper } = await mountWithRouter(mountOptions, routes, route);

    const itemsComponents = wrapper.findAllComponents({ name: 'ShopGridItem' });

    itemsComponents.at(0)!.vm.$emit('select');

    expect(mockToggleToCompareList).toBeCalledWith('1');
  });

  it('pass prop - disabled', async () => {
    userStore.userItems = [{ baseItem: { id: '2' } } as UserItem];
    const { wrapper } = await mountWithRouter(mountOptions, routes, route);

    const itemsComponents = wrapper.findAllComponents({ name: 'ShopGridItem' });

    expect(itemsComponents.at(0)!.attributes('disabled')).toEqual('false');
    expect(itemsComponents.at(1)!.attributes('disabled')).toEqual('true');
  });

  it('pass prop - selected', async () => {
    const originalMock = mockUseItemsCompare.getMockImplementation()!();
    mockUseItemsCompare.mockImplementation(() => ({
      ...originalMock,
      compareList: computed(() => ['1']),
    }));

    const { wrapper } = await mountWithRouter(mountOptions, routes, route);

    const itemsComponents = wrapper.findAllComponents({ name: 'ShopGridItem' });

    expect(itemsComponents.at(0)!.attributes('selected')).toEqual('true');
    expect(itemsComponents.at(1)!.attributes('selected')).toEqual('false');
  });

  it('pass prop - colsCount, fields', async () => {
    const originalMock = mockUseItemsFilter.getMockImplementation()!();
    mockUseItemsFilter.mockImplementation(() => ({
      ...originalMock,
      aggregationsConfigVisible: computed(() => ({
        price: {},
        handling: {},
      })),
    }));

    const { wrapper } = await mountWithRouter(mountOptions, routes, route);

    const itemsComponent = wrapper.findComponent({ name: 'ShopGridItem' });

    expect(itemsComponent.attributes('colscount')).toEqual('2');
    expect(itemsComponent.attributes('fields')).toEqual('price,handling');
  });

  describe('pass prop - compareMode, comparedResult', () => {
    it('compareMode:false', async () => {
      const { wrapper } = await mountWithRouter(mountOptions, routes, route);

      const itemComponent = wrapper.findComponent({ name: 'ShopGridItem' });

      expect(itemComponent.attributes('comparemode')).toEqual('false');
      expect(itemComponent.attributes('comparedresult')).not.toBeDefined();
    });

    it('compareMode:true', async () => {
      const originalMock = mockUseItemsCompare.getMockImplementation()!();
      mockUseItemsCompare.mockImplementation(() => ({
        ...originalMock,
        compareMode: computed(() => true),
      }));

      const { wrapper } = await mountWithRouter(mountOptions, routes, route);

      const itemComponent = wrapper.findComponent({ name: 'ShopGridItem' });

      expect(itemComponent.attributes('comparemode')).toEqual('true');
      expect(itemComponent.attributes('comparedresult')).toEqual('[object Object]');
    });
  });
});

describe('pagination', () => {
  it('pass props', async () => {
    mockGetSearchResult.mockReturnValue({
      data: {
        aggregations: {},
        items: [],
      },
      pagination: {
        page: 1,
        per_page: 25,
        total: 12,
      },
    });

    const { wrapper } = await mountWithRouter(mountOptions, routes, route);

    const paginationComponent = wrapper.findComponent({ name: 'OPagination' });

    expect(paginationComponent.attributes('total')).toEqual('12');
    expect(paginationComponent.attributes('perpage')).toEqual('25');
  });
});

it.todo('v-if="isPrimaryUsage in filterModel', () => {});
