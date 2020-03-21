const TOKEN_KEY = 'token';
const REDIRECT_URI = window.location.origin;
const API_BASE_URI = 'http://localhost:8000/api';

export function getToken(): string | undefined {
  return localStorage[TOKEN_KEY];
}

export function setToken(token: string) {
  localStorage[TOKEN_KEY] = token;
}

export function challenge() {
  window.location.href = `${API_BASE_URI}/auth/signIn?redirectUri=${encodeURIComponent(REDIRECT_URI)}`;
}

export function clearToken() {
  localStorage.removeItem(TOKEN_KEY);
}
