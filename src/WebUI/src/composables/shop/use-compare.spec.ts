const mockedPush = vi.fn();
const mockedUseRoute = vi.fn();
vi.mock('vue-router', () => ({
  useRoute: mockedUseRoute,
  useRouter: vi.fn().mockImplementation(() => ({
    push: mockedPush,
  })),
}));

import { useItemsCompare } from './use-compare';

describe('compare mode model', () => {
  it('empty query', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {},
    }));

    const { compareMode } = useItemsCompare();

    expect(compareMode.value).toEqual(false);
  });

  it('with query', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {
        compareMode: true,
      },
    }));

    const { compareMode } = useItemsCompare();

    expect(compareMode.value).toEqual(true);
  });

  it('change', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {
        page: 3,
        sort: 'price_desc',
      },
    }));

    const { compareMode } = useItemsCompare();

    compareMode.value = true;

    expect(mockedPush).toBeCalledWith({
      query: {
        page: 3,
        sort: 'price_desc',
        compareMode: 'true',
      },
    });
  });

  it('toggle compare mode', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {},
    }));

    const { compareMode, toggleCompareMode } = useItemsCompare();

    expect(compareMode.value).toEqual(false);

    toggleCompareMode();

    expect(mockedPush).toBeCalledWith({
      query: {
        compareMode: 'true',
      },
    });
  });
});

describe('compare list', () => {
  it('empty query', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {},
    }));

    const { compareList } = useItemsCompare();

    expect(compareList.value).toEqual([]);
  });

  it('with query', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {
        compareList: ['1', '2', '3'],
      },
    }));

    const { compareList } = useItemsCompare();

    expect(compareList.value).toEqual(['1', '2', '3']);
  });

  it('with query', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {
        compareList: ['1', '2', '3'],
      },
    }));

    const { compareList } = useItemsCompare();

    expect(compareList.value).toEqual(['1', '2', '3']);
  });

  it('change - compareMode should be reset, if length of compareList less than 2', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {
        compareMode: true,
        compareList: ['3', '4'],
      },
    }));

    const { compareList } = useItemsCompare();

    compareList.value = ['3'];

    expect(mockedPush).toBeCalledWith({
      query: {
        compareList: ['3'],
      },
    });
  });

  describe('toggleToCompareList', () => {
    it('add', () => {
      mockedUseRoute.mockImplementation(() => ({
        query: {},
      }));

      const { toggleToCompareList } = useItemsCompare();

      toggleToCompareList('1');

      expect(mockedPush).toBeCalledWith({
        query: {
          compareList: ['1'],
        },
      });
    });

    it('remove', () => {
      mockedUseRoute.mockImplementation(() => ({
        query: {
          compareList: ['1'],
        },
      }));

      const { toggleToCompareList } = useItemsCompare();

      toggleToCompareList('1');

      expect(mockedPush).nthCalledWith(1, {
        query: {
          compareList: [],
        },
      });
    });
  });
});
