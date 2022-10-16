import { setActivePinia, createPinia } from 'pinia';
import mockUserResponse from '@/__mocks__/example.json';
import { useUserStore } from './example';

vi.mock('@/services/example', () => {
  return {
    getUser: vi.fn().mockResolvedValue(mockUserResponse),
  };
});

// https://pinia.vuejs.org/cookbook/testing.html#unit-testing-a-store
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
    expect(store.name).toEqual('');
    expect(store.role).toEqual('');
  });

  it('actions: fetch', async () => {
    await store.fetch();
    expect(store.name).toEqual(mockUserResponse.name);
    expect(store.role).toEqual(mockUserResponse.role);
  });

  it('getters: namePlusRole', async () => {
    await store.fetch();
    expect(store.namePlusRole).toEqual(`${mockUserResponse.name} ${mockUserResponse.role}`);
  });
});
