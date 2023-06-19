const mockedPush = vi.fn();
const mockedUseRoute = vi.fn();
vi.mock('vue-router', () => ({
  useRoute: mockedUseRoute,
  useRouter: vi.fn().mockImplementation(() => ({
    push: mockedPush,
  })),
}));

import { useItemsCompare } from './use-compare';

describe('isCompare model', () => {
  it('empty query', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {},
    }));

    const { isCompare } = useItemsCompare();

    expect(isCompare.value).toEqual(false);
  });

  it('with query', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {
        isCompare: true,
      },
    }));

    const { isCompare } = useItemsCompare();

    expect(isCompare.value).toEqual(true);
  });

  it('change', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {
        page: 3,
        sort: 'price_desc',
      },
    }));

    const { isCompare } = useItemsCompare();

    isCompare.value = true;

    expect(mockedPush).toBeCalledWith({
      query: {
        page: 3,
        sort: 'price_desc',
        isCompare: 'true',
      },
    });
  });

  it('toggle compare', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {},
    }));

    const { isCompare, toggleCompare } = useItemsCompare();

    expect(isCompare.value).toEqual(false);

    toggleCompare();

    expect(mockedPush).toBeCalledWith({
      query: {
        isCompare: 'true',
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

  it('change - isCompare should be reset, if length of compareList less than 2', () => {
    mockedUseRoute.mockImplementation(() => ({
      query: {
        isCompare: true,
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
