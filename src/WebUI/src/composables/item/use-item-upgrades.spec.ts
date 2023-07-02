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

import { useItemUpgrades } from './use-item-upgrades';

beforeEach(() => {
  userStore.$reset();
  userStore.$patch({ user: { gold: 1000 } });
});

it.todo('TODO:', () => {});
