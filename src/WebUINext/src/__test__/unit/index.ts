// https://vitest.dev/config/#setupfiles (1)
// https://github.com/sheremet-va/vi-fetch (2)

import 'vi-fetch/setup';
import { mockFetch } from 'vi-fetch';

mockFetch.setOptions({
  baseUrl: import.meta.env.VITE_API_BASE_URL,
});

beforeEach(() => {
  mockFetch.clearAll();
});

export {};
