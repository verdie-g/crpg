import { mount } from '@vue/test-utils';
import { createTestingPinia, TestingOptions } from '@pinia/testing';
import { useUserStore } from '@/stores/example';
import { VueWaitKey } from '@/boot/wait';
import MockWait from '@/__mocks__/wait';

import Example from './Example.vue';

// START MOCKS
const mockedWait = new MockWait();

const TEST_PATH = '/test-path';
const mockedPush = vi.fn();

// FIXME: https://github.com/vuejs/router/issues/1466
vi.mock('vue-router/dist/vue-router.mjs', () => ({
  useRoute: vi.fn().mockImplementation(() => ({
    path: TEST_PATH,
  })),
  useRouter: vi.fn().mockImplementation(() => ({
    push: mockedPush,
  })),
}));
// END MOCKS

const factory = (options?: TestingOptions) => {
  const wrapper = mount(Example, {
    props: {
      msg: 'Applejack',
    },
    shallow: true,
    global: {
      provide: {
        [VueWaitKey as symbol]: mockedWait,
      },
      plugins: [createTestingPinia(options)],
    },
  });

  const store = useUserStore();
  return { wrapper, store };
};

describe('Example component unit test', () => {
  it('basic', () => {
    const { wrapper } = factory();

    expect(wrapper.find('[data-aq-example-prop]').text()).toEqual('Applejack');
    expect(wrapper.find('[data-aq-example-ref]').text()).toEqual('example cmp');
    expect(wrapper.find('[data-aq-example-computed]').text()).toEqual('Applejack - example cmp');
  });

  // https://test-utils.vuejs.org/guide/essentials/event-handling.html
  it('emit', async () => {
    const { wrapper } = factory();

    await wrapper.find('[data-aq-example-emit]').trigger('click');

    expect(wrapper.emitted('example:changed')).toHaveLength(1);
    expect(wrapper.emitted('example:changed')?.[0]).toEqual([
      {
        message: 'Applejack',
        superMessage: 'Applejack - example cmp',
      },
    ]);
  });

  it('provide/inject', async () => {
    const { wrapper } = factory();

    await wrapper.find('[data-aq-example-method-with-provide-inject]').trigger('click');

    expect(wrapper.find('[data-aq-example-ref]').text()).toEqual('changed example cmp');

    expect(mockedWait.start).toHaveBeenCalled();
    expect(mockedWait.end).toHaveBeenCalled();
    expect(mockedWait.is).toHaveBeenCalled();
  });

  // https://test-utils.vuejs.org/guide/advanced/reusability-composition.html
  it('with composable', async () => {
    const { wrapper } = factory();

    const composableNodeEl = wrapper.find('[data-aq-example-composable]');

    expect(composableNodeEl.text()).toEqual('0');

    await wrapper.find('[data-aq-example-composable-method]').trigger('click');

    expect(composableNodeEl.text()).toEqual('1');
  });

  // https://pinia.vuejs.org/cookbook/testing.html#unit-testing-components
  it('with store', async () => {
    const { wrapper, store } = factory({
      initialState: { user: { name: 'Twilight Sparkle', role: 'Royalty' } },
    });

    const storeStateNodeEl = wrapper.find('[data-aq-example-store-state]');
    const storeGetterNodeEl = wrapper.find('[data-aq-example-store-getter]');

    expect(storeStateNodeEl.text()).toEqual('Twilight Sparkle');
    expect(storeGetterNodeEl.text()).toEqual('Twilight Sparkle Royalty');

    await wrapper.find('[data-aq-example-store-action]').trigger('click');

    expect(store.fetch).toHaveBeenCalledTimes(1);
  });

  // https://test-utils.vuejs.org/guide/advanced/vue-router.html#using-a-mocked-router-with-composition-api
  it('with route', () => {
    const { wrapper } = factory();

    expect(wrapper.find('[data-aq-example-route-path]').text()).toEqual(TEST_PATH);
  });

  it('with router', async () => {
    const { wrapper } = factory();

    await wrapper.find('[data-aq-example-router-push]').trigger('click');
    expect(mockedPush).toHaveBeenCalledTimes(1);
    expect(mockedPush).toBeCalledWith({ name: 'index' });
  });

  // https://stackoverflow.com/questions/72260793/best-way-to-mock-stub-vue-i18n-translations-in-a-vue3-component-when-using-vites/73630072#73630072
  it('with i18n', async () => {
    const { wrapper } = factory();

    expect(wrapper.find('[data-aq-example-i18n]').text()).toEqual('button.back');
  });

  // https://vitest.dev/guide/snapshot.html
  it('snapshot', async () => {
    const { wrapper } = factory();

    // for update snapshot, use `-- -u`  ex: npm run test:unit src/components/ -- -u
    expect(wrapper.html()).toMatchSnapshot();
  });
});
