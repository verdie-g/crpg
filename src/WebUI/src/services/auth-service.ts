import Role from '../models/role';

const TOKEN_KEY = 'token';
const REDIRECT_URI = window.location.origin;
const API_BASE_URL = process.env.VUE_APP_API_BASE_URL;

export class TokenPayload {
  public userId: number;
  public userRole: Role;
  public expiration: Date;
  public issuedAt: Date;
}

export function getToken(): string | undefined {
  return localStorage[TOKEN_KEY];
}

export function getDecodedToken(): TokenPayload | undefined {
  const token = getToken();
  if (token === undefined) {
    return undefined;
  }

  const payload = JSON.parse(atob(token.split('.')[1]));
  return {
    userId: parseInt(payload.nameid, 10),
    userRole: payload.role,
    expiration: new Date(payload.exp * 1000),
    issuedAt: new Date(payload.iat * 1000),
  };
}

export function setToken(token: string): void {
  localStorage[TOKEN_KEY] = token;
}

export function clearToken(): void {
  localStorage.removeItem(TOKEN_KEY);
}

export function challenge(): void {
  clearToken();
  window.location.href = `${API_BASE_URL}/auth/signIn?redirectUri=${encodeURIComponent(REDIRECT_URI)}`;
}
