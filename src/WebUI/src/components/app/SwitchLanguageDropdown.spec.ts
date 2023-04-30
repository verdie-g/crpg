import { mount } from '@vue/test-utils';

const mockSupportedLocales = vi.fn().mockReturnValue(['unicorn', 'pony']);
const mockSwitchLanguage = vi.fn();
const mockCurrentLocale = vi.fn().mockReturnValue('unicorn');

vi.mock('@/services/translate-service', () => ({
  supportedLocales: mockSupportedLocales,
  currentLocale: mockCurrentLocale,
  switchLanguage: mockSwitchLanguage,
}));

import SwitchLanguageDropdown from './SwitchLanguageDropdown.vue';

it('switch lang', async () => {
  const wrapper = mount(SwitchLanguageDropdown);

  const langItems = wrapper.findAll('[data-aq-switch-lang-item]');

  // checked icon
  expect(langItems.at(0)!.attributes('checked')).toEqual('true');
  expect(langItems.at(1)!.attributes('checked')).toEqual('false');

  await langItems.at(1)!.trigger('click');

  expect(mockSwitchLanguage).toBeCalledWith('pony');
});
