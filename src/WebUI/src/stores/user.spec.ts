import { setActivePinia, createPinia } from 'pinia';
import mockUser from '@/__mocks__/user.json';
import mockCharacters from '@/__mocks__/characters.json';
import mockUserItems from '@/__mocks__/user-items.json';

import { type UserItem } from '@/models/user';
import { type Character } from '@/models/character';

const mockedGetUserClan = vi.fn();
vi.mock('@/services/users-service', () => {
  return {
    getUser: vi.fn().mockResolvedValue(mockUser),
    getUserItems: vi.fn().mockResolvedValue(mockUserItems),
    buyUserItem: vi.fn().mockResolvedValue(mockUserItems[0]),
    getUserClan: mockedGetUserClan,
  };
});

vi.mock('@/services/characters-service', () => {
  return {
    getCharacters: vi.fn().mockResolvedValue(mockCharacters),
  };
});

const mockedGetClanMembers = vi.fn();
const mockedGetClanMember = vi.fn();
vi.mock('@/services/clan-service', () => {
  return {
    getClanMembers: mockedGetClanMembers,
    getClanMember: mockedGetClanMember,
  };
});

import { useUserStore } from './user';

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
    expect(store.characters).toEqual([]);
  });

  describe('getters: activeCharacterId', () => {
    it('user.activeCharacterId', () => {
      store.$patch({ user: { activeCharacterId: 112 }, characters: [{ id: 1 }, { id: 112 }] });

      expect(store.activeCharacterId).toEqual(112);
    });

    it('characters[0].id', () => {
      store.$patch({ characters: [{ id: 1 }, { id: 112 }] });

      expect(store.activeCharacterId).toEqual(1);
    });

    it('non active char', () => {
      expect(store.activeCharacterId).toEqual(null);
    });
  });

  describe('actions', () => {
    it('fetchUser', async () => {
      await store.fetchUser();

      expect(store.user).toEqual(mockUser);
    });

    it('fetchCharacters', async () => {
      await store.fetchCharacters();

      expect(store.characters).toEqual(mockCharacters);
    });

    it('validateCharacter', () => {
      store.$patch({ characters: [{ id: 1 }, { id: 112 }] });
      expect(store.validateCharacter(1)).toBeTruthy();
      expect(store.validateCharacter(112)).toBeTruthy();
      expect(store.validateCharacter(223)).toBeFalsy();
    });

    it('replaceCharacter', () => {
      store.$patch({
        characters: [
          { id: 1, name: 'Rarity' },
          { id: 112, name: 'Spike' },
        ],
      });

      store.replaceCharacter({ id: 112, name: 'Twilight Sparkle' } as Character);

      expect(store.characters).toEqual([
        { id: 1, name: 'Rarity' },
        { id: 112, name: 'Twilight Sparkle' },
      ]);
    });

    it('fetchUserItems', async () => {
      await store.fetchUserItems();

      expect(store.userItems).toEqual(mockUserItems);
    });

    it('addUserItem', () => {
      expect(store.userItems).toEqual([]);

      store.addUserItem({ id: 1 } as UserItem);
      expect(store.userItems).toEqual([{ id: 1 }]);

      store.addUserItem({ id: 2 } as UserItem);
      expect(store.userItems).toEqual([{ id: 1 }, { id: 2 }]);
    });

    it('subtractGold', () => {
      store.$patch({ user: { gold: 100 } });

      store.subtractGold(50);

      expect(store.user!.gold).toEqual(50);
    });

    it('buyItem', async () => {
      store.$patch({ user: { gold: 100 } });

      await store.buyItem('id123');

      expect(store.userItems).toEqual([mockUserItems[0]]);
    });

    it('getUserClan', async () => {
      const CLAN = { id: 1, tag: 'mlp' };
      mockedGetUserClan.mockResolvedValue(CLAN);

      await store.getUserClan();

      expect(store.clan).toEqual(CLAN);
    });

    describe('getUserClanMember', () => {
      it('not in a clan', async () => {
        store.$patch({ clan: null, user: null });

        await store.getUserClanMember();

        expect(store.clanMember).toEqual(null);
      });

      it('has some clan', async () => {
        const CLAN = { id: 1, tag: 'mlp' };
        mockedGetClanMember.mockReturnValue(CLAN);
        store.$patch({ clan: { id: 1 }, user: { id: 1 } });

        await store.getUserClanMember();

        expect(store.clanMember).toEqual(CLAN);
      });
    });
  });
});
