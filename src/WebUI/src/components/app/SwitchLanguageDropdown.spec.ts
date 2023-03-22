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
  expect(langItems.at(0)!.findComponent({ name: 'FontAwesomeLayers' }).exists()).toBeTruthy();
  expect(langItems.at(0)!.classes('dropdown-item_active')).toBeTruthy();

  expect(langItems.at(1)!.findComponent({ name: 'FontAwesomeLayers' }).exists()).toBeFalsy();
  expect(langItems.at(1)!.classes('dropdown-item_active')).toBeFalsy();

  await langItems.at(1)!.trigger('click');

  expect(mockSwitchLanguage).toBeCalledWith('pony');
});
