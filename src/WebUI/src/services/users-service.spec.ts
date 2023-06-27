import { mockGet, mockPost, mockDelete, mockPut } from 'vi-fetch';
import { response } from '@/__mocks__/crpg-client';
import mockUser from '@/__mocks__/user.json';
import mockUserPublic from '@/__mocks__/user-public.json';
import mockUserItems from '@/__mocks__/user-items.json';

import type { User, UserItem, UserPublic } from '@/models/user';

vi.mock('@/services/auth-service', () => ({
  getToken: vi.fn().mockResolvedValue('mockedToken'),
}));

const { mockedMapRestrictions, mockedGetActiveJoinRestriction } = vi.hoisted(() => ({
  mockedMapRestrictions: vi.fn(),
  mockedGetActiveJoinRestriction: vi.fn(),
}));
vi.mock('@/services/restriction-service', () => ({
  getActiveJoinRestriction: mockedGetActiveJoinRestriction,
  mapRestrictions: mockedMapRestrictions,
}));

const { mockedMapClanResponse } = vi.hoisted(() => ({
  mockedMapClanResponse: vi.fn(),
}));
vi.mock('@/services/clan-service', () => ({ mapClanResponse: mockedMapClanResponse }));

import {
  getUser,
  getUserById,
  deleteUser,
  mapUserItem,
  extractItemFromUserItem,
  getUserItems,
  buyUserItem,
  repairUserItem,
  sellUserItem,
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
        item: {
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

it('repairUserItem', async () => {
  mockPut('/users/self/items/123/repair').willResolve(response(mockUserItems[0]));

  expect(await repairUserItem(123)).toEqual({
    ...mockUserItems[0],
    createdAt: new Date(mockUserItems[0].createdAt),
  });
});

it('sellUserItem', async () => {
  mockDelete('/users/self/items/123').willResolve(null, 204);

  expect(await sellUserItem(123)).toEqual(null);
});

describe('userItems: filterBy, sortBy, groupBy', () => {
  const userItems = [
    {
      id: 1,
      item: {
        type: 'HeadArmor',
        name: 'Fluttershy',
      },
    },
    {
      id: 2,
      item: {
        type: 'Thrown',
        name: 'Rainbow Dash',
      },
    },
    {
      id: 3,
      item: {
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
    expect(mockedMapClanResponse).not.toBeCalled();
  });

  it('user has a clan', async () => {
    mockGet('/users/self/clans').willResolve(
      response({
        clan: {
          id: 1,
          tag: 'mlp',
          name: 'My Little Pony',
        },
      })
    );

    await getUserClan();
    expect(mockedMapClanResponse).toBeCalled();
  });
});

it('getUserRestrictions', async () => {
  const USER_ID = 123;
  const USER_RESTRICTIONS = [{ id: 1 }];
  mockGet(`/users/${USER_ID}/restrictions`).willResolve(response(USER_RESTRICTIONS));

  await getUserRestrictions(USER_ID);

  expect(mockedMapRestrictions).toBeCalledWith(USER_RESTRICTIONS);
});

it('getUserActiveJoinRestriction', async () => {
  const USER_ID = 123;
  const USER_RESTRICTIONS = [{ id: 1 }];
  mockGet(`/users/${USER_ID}/restrictions`).willResolve(response(USER_RESTRICTIONS));

  await getUserActiveJoinRestriction(USER_ID);

  expect(mockedGetActiveJoinRestriction).toBeCalled();
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
