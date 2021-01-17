import { UserManager, WebStorageStateStore } from 'oidc-client';
import Role from '../models/role';

const userStore = new WebStorageStateStore({});

const userManager = new UserManager({
  authority: process.env.VUE_APP_API_BASE_URL,
  client_id: 'crpg_web_ui', // eslint-disable-line @typescript-eslint/camelcase
  redirect_uri: window.location.origin, // eslint-disable-line @typescript-eslint/camelcase
  response_type: 'code', // eslint-disable-line @typescript-eslint/camelcase
  scope: 'openid offline_access user_api',
  post_logout_redirect_uri: window.location.origin, // eslint-disable-line @typescript-eslint/camelcase
  // Refresh access token after half of its lifetime (30 minutes)
  accessTokenExpiringNotificationTime: 30 * 60,
  automaticSilentRenew: true,
  userStore,
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
  console.log(payload);
  return {
    userId: parseInt(payload.sub, 10),
    userRole: payload.role,
    expiration: new Date(payload.exp * 1000),
    issuedAt: new Date(payload.iat * 1000),
  };
}

export function signIn(): Promise<void> {
  return userManager.signinRedirect().catch(err => {
    console.error(err);
  });
}

export async function signInCallback(): Promise<void> {
  const mgr = new UserManager({
    // eslint-disable-next-line @typescript-eslint/camelcase
    response_mode: 'query',
    userStore,
  });

  await mgr.signinRedirectCallback();
}

export async function signOut(): Promise<void> {
  return userManager.signoutRedirect();
}
