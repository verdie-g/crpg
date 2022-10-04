import { mount } from '@vue/test-utils';

// The component to test
const Component = {
  template: '<p>{{ msg }}</p>',
  props: ['msg'],
};

test('displays message', () => {
  const wrapper = mount(Component, {
    props: {
      msg: 'Hello world',
    },
  });

  // Assert the rendered text of the component
  expect(wrapper.text()).toContain('Hello world');
});
