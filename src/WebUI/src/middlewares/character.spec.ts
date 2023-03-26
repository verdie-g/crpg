import { getRoute, next } from '@/__mocks__/router';
import { createTestingPinia } from '@pinia/testing';

import { useUserStore } from '@/stores/user';

const userStore = useUserStore(createTestingPinia());

import { characterValidate, activeCharacterRedirect } from './character';

beforeEach(() => {
  userStore.$reset();
});

describe('characterValidate', () => {
  it('redirect - no chars page', async () => {
    vi.mocked(userStore.validateCharacter).mockReturnValue(false);
    const RANDOM_CHAR_ID = 123;

    const to = getRoute({
      params: { id: String(RANDOM_CHAR_ID) },
    });

    // https://pinia.vuejs.org/cookbook/testing.html#mocking-getters
    // @ts-expect-error
    userStore.activeCharacterId = null;

    const result = await characterValidate(to, getRoute(), next);

    expect(userStore.fetchCharacters).toHaveBeenCalled();
    expect(userStore.validateCharacter).toHaveBeenCalledWith(RANDOM_CHAR_ID);
    expect(result).toEqual({ name: 'Characters' });
  });

  it('redirect - active char page', async () => {
    vi.mocked(userStore.validateCharacter).mockReturnValue(false);

    const ACTIVE_CHAR_ID = 6;
    const RANDOM_CHAR_ID = 123;

    // @ts-expect-error
    userStore.activeCharacterId = ACTIVE_CHAR_ID;

    const to = getRoute({
      params: { id: String(RANDOM_CHAR_ID) },
    });

    const result = await characterValidate(to, getRoute(), next);

    expect(userStore.validateCharacter).toHaveBeenCalledWith(RANDOM_CHAR_ID);
    expect(userStore.fetchCharacters).toHaveBeenCalled();
    expect(result).toEqual({
      name: 'CharactersIdInventory',
      params: { id: String(ACTIVE_CHAR_ID) },
    });
  });

  it.only('no redirect', async () => {
    vi.mocked(userStore.validateCharacter).mockReturnValue(true);
    const RANDOM_CHAR_ID = 123;

    const to = getRoute({
      params: { id: String(RANDOM_CHAR_ID) },
    });

    const result = await characterValidate(to, getRoute(), next);

    expect(userStore.validateCharacter).toHaveBeenCalledWith(RANDOM_CHAR_ID);
    expect(result).toStrictEqual(true);
  });
});

describe('activeCharacterRedirect', () => {
  it('no redirect', async () => {
    // @ts-expect-error
    userStore.activeCharacterId = null;

    const result = await activeCharacterRedirect(getRoute(), getRoute(), next);

    expect(userStore.fetchCharacters).toHaveBeenCalled();
    expect(result).toStrictEqual(true);
  });

  it('redirect to active char page - user.activeCharacterId', async () => {
    const ACTIVE_CHAR_ID = 3;
    // @ts-expect-error
    userStore.activeCharacterId = ACTIVE_CHAR_ID;

    const result = await activeCharacterRedirect(getRoute(), getRoute(), next);

    expect(userStore.fetchCharacters).toHaveBeenCalled();
    expect(result).toEqual({
      name: 'CharactersIdInventory',
      params: { id: String(ACTIVE_CHAR_ID) },
    });
  });

  it('characters already fetched', async () => {
    userStore.$patch({
      charactersOnceFetched: true,
    });
    // @ts-expect-error
    userStore.activeCharacterId = null;

    const result = await activeCharacterRedirect(getRoute(), getRoute(), next);

    expect(userStore.fetchCharacters).not.toHaveBeenCalled();
    expect(result).toStrictEqual(true);
  });
});
