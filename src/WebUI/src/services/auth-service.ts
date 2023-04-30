import {
  UserManager,
  WebStorageStateStore,
  type User,
  // Log,
} from 'oidc-client-ts';
import Role from '@/models/role';
import { Platform } from '@/models/platform';

// Log.setLogger(console);
// Log.setLevel(Log.DEBUG);

interface TokenPayload {
  userId: number;
  userRole: Role;
  expiration: Date;
  issuedAt: Date;
}

export const extractToken = (user: User | null): string | null =>
  user !== null ? user.access_token : null;

export const parseJwt = (token: string) =>
  JSON.parse(Buffer.from(token.split('.')[1], 'base64').toString());

export const userManager = new UserManager({
  authority: import.meta.env.VITE_API_BASE_URL,
  scope: 'openid offline_access user_api', // TODO: to .env/cfg
  client_id: 'crpg-web-ui', // TODO: to .env/cfg
  redirect_uri: window.location.origin + '/signin-callback',
  silent_redirect_uri: window.location.origin + '/signin-silent-callback',
  post_logout_redirect_uri: window.location.origin,
  response_type: 'code', // TODO: to .env/cfg
  userStore: new WebStorageStateStore({ store: window.localStorage }),
});

export const getUser = () => userManager.getUser();

export const signInSilent = async (platform: Platform) => {
  try {
    const user = await userManager.signinSilent({
      extraQueryParams: {
        identity_provider: platform,
      },
    });
    return extractToken(user);
  } catch (error) {
    // hide the oidc-client warning - login_require
    return null;
  }
};

export const login = async (platform: Platform) => {
  const token = await signInSilent(platform);

  if (token === null) {
    userManager.signinRedirect({
      extraQueryParams: {
        identity_provider: platform,
      },
    });
  }
};

export const logout = () => userManager.signoutRedirect();

export const getToken = async () => {
  const user = await userManager.getUser();
  return extractToken(user);
};

export const getDecodedToken = async (): Promise<TokenPayload | null> => {
  const token = await getToken();

  if (token === null) {
    return null;
  }

  const payload = parseJwt(token);

  return {
    userId: parseInt(payload.sub, 10),
    userRole: payload.role,
    expiration: new Date(payload.exp * 1000),
    issuedAt: new Date(payload.iat * 1000),
  };
};
