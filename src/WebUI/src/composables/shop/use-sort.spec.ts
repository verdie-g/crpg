const mockedPush = vi.fn();
const mockedUseRoute = vi.fn();
vi.mock('vue-router', () => ({
  useRoute: mockedUseRoute,
  useRouter: vi.fn().mockImplementation(() => ({
    push: mockedPush,
  })),
}));

const mockedGetSortingConfig = vi.fn();
vi.mock('@/services/item-search-service', () => ({
  getSortingConfig: mockedGetSortingConfig,
}));

import { useItemsSort } from './use-sort';

describe('sortingModel', () => {
  it('empty query', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {},
    }));
    const { sortingModel } = useItemsSort(computed(() => ({})));

    expect(sortingModel.value).toEqual('price_desc');
  });

  it('with query', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {
        sort: 'price_asc',
      },
    }));

    const { sortingModel } = useItemsSort(computed(() => ({})));

    expect(sortingModel.value).toEqual('price_asc');
  });

  it('change, page should be reset', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {
        page: 3,
        type: 'someType',
        sort: 'price_asc',
      },
    }));

    const { sortingModel } = useItemsSort(computed(() => ({})));

    sortingModel.value = 'price_desc';

    expect(mockedPush).toBeCalledWith({
      query: {
        sort: 'price_desc',
        type: 'someType',
      },
    });
  });
});

it('sortingConfig', () => {
  const mockSortingConfig = {
    price_desc: {
      field: ' price',
      order: 'desc',
    },
    price_asc: {
      field: ' price',
      order: 'asc',
    },
  };

  mockedGetSortingConfig.mockReturnValue(mockSortingConfig);

  const { sortingConfig } = useItemsSort(computed(() => ({})));

  expect(sortingConfig.value).toEqual(mockSortingConfig);
  expect(mockedGetSortingConfig).toBeCalledWith({});
});
