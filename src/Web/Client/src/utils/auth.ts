const TOKEN_KEY = 'token';
const REDIRECT_URI = window.location.origin;
const API_BASE_URI = 'http://localhost:5000/api';

export function getToken(): string {
  return localStorage[TOKEN_KEY];
}

export function setToken(token: string) {
  localStorage[TOKEN_KEY] = token;
}

export function signIn() {
  window.location.href = `${API_BASE_URI}/auth/signIn?redirectUri=${encodeURIComponent(REDIRECT_URI)}`;
}

export function signOut() {
  localStorage.removeItem(TOKEN_KEY);
}

export function isSignedIn(): boolean {
  return getToken() !== undefined;
}
