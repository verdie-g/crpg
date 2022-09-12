import { User, UserManager } from 'oidc-client';
import Role from '../models/role';
import router from '../router/index';

const userManager = new UserManager({
  authority: process.env.VUE_APP_API_BASE_URL,
  client_id: 'crpg-web-ui', // eslint-disable-line @typescript-eslint/naming-convention
  redirect_uri: window.location.origin, // eslint-disable-line @typescript-eslint/naming-convention
  silent_redirect_uri: `${window.location.origin}/silent-renew.html`, // eslint-disable-line @typescript-eslint/naming-convention
  response_type: 'code', // eslint-disable-line @typescript-eslint/naming-convention
  scope: 'openid offline_access user_api',
  post_logout_redirect_uri: window.location.origin, // eslint-disable-line @typescript-eslint/naming-convention
  // Refresh access token after half of its lifetime (30 minutes)
  accessTokenExpiringNotificationTime: 30 * 60,
  automaticSilentRenew: true,
});

export class TokenPayload {
  public userId: number;
  public userRole: Role;
  public expiration: Date;
  public issuedAt: Date;
}

export async function getToken(): Promise<string | null> {
  const user = await userManager.getUser();
  return user !== null ? user.access_token : null;
}

export async function getDecodedToken(): Promise<TokenPayload | null> {
  const token = await getToken();
  if (token === null) {
    return null;
  }

  const payload = JSON.parse(atob(token.split('.')[1]));
  return {
    userId: parseInt(payload.sub, 10),
    userRole: payload.role,
    expiration: new Date(payload.exp * 1000),
    issuedAt: new Date(payload.iat * 1000),
  };
}

export function signIn(): Promise<void> {
  return userManager.signinRedirect({ state: { url: router.currentRoute.fullPath } });
}

export async function signInSilent(): Promise<string | null> {
  const user = await userManager.signinSilent({ state: { url: router.currentRoute.fullPath } });
  return user !== null ? user.access_token : null;
}

export async function signInCallback(): Promise<User | undefined> {
  const mgr = new UserManager({
    response_mode: 'query', // eslint-disable-line @typescript-eslint/naming-convention
  });

  return mgr.signinRedirectCallback();
}

export function signOut(): Promise<void> {
  return userManager.signoutRedirect();
}
