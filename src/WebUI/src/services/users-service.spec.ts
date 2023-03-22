import { mockGet, mockPost, mockDelete, mockPut } from 'vi-fetch';
import { response } from '@/__mocks__/crpg-client';
import mockUser from '@/__mocks__/user.json';
import mockUserPublic from '@/__mocks__/user-public.json';
import mockUserItems from '@/__mocks__/user-items.json';

import type { User, UserItem, UserItemRank, UserPublic } from '@/models/user';
import { ItemType } from '@/models/item';

vi.mock('@/services/auth-service', () => ({
  getToken: vi.fn().mockResolvedValue('mockedToken'),
}));

const mockGetActiveJoinRestriction = vi.fn();
const mockMapRestrictions = vi.fn();
vi.mock('@/services/restriction-service', () => ({
  getActiveJoinRestriction: mockGetActiveJoinRestriction,
  mapRestrictions: mockMapRestrictions,
}));

const mockMapClanResponse = vi.fn();
vi.mock('@/services/clan-service', () => ({
  mapClanResponse: mockMapClanResponse,
}));

import {
  getUser,
  getUserById,
  deleteUser,
  mapUserItem,
  extractItemFromUserItem,
  getUserItems,
  buyUserItem,
  upgradeUserItem,
  sellUserItem,
  getItemRanks,
  getUserClan,
  getUserRestrictions,
  getUserActiveJoinRestriction,
  groupUserItemsByType,
  searchUser,
} from './users-service';

it('getUser', async () => {
  mockGet('/users/self').willResolve(response<User>(mockUser as User));

  expect(await getUser()).toEqual(mockUser);
});

it('getUserById', async () => {
  mockGet('/users/123').willResolve(response<UserPublic>(mockUserPublic as UserPublic));

  expect(await getUserById(123)).toEqual(mockUserPublic);
});

it('deleteUser', async () => {
  mockDelete('/users/self').willResolve(null, 204);

  expect(await deleteUser()).toEqual(null);
});

it('mapUserItem', () => {
  expect(
    mapUserItem({
      id: 1,
      // @ts-ignore
      createdAt: '2022-12-20T07:47:37.3198708Z',
    })
  ).toEqual({
    id: 1,
    createdAt: new Date('2022-12-20T07:47:37.3198708Z'),
  });
});

it('extractItemFromUserItem', () => {
  expect(
    extractItemFromUserItem([
      {
        id: 1,
        baseItem: {
          id: '123',
        },
      } as UserItem,
    ])
  ).toEqual([
    {
      id: '123',
    },
  ]);
});

it('getUserItems', async () => {
  mockGet('/users/self/items').willResolve(response(mockUserItems));

  expect(await getUserItems()).toEqual(
    mockUserItems.map(ui => ({ ...ui, createdAt: new Date(ui.createdAt) }))
  );
});

it('buyUserItem', async () => {
  const mock = mockPost('/users/self/items').willResolve(response(mockUserItems[0]));

  expect(await buyUserItem('123')).toEqual({
    ...mockUserItems[0],
    createdAt: new Date(mockUserItems[0].createdAt),
  });

  expect(mock).toHaveFetchedWithBody({ itemId: '123' });
});

it('upgradeUserItem', async () => {
  mockPut('/users/self/items/123/upgrade').willResolve(response(mockUserItems[0]));

  expect(await upgradeUserItem(123)).toEqual({
    ...mockUserItems[0],
    createdAt: new Date(mockUserItems[0].createdAt),
  });
});

it('sellUserItem', async () => {
  mockDelete('/users/self/items/123').willResolve(null, 204);

  expect(await sellUserItem(123)).toEqual(null);
});

it('getItemRanks', () => {
  expect(getItemRanks()).toEqual([0, 1, 2, 3]);
});

describe('userItems: filterBy, sortBy, groupBy', () => {
  const userItems = [
    {
      id: 1,
      rank: 0,
      baseItem: {
        type: 'HeadArmor',
        name: 'Fluttershy',
      },
    },
    {
      id: 2,
      rank: 1,
      baseItem: {
        type: 'Thrown',
        name: 'Rainbow Dash',
      },
    },
    {
      id: 3,
      rank: 1,
      baseItem: {
        type: 'Thrown',
        name: 'Rarity',
      },
    },
  ] as UserItem[];

  it('groupUserItemsByType', () => {
    expect(groupUserItemsByType(userItems)).toMatchObject([
      {
        type: 'HeadArmor',
        items: [
          {
            id: 1,
          },
        ],
      },
      {
        type: 'Thrown',
        items: [
          {
            id: 2,
          },
          {
            id: 3,
          },
        ],
      },
    ]);
  });
});

describe('getUserClan', () => {
  it('user does`t have a clan', async () => {
    mockGet('/users/self/clans').willResolve(response(null));

    expect(await getUserClan()).toEqual(null);
    expect(mockMapClanResponse).not.toBeCalled();
  });

  it('user has a clan', async () => {
    mockGet('/users/self/clans').willResolve(
      response({
        id: 1,
        tag: 'mlp',
        name: 'My Little Pony',
      })
    );

    await getUserClan();
    expect(mockMapClanResponse).toBeCalled();
  });
});

it('getUserRestrictions', async () => {
  const USER_ID = 123;
  const USER_RESTRICTIONS = [{ id: 1 }];
  mockGet(`/users/${USER_ID}/restrictions`).willResolve(response(USER_RESTRICTIONS));

  await getUserRestrictions(USER_ID);

  expect(mockMapRestrictions).toBeCalledWith(USER_RESTRICTIONS);
});

it('getUserActiveJoinRestriction', async () => {
  const USER_ID = 123;
  const USER_RESTRICTIONS = [{ id: 1 }];
  mockGet(`/users/${USER_ID}/restrictions`).willResolve(response(USER_RESTRICTIONS));

  await getUserActiveJoinRestriction(USER_ID);

  expect(mockGetActiveJoinRestriction).toBeCalled();
});

it('searchUser', async () => {
  const NAME = 'Fluttershy';
  const USER = {
    id: 1,
    name: 'Fluttershy',
    platform: 'Steam',
  };

  mockGet(`/users/search/?name=${NAME}`, true).willResolve(response(USER));

  expect(await searchUser({ name: NAME })).toEqual(USER);
});
