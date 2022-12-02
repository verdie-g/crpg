import { flushPromises, mount } from '@vue/test-utils';

import ConfirmActionForm from './ConfirmActionForm.vue';

it('confirm action', async () => {
  const NAME = 'Fluttershy';
  const wrapper = mount(ConfirmActionForm, {
    props: {
      title: 'title',
      description: 'description',
      name: NAME,
      confirmLabel: 'confirmLabel',
    },
  });

  const input = wrapper.findComponent('[data-aq-confirm-input]');
  const field = wrapper.findComponent('[data-aq-confirm-field]');
  const submitBtn = wrapper.findComponent('[data-aq-confirm-action="submit"]');
  const cancelBtn = wrapper.findComponent('[data-aq-confirm-action="cancel"]');

  expect(submitBtn.attributes('disabled')).toBeDefined();
  expect(field.attributes('variant')).not.toBeDefined();
  expect(field.attributes('message')).not.toBeDefined();

  await input.trigger('blur');

  expect(field.attributes('variant')).toEqual('danger');
  expect(field.attributes('message')).toEqual('validations.sameAs');

  await input.setValue(NAME);
  await input.trigger('blur');

  expect(submitBtn.attributes('disabled')).not.toBeDefined();
  expect(field.attributes('variant')).not.toBeDefined();
  expect(field.attributes('message')).not.toBeDefined();

  await submitBtn.trigger('click');
  await flushPromises();

  expect(wrapper.emitted()).toHaveProperty('confirm');

  await cancelBtn.trigger('click');

  expect(wrapper.emitted()).toHaveProperty('cancel');
});
