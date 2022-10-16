import { createTestingPinia } from '@pinia/testing';
import { mount } from '@vue/test-utils';
import Role from '@/models/role';
import Platform from '@/models/platform';
import User from '@/models/user';

const mockedSignInSilent = vi.fn();
vi.mock('@/services/auth-service', () => {
  return {
    signInSilent: mockedSignInSilent,
  };
});

import { useUserStore } from '@/stores/user';
import { authRouterMiddleware } from './auth';

// TODO:
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

const from = {
  name: 'index',
  path: '/',
};

const getUser = (): User => ({
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
});

it('skip any route validation', async () => {
  const to = {
    name: 'skip-auth-route',
    path: '/skip-auth-route',
    meta: {
      skipAuth: true,
    },
  };

  // @ts-ignore need mock next() fn :(
  expect(await authRouterMiddleware(to, from)).toEqual(true);

  expect(userStore.fetchUser).not.toBeCalled();
  expect(mockedSignInSilent).not.toBeCalled();
});

describe('route requires User role', () => {
  const to = {
    name: 'user',
    path: '/user',
    meta: {},
  };

  it('!user - no token -> try to signInSilent and fetch user', async () => {
    // @ts-ignore need mock next() fn :(
    expect(await authRouterMiddleware(to, from)).toEqual(true);

    expect(mockedSignInSilent).toBeCalled();
    expect(userStore.fetchUser).not.toBeCalled();
  });

  it('!user - has token -> try to signInSilent and fetch user', async () => {
    mockedSignInSilent.mockResolvedValueOnce('test_token');

    // @ts-ignore need mock next() fn :(
    expect(await authRouterMiddleware(to, from)).toEqual(true);

    expect(mockedSignInSilent).toBeCalled();
    expect(userStore.fetchUser).toBeCalled();
  });
});

describe('route requires role', () => {
  const to = {
    name: 'user',
    path: '/user',
    meta: {
      roles: ['User'],
    },
  };

  it('!user + no token -> go to index page', async () => {
    // @ts-ignore need mock next() fn :(
    expect(await authRouterMiddleware(to, from)).toEqual({ name: 'index' });
  });

  it('user with role:User -> validation passed', async () => {
    userStore.user = {
      ...getUser(),
      role: Role.User,
    };

    // @ts-ignore need mock next() fn :(
    expect(await authRouterMiddleware(to, from)).toEqual(true);
    expect(mockedSignInSilent).not.toBeCalled();
    expect(userStore.fetchUser).not.toBeCalled();
  });
});

describe('with Admin role', () => {
  const to = {
    name: 'admin',
    path: '/admin',
    meta: {
      roles: ['Admin'],
    },
  };

  it('user with role:User -> go to index page', async () => {
    userStore.user = {
      ...getUser(),
      role: Role.User,
    };

    // @ts-ignore need mock next() fn :(
    expect(await authRouterMiddleware(to, from)).toEqual({ name: 'index' });
  });

  it('user with role:Admin -> validation passed', async () => {
    userStore.user = {
      ...getUser(),
      role: Role.Admin,
    };

    // @ts-ignore need mock next() fn :(`
    expect(await authRouterMiddleware(to, from)).toEqual(true);
  });
});
