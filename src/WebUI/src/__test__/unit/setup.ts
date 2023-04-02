// Mock fetch API
// https://github.com/sheremet-va/vi-fetch
import 'vi-fetch/setup';
import { mockFetch } from 'vi-fetch';

import { config } from '@vue/test-utils';
import { createI18n } from 'vue-i18n';
import Oruga from '@oruga-ui/oruga-next';

import { i18nTMock, i18nNMock, i18nDMock } from '@/__mocks__/i18n';
import mockConstants from '@/__mocks__/constants.json';

const mockMatchMedia = vi.fn().mockImplementation(query => ({
  matches: false,
  media: query,
  onchange: null,
  addListener: vi.fn(), // deprecated
  removeListener: vi.fn(), // deprecated
  addEventListener: vi.fn(),
  removeEventListener: vi.fn(),
  dispatchEvent: vi.fn(),
}));
vi.stubGlobal('matchMedia', mockMatchMedia);

vi.mock(
  '@/services/translate-service',
  vi.fn().mockImplementation(() => ({
    t: i18nTMock,
    n: i18nNMock,
    d: i18nDMock,
  }))
);

vi.mock(
  '@root/data/constants.json',
  vi.fn().mockImplementation(() => mockConstants)
);

mockFetch.setOptions({
  baseUrl: import.meta.env.VITE_API_BASE_URL,
});

const FakeInput = {
  template: `<input :value="modelValue" @input="$emit('update:modelValue', $event.target.value)" />`,
  props: ['modelValue', 'size'],
};

// TODO:
const FakeCheckBox = {
  template: `<input :value="Boolean(modelValue)" @input="$emit('update:modelValue', Boolean($event.target.value))" />`,
  props: ['modelValue'],
};

const FakeRadioBox = {
  template: `<input type="radio" :value="nativeValue" @change="$emit('update:modelValue', $event.target.value)" />`,
  props: ['modelValue', 'nativeValue'],
};

const FakeBtn = {
  template: `<button :type="nativeType" />`,
  props: ['nativeType'],
};

config.global.plugins = [
  Oruga,
  createI18n({ legacy: false, fallbackWarn: false, missingWarn: false }),
];

config.global.stubs = {
  RouterLink: true,
  OButton: FakeBtn,
  OField: {
    template: `<div><div data-aq-o-field-stub-message-slot><slot name="message"/></div><slot /></div>`,
  },
  OInput: FakeInput,
  OSwitch: FakeCheckBox,
  OCheckbox: FakeCheckBox,
  ORadio: FakeRadioBox,
  ODateTimePicker: true,
  OIcon: true,

  OPagination: true,
  //
  OTabs: false,
  OTabItem: false,
  OTable: false,
  OTableColumn: false,

  //
  FontAwesomeLayers: true,
  FontAwesomeIcon: true,

  'i18n-t': {
    template: `<div data-aq-i18n-t-stub><slot /><slot name="link"/></div>`,
  },
  Modal: {
    template: `<div data-aq-modal-stub><slot /><slot name="popper" v-bind="{ hide: () => {} }" /></div>`,
  },
  VDropdown: {
    template: `<div data-aq-vdropdown-stub><slot /><slot name="popper" v-bind="{ hide: () => {} }" /></div>`,
  },
  DropdownItem: true,
};

config.global.directives = {
  tooltip: {
    beforeMount(el: Element) {
      el.setAttribute('data-aq-with-tooltip', 'true');
    },
  },
};

vi.mock('@vueuse/head', () => ({
  useHead: vi.fn(),
}));

beforeEach(() => {
  mockFetch.clearAll();
});
