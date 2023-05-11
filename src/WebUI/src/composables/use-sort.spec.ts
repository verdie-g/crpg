const { mockedPush, mockedUseRoute } = vi.hoisted(() => ({
  mockedPush: vi.fn(),
  mockedUseRoute: vi.fn(),
}));
vi.mock('vue-router', () => ({
  useRoute: mockedUseRoute,
  useRouter: vi.fn().mockImplementation(() => ({
    push: mockedPush,
  })),
}));

import { useSort } from './use-sort';

const SORT_KEY = 'createdAt';

it('empty query', () => {
  mockedUseRoute.mockImplementation(() => ({
    query: {},
  }));

  const { sort } = useSort(SORT_KEY);

  expect(sort.value).toEqual('asc');
});

it('with query', () => {
  mockedUseRoute.mockImplementation(() => ({
    query: {
      sort: 'createdAt_desc',
    },
  }));

  const { sort } = useSort(SORT_KEY);

  expect(sort.value).toEqual('desc');
});

it('toggle asc->desc', () => {
  mockedUseRoute.mockImplementation(() => ({
    query: {
      sort: 'createdAt_asc',
    },
  }));

  const { toggleSort } = useSort(SORT_KEY);

  toggleSort();

  expect(mockedPush).toBeCalledWith({
    query: {
      sort: 'createdAt_desc',
    },
  });
});

it('toggle desc->asc', () => {
  mockedUseRoute.mockImplementation(() => ({
    query: {
      sort: 'createdAt_desc',
    },
  }));

  const { toggleSort } = useSort(SORT_KEY);

  toggleSort();

  expect(mockedPush).toBeCalledWith({
    query: {
      sort: undefined,
    },
  });
});
