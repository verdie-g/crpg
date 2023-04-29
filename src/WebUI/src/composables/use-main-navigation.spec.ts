import { type PartialDeep } from 'type-fest';
import { type RouteRecordRaw } from 'vue-router/auto';
import { createTestingPinia } from '@pinia/testing';
import Role from '@/models/role';
import { useUserStore } from '@/stores/user';

const userStore = useUserStore(createTestingPinia());

const mockRoutes = vi.fn();
vi.mock('vue-router/auto/routes', () => ({
  get routes() {
    return mockRoutes();
  },
}));

import { useNavigation } from './use-main-navigation';

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
  userStore.$patch({ user: { role } });
  mockRoutes.mockReturnValue(routes);

  const { mainNavigation } = useNavigation();
  expect(mainNavigation.value.length).toEqual(expectation);
});

it('sort', () => {
  userStore.$patch({ user: { role: Role.User } });
  mockRoutes.mockReturnValue([
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
  ]);

  const { mainNavigation } = useNavigation();
  expect(mainNavigation.value[0].name).toEqual('char');
  expect(mainNavigation.value[1].name).toEqual('shop');
});
