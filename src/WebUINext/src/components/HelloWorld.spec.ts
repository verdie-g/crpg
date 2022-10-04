import { mount } from '@vue/test-utils';
import HelloWorld from './HelloWorld.vue';

it('displays message', () => {
  const wrapper = mount(HelloWorld, {
    props: {
      msg: 'Hello world',
    },
  });

  expect(wrapper.text()).toContain('Hello world');

  expect(wrapper.html()).toMatchSnapshot();
});
