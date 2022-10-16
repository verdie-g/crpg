import { setActivePinia, createPinia } from 'pinia';
import mockUserResponse from '@/__mocks__/user.json';
import { useUserStore } from './user';

vi.mock('@/services/users-service', () => {
  return {
    getUser: vi.fn().mockResolvedValue(mockUserResponse),
  };
});

describe('userStore', () => {
  let store: ReturnType<typeof useUserStore>;

  beforeEach(() => {
    setActivePinia(createPinia());

    store = useUserStore();
  });

  afterEach(() => {
    store.$reset();
  });

  it('references a store', () => {
    expect(store).toBeDefined();
  });

  it('has default state on init', () => {
    expect(store.user).toEqual(null);
  });

  it('actions: fetchUser', async () => {
    await store.fetchUser();

    expect(store.user).toEqual(mockUserResponse);
  });
});
