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

import { useSearchDebounced } from './use-search-debounce';

describe('searchModel', () => {
  it('empty query', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {},
    }));

    const { searchModel } = useSearchDebounced();

    expect(searchModel.value).toEqual('');
  });

  it('with query', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {
        search: 'mlp',
      },
    }));

    const { searchModel } = useSearchDebounced();

    expect(searchModel.value).toEqual('mlp');
  });

  it('change', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {
        search: 'mlp',
      },
    }));

    const { searchModel } = useSearchDebounced();

    searchModel.value = 'mlp the best';

    expect(mockedPush).toBeCalledWith({
      query: {
        search: 'mlp the best',
      },
    });
  });
});
