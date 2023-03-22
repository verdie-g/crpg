import { getRoute, next } from '@/__mocks__/router';
import { createTestingPinia } from '@pinia/testing';

import { type User } from '@/models/user';
import { type Character } from '@/models/character';

import { useUserStore } from '@/stores/user';
const userStore = useUserStore(createTestingPinia());

import { characterValidate, activeCharacterRedirect } from './character';

beforeEach(() => {
  userStore.$reset();
});

describe('characterValidate', () => {
  it('redirect - no chars page', async () => {
    const to = getRoute({
      params: { id: '1224' },
    });

    const result = await characterValidate(to, getRoute(), next);

    expect(userStore.fetchCharacters).toHaveBeenCalled();
    expect(result).toEqual({ name: 'Characters' });
  });

  it('redirect - active char page', async () => {
    vi.mocked(userStore.validateCharacter).mockReturnValue(false);

    const ACTIVE_CHAR_ID = 6;

    userStore.user = {
      activeCharacterId: ACTIVE_CHAR_ID,
    } as User;

    const to = getRoute({
      params: { id: '1224' },
    });

    const result = await characterValidate(to, getRoute(), next);

    expect(userStore.fetchCharacters).toHaveBeenCalled();
    expect(result).toEqual({ name: 'CharactersIdInventory', params: { id: ACTIVE_CHAR_ID } });
  });

  it('no redirect', async () => {
    vi.mocked(userStore.validateCharacter).mockReturnValue(true);

    const ACTIVE_CHAR_ID = 6;

    userStore.user = {
      activeCharacterId: ACTIVE_CHAR_ID,
    } as User;

    userStore.characters = [{ id: ACTIVE_CHAR_ID }] as Character[];

    const to = getRoute({
      params: { id: '1224' },
    });

    const result = await characterValidate(to, getRoute(), next);

    expect(userStore.fetchCharacters).not.toHaveBeenCalled();
    expect(result).toStrictEqual(true);
  });
});

describe('activeCharacterRedirect', () => {
  it('no redirect', async () => {
    const result = await activeCharacterRedirect(getRoute(), getRoute(), next);

    expect(userStore.fetchCharacters).toHaveBeenCalled();
    expect(result).toStrictEqual(true);
  });

  it('redirect to active char page - user.activeCharacterId', async () => {
    const ACTIVE_CHAR_ID = 3;

    userStore.user = {
      activeCharacterId: ACTIVE_CHAR_ID,
    } as User;

    const result = await activeCharacterRedirect(getRoute(), getRoute(), next);

    expect(userStore.fetchCharacters).toHaveBeenCalled();
    expect(result).toEqual({
      name: 'CharactersIdInventory',
      params: { id: ACTIVE_CHAR_ID },
    });
  });

  it('redirect to active char page - characters[0].id - first char', async () => {
    const CHAR_ID = 3;

    userStore.characters = [{ id: CHAR_ID }] as Character[];

    const result = await activeCharacterRedirect(getRoute(), getRoute(), next);

    expect(userStore.fetchCharacters).not.toHaveBeenCalled();
    expect(result).toEqual({
      name: 'CharactersIdInventory',
      params: { id: CHAR_ID },
    });
  });
});
