import { getRoute, next } from '@/__mocks__/router';
import { createTestingPinia } from '@pinia/testing';

import { type Clan, ClanMemberRole } from '@/models/clan';
import { useUserStore } from '@/stores/user';
const userStore = useUserStore(createTestingPinia());

import {
  clanIdParamValidate,
  clanExistValidate,
  canUpdateClan,
  canManageApplications,
} from './clan';

beforeEach(() => {
  userStore.$reset();
});

describe('clan id format validate', () => {
  it('404', () => {
    const to = getRoute({
      name: 'ClansId',
      path: '/clans/:id',
      params: {
        id: 'abc',
      },
    });

    const result = clanIdParamValidate(to, getRoute(), next);

    expect(result).toEqual({ name: 'PageNotFound' });
  });

  it('ok', () => {
    const to = getRoute({
      name: 'ClansId',
      path: '/clans/:id',
      params: {
        id: '1',
      },
    });

    const result = clanIdParamValidate(to, getRoute(), next);

    expect(result).toStrictEqual(true);
  });
});

describe('clan exist validate', () => {
  it('user already have a clan', async () => {
    const CLAN_ID = 1;
    userStore.clan = { id: CLAN_ID } as Clan;

    const result = await clanExistValidate(getRoute(), getRoute(), next);

    expect(userStore.getUserClanAndRole).not.toHaveBeenCalled();
    expect(result).toEqual({ name: 'ClansId', params: { id: String(CLAN_ID) } });
  });

  it('user already have a clan', async () => {
    const result = await clanExistValidate(getRoute(), getRoute(), next);

    expect(userStore.getUserClanAndRole).toHaveBeenCalled();
    expect(result).toStrictEqual(true);
  });
});

describe('can update clan', () => {
  const CLAN_ID = 1;
  const to = getRoute({
    name: 'ClansIdUpdate',
    path: '/clans/:id/update',
    params: {
      id: String(CLAN_ID),
    },
  });

  it('officer', async () => {
    userStore.clan = { id: CLAN_ID } as Clan;
    userStore.clanMemberRole = ClanMemberRole.Officer;

    const result = await canUpdateClan(to, getRoute(), next);

    expect(userStore.getUserClanAndRole).not.toHaveBeenCalled();

    expect(result).toEqual({ name: 'Clans' });
  });

  it('leader', async () => {
    userStore.clan = { id: CLAN_ID } as Clan;
    userStore.clanMemberRole = ClanMemberRole.Leader;

    const result = await canUpdateClan(to, getRoute(), next);

    expect(userStore.getUserClanAndRole).not.toHaveBeenCalled();

    expect(result).toStrictEqual(true);
  });
});

describe('can update clan', () => {
  const CLAN_ID = 1;
  const to = getRoute({
    name: 'ClansIdApplications',
    path: '/clans/:id/application',
    params: {
      id: String(CLAN_ID),
    },
  });

  it('member', async () => {
    const CLAN_ID = 1;
    userStore.clan = { id: CLAN_ID } as Clan;
    userStore.clanMemberRole = ClanMemberRole.Member;

    const result = await canManageApplications(to, getRoute(), next);

    expect(userStore.getUserClanAndRole).not.toHaveBeenCalled();

    expect(result).toEqual({ name: 'Clans' });
  });

  it('officer', async () => {
    const CLAN_ID = 1;
    userStore.clan = { id: CLAN_ID } as Clan;
    userStore.clanMemberRole = ClanMemberRole.Leader;

    const result = await canManageApplications(to, getRoute(), next);

    expect(userStore.getUserClanAndRole).not.toHaveBeenCalled();

    expect(result).toStrictEqual(true);
  });
});
