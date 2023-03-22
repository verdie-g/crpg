import { nextTick } from 'vue';
import { i18n } from '@/boot/i18n';

const defaultLocale = () => import.meta.env.VITE_LOCALE_DEFAULT;

export const supportedLocales = () => import.meta.env.VITE_LOCALE_SUPPORTED.split(',');

const localStorageKey = 'user-locale';

export const currentLocale = () => i18n.global.locale.value as string;

const setCurrentLocale = (locale: any) => {
  i18n.global.locale.value = locale;
};

export const switchLanguage = async (locale: any) => {
  await loadLocaleMessages(locale);

  setCurrentLocale(locale);

  document.querySelector('html')!.setAttribute('lang', locale);
  window.localStorage.setItem(localStorageKey, locale);
};

const loadLocaleMessages = async (locale: any) => {
  if (!i18n.global.availableLocales.includes(locale)) {
    const messages = await import(`../../locales/${locale}.yml`);
    i18n.global.setLocaleMessage(locale, messages.default);
  }

  return nextTick();
};

const isLocaleSupported = (locale: string) => supportedLocales().includes(locale);

const getUserLocale = () => window.navigator.language.split('-')[0];

const getPersistedLocale = () => {
  const persistedLocale = localStorage.getItem(localStorageKey);

  if (persistedLocale && isLocaleSupported(persistedLocale)) {
    return persistedLocale;
  }

  return null;
};

export const guessDefaultLocale = () => {
  const userPersistedLocale = getPersistedLocale();

  if (userPersistedLocale !== null && isLocaleSupported(userPersistedLocale)) {
    return userPersistedLocale;
  }

  const userPreferredLocale = getUserLocale();

  if (isLocaleSupported(userPreferredLocale)) {
    return userPreferredLocale;
  }

  return defaultLocale();
};

// @ts-ignore
export const t = i18n.global.t;

export const n = i18n.global.n;
export const d = i18n.global.d;
