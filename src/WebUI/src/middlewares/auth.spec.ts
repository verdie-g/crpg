import { createTestingPinia } from '@pinia/testing';
import { ErrorResponse } from 'oidc-client-ts';
import Role from '@/models/role';
import { getRoute, next } from '@/__mocks__/router';

const { mockedSignInCallback, mockedGetUser } = vi.hoisted(() => ({
  mockedSignInCallback: vi.fn(),
  mockedGetUser: vi.fn(),
}));

vi.mock('@/services/auth-service', () => {
  return {
    userManager: {
      signinCallback: mockedSignInCallback,
    },
    getUser: mockedGetUser,
  };
});

import { useUserStore } from '@/stores/user';
import { authRouterMiddleware, signInCallback } from './auth';

const userStore = useUserStore(createTestingPinia());

beforeEach(() => {
  userStore.$reset();
});

const from = getRoute();

it('skip route validation', async () => {
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
});

describe('route not requires any role', () => {
  const to = getRoute({
    name: 'user',
    path: '/user',
  });

  it('!userStore && !isSignIn', async () => {
    mockedGetUser.mockResolvedValueOnce(null);

    expect(await authRouterMiddleware(to, from, next)).toEqual(true);

    expect(userStore.fetchUser).not.toBeCalled();
    expect(mockedGetUser).toBeCalled();
  });

  it('!userStore && isSignIn', async () => {
    mockedGetUser.mockResolvedValueOnce({});

    expect(await authRouterMiddleware(to, from, next)).toEqual(true);

    expect(userStore.fetchUser).toBeCalled();
    expect(mockedGetUser).toBeCalled();
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

  it('!user + !isSignIn -> go to index page', async () => {
    expect(await authRouterMiddleware(to, from, next)).toEqual({ name: 'Root' });
    expect(userStore.fetchUser).toBeCalled();
    expect(mockedGetUser).toBeCalled();
  });

  it('user with role:User -> validation passed', async () => {
    userStore.$patch({ user: { role: Role.User } });

    expect(await authRouterMiddleware(to, from, next)).toEqual(true);
    expect(userStore.fetchUser).not.toBeCalled();
    expect(mockedGetUser).not.toBeCalled();
  });
});

describe('with Admin or Moderator role', () => {
  const to = getRoute({
    name: 'admin',
    path: '/admin',
    meta: {
      roles: ['Admin', 'Moderator'],
    },
  });

  it('user with role:User -> go to index page', async () => {
    userStore.$patch({ user: { role: Role.User } });

    expect(await authRouterMiddleware(to, from, next)).toEqual({ name: 'Root' });
  });

  it('user with role:Admin -> validation passed', async () => {
    userStore.$patch({ user: { role: Role.Admin } });

    expect(await authRouterMiddleware(to, from, next)).toEqual(true);
  });

  it('user with role:Moderator -> validation passed', async () => {
    userStore.$patch({ user: { role: Role.Moderator } });

    expect(await authRouterMiddleware(to, from, next)).toEqual(true);
  });
});

describe('signInCallback', () => {
  it('ok', async () => {
    expect(await signInCallback(getRoute(), getRoute(), next)).toEqual({ name: 'Characters' });
    expect(mockedSignInCallback).toHaveBeenCalled();
  });

  it('error - invalid grant', async () => {
    mockedSignInCallback.mockRejectedValue(
      new ErrorResponse({
        error: 'access_denied',
      })
    );

    expect(await signInCallback(getRoute(), getRoute(), next)).toEqual({ name: 'Banned' });
  });

  it('error - invalid grant', async () => {
    mockedSignInCallback.mockRejectedValue({ error: 'some error' });

    expect(await signInCallback(getRoute(), getRoute(), next)).toEqual({ name: 'Root' });
  });
});
