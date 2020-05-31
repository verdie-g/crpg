const TOKEN_KEY = 'token';
const REDIRECT_URI = window.location.origin;
const API_BASE_URL = process.env.VUE_APP_API_BASE_URL;

export function getToken(): string | undefined {
  return localStorage[TOKEN_KEY];
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
