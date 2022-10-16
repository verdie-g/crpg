import { createTestingPinia } from '@pinia/testing';
import { mount } from '@vue/test-utils';
import Role from '@/models/role';
import Platform from '@/models/platform';
import User from '@/models/user';
import { getRoute, next } from '@/__mocks__/router';

const mockedSignInSilent = vi.fn();
vi.mock('@/services/auth-service', () => {
  return {
    signInSilent: mockedSignInSilent,
  };
});

import { useUserStore } from '@/stores/user';
import { authRouterMiddleware } from './auth';

// need for testing with pinia-store
mount(
  defineComponent({
    template: `<div></div>`,
  }),
  {
    global: {
      plugins: [createTestingPinia()],
    },
  }
);

const userStore = useUserStore();

beforeEach(() => {
  userStore.$reset();
});

const getUser = (user: Partial<User> = {}): User => ({
  id: 1,
  platform: Platform.Steam,
  platformUserId: 123,
  name: 'Rainbowdash',
  gold: 300,
  heirloomPoints: 2,
  role: Role.User,
  avatarSmall: '',
  avatarMedium: '',
  avatarFull: '',
  ...user,
});

const from = getRoute();

it('skip any route validation', async () => {
  const to = getRoute({
    name: 'skip-auth-route',
    path: '/skip-auth-route',
    meta: {
      layout: 'default',
      skipAuth: true,
    },
  });

  expect(await authRouterMiddleware(to, from, next)).toEqual(true);

  expect(userStore.fetchUser).not.toBeCalled();
  expect(mockedSignInSilent).not.toBeCalled();
});

describe('route not requires any role', () => {
  const to = getRoute({
    name: 'user',
    path: '/user',
  });

  it('!user - no token -> try to signInSilent and fetch user', async () => {
    expect(await authRouterMiddleware(to, from, next)).toEqual(true);

    expect(mockedSignInSilent).toBeCalled();
    expect(userStore.fetchUser).not.toBeCalled();
  });

  it('!user - has token -> try to signInSilent and fetch user', async () => {
    mockedSignInSilent.mockResolvedValueOnce('test_token');

    expect(await authRouterMiddleware(to, from, next)).toEqual(true);

    expect(mockedSignInSilent).toBeCalled();
    expect(userStore.fetchUser).toBeCalled();
  });
});

describe('route requires role', () => {
  const to = getRoute({
    name: 'user',
    path: '/user',
    meta: {
      roles: ['User'],
    },
  });

  it('!user + no token -> go to index page', async () => {
    expect(await authRouterMiddleware(to, from, next)).toEqual({ name: 'index' });
  });

  it('user with role:User -> validation passed', async () => {
    userStore.user = getUser({ role: Role.User });

    expect(await authRouterMiddleware(to, from, next)).toEqual(true);
    expect(mockedSignInSilent).not.toBeCalled();
    expect(userStore.fetchUser).not.toBeCalled();
  });
});

describe('with Admin role', () => {
  const to = getRoute({
    name: 'admin',
    path: '/admin',
    meta: {
      roles: ['Admin'],
    },
  });

  it('user with role:User -> go to index page', async () => {
    userStore.user = getUser({ role: Role.User });

    expect(await authRouterMiddleware(to, from, next)).toEqual({ name: 'index' });
  });

  it('user with role:Admin -> validation passed', async () => {
    userStore.user = getUser({ role: Role.Admin });

    expect(await authRouterMiddleware(to, from, next)).toEqual(true);
  });
});
