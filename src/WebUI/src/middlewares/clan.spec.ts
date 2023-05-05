import { getRoute, next } from '@/__mocks__/router';
import { createTestingPinia } from '@pinia/testing';

import { type Clan } from '@/models/clan';
import { useUserStore } from '@/stores/user';
const userStore = useUserStore(createTestingPinia());

import { clanIdParamValidate, clanExistValidate } from './clan';

beforeEach(() => {
  userStore.$reset();
});

describe('clan id format validate', () => {
  it('404', () => {
    const to = getRoute({
      name: 'clans-id',
      path: '/clans/:id',
      params: {
        id: 'abc',
      },
    });

    const result = clanIdParamValidate(to, getRoute(), next);

    expect(result).toEqual({ name: '$404' });
  });

  it('ok', () => {
    const to = getRoute({
      name: 'clans-id',
      path: '/clans/:id',
      params: {
        id: '1',
      },
    });

    const result = clanIdParamValidate(to, getRoute(), next);

    expect(result).toEqual(true);
  });
});

describe('clan exist validate', () => {
  it('user already have a clan', async () => {
    const CLAN_ID = 1;
    userStore.clan = { id: CLAN_ID } as Clan;

    const result = await clanExistValidate(getRoute(), getRoute(), next);

    expect(userStore.getUserClanAndRole).not.toHaveBeenCalled();
    expect(result).toEqual({ name: 'ClansId', params: { id: CLAN_ID } });
  });

  it('user already have a clan', async () => {
    const result = await clanExistValidate(getRoute(), getRoute(), next);

    expect(userStore.getUserClanAndRole).toHaveBeenCalled();
    expect(result).toEqual(true);
  });
});
