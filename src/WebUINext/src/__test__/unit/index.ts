// Mock fetch API
// https://github.com/sheremet-va/vi-fetch
import 'vi-fetch/setup';
import { mockFetch } from 'vi-fetch';
import { config } from '@vue/test-utils';
import { createI18n } from 'vue-i18n';

mockFetch.setOptions({
  baseUrl: import.meta.env.VITE_API_BASE_URL,
});

config.global.plugins = [createI18n({ silentTranslationWarn: true, silentFallbackWarn: true })];

beforeEach(() => {
  mockFetch.clearAll();
});
