import { createTestingPinia } from '@pinia/testing';
import { type ItemFlat } from '@/models/item';
import { useUserStore } from '@/stores/user';

const { mockedReforgeCostByRank } = vi.hoisted(() => ({
  mockedReforgeCostByRank: {
    0: 0,
    1: 40000,
    2: 90000,
    3: 150000,
  },
}));
vi.mock('@/services/item-service', () => ({
  reforgeCostByRank: mockedReforgeCostByRank,
}));

const userStore = useUserStore(createTestingPinia());

import { useItemReforge } from './use-item-reforge';

beforeEach(() => {
  userStore.$reset();
  userStore.$patch({ user: { gold: 1000 } });
});

const item = {
  rank: 2,
  baseId: 'test-item',
  id: 'test-item-h2',
} as ItemFlat;

it('reforgeCost', () => {
  const { reforgeCost } = useItemReforge(item);

  expect(reforgeCost.value).toEqual(mockedReforgeCostByRank[item.rank]);
});

it('reforgeCostTable - table without 0 item', () => {
  const { reforgeCostTable } = useItemReforge(item);

  expect(reforgeCostTable.value.length).toEqual(3);
});

describe('validation', () => {
  it('not much gold', () => {
    userStore.$patch({ user: { gold: 1000 } });

    const { validation } = useItemReforge(item);

    expect(validation.value.rank).toBeTruthy();
    expect(validation.value.gold).toBeFalsy();
    expect(validation.value.exist).toBeTruthy();
  });

  it('lots of gold', () => {
    userStore.$patch({ user: { gold: 500000 } });

    const { validation } = useItemReforge(item);

    expect(validation.value.gold).toBeTruthy();
  });

  it('rank 0 basic item in the inventory', () => {
    userStore.$patch({
      user: { gold: 500000 },
      userItems: [
        {
          item: {
            baseId: item.baseId,
            rank: 0,
          },
        },
      ],
    });

    const { validation } = useItemReforge(item);

    expect(validation.value.exist).toBeFalsy();
  });

  it('you cannot reforge an ungraded item', () => {
    userStore.$patch({ user: { gold: 500000 } });

    const { validation } = useItemReforge({
      rank: 0,
      baseId: 'test-item',
      id: 'test-item-h2',
    } as ItemFlat);

    expect(validation.value.rank).toBeFalsy();
  });

  it('canReforge', () => {
    userStore.$patch({ user: { gold: 500000 } });

    const { canReforge } = useItemReforge(item);

    expect(canReforge.value).toBeTruthy();
  });
});
