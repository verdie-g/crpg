import { type PartialDeep } from 'type-fest';
import { type RouteRecordRaw } from 'vue-router/auto';
import Role from '@/models/role';
import { useNavigation } from './use-navigation';

it.each<[Array<PartialDeep<RouteRecordRaw>>, Role, number]>([
  [
    [
      {
        name: 'shop',
        meta: {
          roles: ['Admin'],
          showInNav: false,
        },
      },
    ],
    Role.Admin,
    0,
  ],
  [
    [
      {
        name: 'shop',
        meta: {
          roles: ['Admin'],
          showInNav: true,
        },
      },
    ],
    Role.User,
    0,
  ],
  [
    [
      {
        name: 'shop',
        meta: {
          roles: ['Admin'],
          showInNav: true,
        },
      },
    ],
    Role.Admin,
    1,
  ],
  [
    [
      {
        name: 'shop',
        meta: {
          showInNav: true,
        },
      },
    ],
    Role.User,
    1,
  ],
  [
    [
      {
        name: 'shop',
        children: [
          {
            name: 'shop-child',
            meta: {
              showInNav: true,
            },
          },
        ],
      },
    ],
    Role.User,
    1,
  ],
])('filter - routes: %j, role: %s', (routes, role, expectation) => {
  const { mainNavigation } = useNavigation(routes as RouteRecordRaw[], role);
  expect(mainNavigation.value.length).toEqual(expectation);
});

it('sort', () => {
  const routes = [
    {
      name: 'shop',
      meta: {
        showInNav: true,
        sortInNav: 50,
      },
    },
    {
      name: 'char',
      meta: {
        showInNav: true,
        sortInNav: 60,
      },
    },
  ] as RouteRecordRaw[];

  const { mainNavigation } = useNavigation(routes, Role.User);
  expect(mainNavigation.value[0].name).toEqual('char');
  expect(mainNavigation.value[1].name).toEqual('shop');
});
