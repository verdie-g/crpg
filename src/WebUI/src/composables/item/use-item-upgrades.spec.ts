import { createTestingPinia } from '@pinia/testing';
import { type ItemFlat } from '@/models/item';
import { useUserStore } from '@/stores/user';

const { mockedGetItemUpgrades, mockedGetRelativeEntries } = vi.hoisted(() => ({
  mockedGetItemUpgrades: vi.fn().mockResolvedValue([]),
  mockedGetRelativeEntries: vi.fn().mockReturnValue({}),
}));
vi.mock('@/services/item-service', () => ({
  getItemUpgrades: mockedGetItemUpgrades,
  getRelativeEntries: mockedGetRelativeEntries,
}));

const userStore = useUserStore(createTestingPinia());

import { useItemUpgrades } from './use-item-upgrades';

beforeEach(() => {
  userStore.$reset();
  userStore.$patch({ user: { gold: 1000 } });
});

it.todo('TODO:', () => {});
