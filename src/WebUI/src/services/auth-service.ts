import { UserManager, WebStorageStateStore, type User } from 'oidc-client-ts';
import Role from '@/models/role';

interface TokenPayload {
  userId: number;
  userRole: Role;
  expiration: Date;
  issuedAt: Date;
}

interface WithAccesToken extends Pick<User, 'access_token'> {}

export const extractToken = (user: WithAccesToken | null): string | null =>
  user !== null ? user.access_token : null;

export const parseJwt = (token: string) =>
  JSON.parse(Buffer.from(token.split('.')[1], 'base64').toString());

export const userManager = new UserManager({
  authority: import.meta.env.VITE_API_BASE_URL,
  scope: 'openid offline_access user_api',
  client_id: 'crpg-web-ui',
  redirect_uri: window.location.origin + '/signin-callback',
  silent_redirect_uri: window.location.origin + '/signin-silent-callback',
  post_logout_redirect_uri: window.location.origin,
  response_type: 'code',
  userStore: new WebStorageStateStore({ store: window.localStorage }),
});

export const getUser = () => userManager.getUser();

export const signInSilent = async () => {
  try {
    const user = await userManager.signinSilent();
    return extractToken(user);
  } catch (error) {
    // hide the oidc-client warning - login_require
    return null;
  }
};

export const login = async (): Promise<void> => {
  const token = await signInSilent();

  if (!token) {
    userManager.signinRedirect();
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
