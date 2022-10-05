import { mount } from '@vue/test-utils';
import { createTestingPinia, TestingOptions } from '@pinia/testing';
import { useUserStore } from '@/stores/example';

import Example from './Example.vue';

const factory = (options?: TestingOptions) => {
  const wrapper = mount(Example, {
    props: {
      msg: 'Applejack',
    },
    shallow: true,
    global: {
      plugins: [createTestingPinia(options)],
    },
  });

  const store = useUserStore();
  return { wrapper, store };
};

describe('Example component unit testing', () => {
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

  // https://vitest.dev/guide/snapshot.html
  it('snapshot', async () => {
    const { wrapper } = factory();

    // for update snapshot, use `-- -u`  ex: npm run test:unit src/components/ -- -u
    expect(wrapper.html()).toMatchSnapshot();
  });
});
