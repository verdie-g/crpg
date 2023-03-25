import { createI18n } from 'vue-i18n';
import { type BootModule } from '@/types/boot-module';

// @ts-ignore
import en from '../../locales/en.yml';

export const i18n = createI18n({
  legacy: false,
  locale: import.meta.env.VITE_LOCALE_DEFAULT,
  fallbackLocale: import.meta.env.VITE_LOCALE_FALLBACK,
  globalInjection: true,
  // fallbackWarn: false,
  missingWarn: false, // FIXME: TODO:
  messages: { en },
  pluralRules: {
    ru: (choice: number, choicesLength: number) => {
      if (choice === 0) {
        return 0;
      }

      const teen = choice > 10 && choice < 20;
      const endsWithOne = choice % 10 === 1;
      if (!teen && endsWithOne) {
        return 1;
      }
      if (!teen && choice % 10 >= 2 && choice % 10 <= 4) {
        return 2;
      }

      return choicesLength < 4 ? 2 : 3;
    },
  },
  numberFormats: {
    en: {
      second: {
        style: 'unit',
        unit: 'second',
        unitDisplay: 'narrow',
        maximumFractionDigits: 3,
      },
      percent: {
        style: 'percent',
        minimumFractionDigits: 2,
      },
      decimal: {
        style: 'decimal',
        maximumFractionDigits: 3,
      },
    },
    ru: {
      second: {
        style: 'unit',
        unit: 'second',
        unitDisplay: 'narrow',
        maximumFractionDigits: 3,
      },
      percent: {
        style: 'percent',
        minimumFractionDigits: 2,
      },
      decimal: {
        style: 'decimal',
        maximumFractionDigits: 3,
      },
    },
  },
  datetimeFormats: {
    en: {
      time: {
        hour: 'numeric',
        minute: 'numeric',
      },
      short: {
        dateStyle: 'short',
      },
      long: {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        weekday: 'short',
        hour: 'numeric',
        minute: 'numeric',
      },
    },
    ru: {
      time: {
        hour: 'numeric',
        minute: 'numeric',
      },
      short: {
        dateStyle: 'short',
      },
      long: {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        weekday: 'short',
        hour: 'numeric',
        minute: 'numeric',
      },
    },
  },
});

export const install: BootModule = app => {
  app.use(i18n);
};
