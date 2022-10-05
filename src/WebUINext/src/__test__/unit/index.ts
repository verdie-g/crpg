// Mock fetch API
// https://github.com/sheremet-va/vi-fetch
import 'vi-fetch/setup';
import { mockFetch } from 'vi-fetch';

mockFetch.setOptions({
  baseUrl: import.meta.env.VITE_API_BASE_URL,
});

beforeEach(() => {
  mockFetch.clearAll();
});
