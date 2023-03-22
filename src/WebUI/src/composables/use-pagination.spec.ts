const mockedPush = vi.fn();
const mockedUseRoute = vi.fn();
vi.mock('vue-router', () => ({
  useRoute: mockedUseRoute,
  useRouter: vi.fn().mockImplementation(() => ({
    push: mockedPush,
  })),
}));

import { usePagination } from './use-pagination';

describe('page model', () => {
  it('empty query', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {},
    }));

    const { pageModel } = usePagination();

    expect(pageModel.value).toEqual(1);
  });

  it('with query', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {
        page: 123,
      },
    }));

    const { pageModel } = usePagination();

    expect(pageModel.value).toEqual(123);
  });

  it('change', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {
        type: 'someType',
        sort: 'price_asc',
      },
    }));

    const { pageModel } = usePagination();

    pageModel.value = 123;

    expect(mockedPush).toBeCalledWith({
      query: {
        sort: 'price_asc',
        type: 'someType',
        page: 123,
      },
    });

    pageModel.value = 1;

    expect(mockedPush).toBeCalledWith({
      query: {
        sort: 'price_asc',
        type: 'someType',
      },
    });
  });
});

describe('per page model', () => {
  it('empty query', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {},
    }));

    const { perPageModel } = usePagination();

    expect(perPageModel.value).toEqual(10);
  });

  it('with query', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {
        perPage: 15,
      },
    }));

    const { perPageModel } = usePagination();

    expect(perPageModel.value).toEqual(15);
  });

  it('change', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {
        type: 'someType',
        sort: 'price_asc',
      },
    }));

    const { perPageModel } = usePagination();

    perPageModel.value = 25;

    expect(mockedPush).toBeCalledWith({
      query: {
        sort: 'price_asc',
        type: 'someType',
        perPage: 25,
      },
    });

    perPageModel.value = 10;

    expect(mockedPush).toBeCalledWith({
      query: {
        sort: 'price_asc',
        type: 'someType',
      },
    });
  });
});

it('per page config', () => {
  const { perPageConfig } = usePagination();

  expect(perPageConfig).toEqual([10, 15, 20]); // TODO:
});
