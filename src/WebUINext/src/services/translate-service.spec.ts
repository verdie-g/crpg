const mockSetAttribute = vi.fn();
document.querySelector = vi.fn().mockReturnValue({
  setAttribute: mockSetAttribute,
} as unknown as HTMLElement);

const spyGetItem = vi.spyOn(Storage.prototype, 'getItem');
const spySetItem = vi.spyOn(Storage.prototype, 'setItem');
const spyLanguageGetter = vi.spyOn(window.navigator, 'language', 'get');

const mockSetLocaleMessage = vi.fn();
const mockLocale = {
  value: '',
};
const mockedSetter = vi.fn();
Object.defineProperty(mockLocale, 'value', {
  set: mockedSetter,
});
vi.mock(
  '@/boot/i18n',
  vi.fn().mockImplementation(() => ({
    i18n: {
      global: {
        locale: mockLocale,
        availableLocales: ['en'],
        setLocaleMessage: mockSetLocaleMessage,
      },
    },
  }))
);

vi.mock('../../locales/en.yml', () => ({
  default: {
    pony: 'Fluttershy',
  },
}));

vi.mock('../../locales/ru.yml', () => ({
  default: {
    pony: 'Applejack',
  },
}));

vi.mock('@/services/translate-service', async () => ({
  ...(await vi.importActual<typeof import('@/services/translate-service')>(
    '@/services/translate-service'
  )),
}));

import { switchLanguage, guessDefaultLocale } from '@/services/translate-service';

describe('switch language', () => {
  it('locale is already loaded', async () => {
    const NEW_LANG = 'en';
    await switchLanguage(NEW_LANG);

    expect(mockSetLocaleMessage).not.toBeCalled();
    expect(mockedSetter).toBeCalledWith(NEW_LANG);
    expect(mockSetAttribute).toBeCalledWith('lang', NEW_LANG);
    expect(spySetItem).toBeCalledWith('user-locale', NEW_LANG);
  });

  it('locale is not yet loaded', async () => {
    const NEW_LANG = 'ru';
    await switchLanguage(NEW_LANG);

    expect(mockSetLocaleMessage).toBeCalledWith(NEW_LANG, expect.any(Object));
    expect(mockedSetter).toBeCalledWith(NEW_LANG);
    expect(mockSetAttribute).toBeCalledWith('lang', NEW_LANG);
    expect(spySetItem).toBeCalledWith('user-locale', NEW_LANG);
  });
});

describe('guess default locale', () => {
  afterEach(() => {
    spyGetItem.mockClear();
    spyLanguageGetter.mockClear();
    localStorage.clear();
  });

  describe('storage is empty', () => {
    it('navigator lang suported - should be navigator lang', () => {
      spyLanguageGetter.mockReturnValue('ru-RU');

      const result = guessDefaultLocale();

      expect(spyGetItem).toBeCalledWith('user-locale');
      expect(result).toEqual('ru');
    });

    it('navigator lang unsuported - should be default lang', () => {
      spyLanguageGetter.mockReturnValue('de-DE');

      const result = guessDefaultLocale();

      expect(spyGetItem).toBeCalledWith('user-locale');
      expect(result).toEqual(import.meta.env.VITE_LOCALE_DEFAULT);
    });
  });

  describe('storage isn`t empty', () => {
    it('lang suported - should be persisted lang', () => {
      localStorage.setItem('user-locale', 'ru');

      const result = guessDefaultLocale();

      expect(spyGetItem).toBeCalledWith('user-locale');
      expect(result).toEqual('ru');
    });

    it('lang unsuported - should be default lang', () => {
      spyLanguageGetter.mockReturnValue('de-DE');
      localStorage.setItem('user-locale', 'de');

      const result = guessDefaultLocale();

      expect(spyGetItem).toBeCalledWith('user-locale');
      expect(result).toEqual(import.meta.env.VITE_LOCALE_DEFAULT);
    });
  });
});
