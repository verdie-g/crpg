import { flushPromises, mount } from '@vue/test-utils';
import { type Clan } from '@/models/clan';

const mockedNotify = vi.fn();
vi.mock('@/services/notification-service', async () => ({
  ...(await vi.importActual<typeof import('@/services/notification-service')>(
    '@/services/notification-service'
  )),
  notify: mockedNotify,
}));

import ClanForm from './ClanForm.vue';

const CLAN = {
  name: 'My Little Pony',
  tag: 'mlp',
  region: 'Oce',
  description: 'mlp the best',
  primaryColor: '#883e97',
  secondaryColor: '#ee3f96',
  bannerKey: '123456',
  discord: null,
} as Omit<Clan, 'id'>;

describe('create mode', () => {
  it('action buttons', () => {
    const wrapper = mount(ClanForm);

    expect(wrapper.find('[data-aq-clan-form-action="create"]').exists()).toBeTruthy();

    expect(wrapper.find('[data-aq-clan-form-action="cancel"]').exists()).toBeFalsy();
    expect(wrapper.find('[data-aq-clan-form-action="save"]').exists()).toBeFalsy();
  });

  it('validation - notify + submit should not emitted', async () => {
    const wrapper = mount(ClanForm);

    await wrapper.find('[data-aq-clan-form]').trigger('submit.prevent');
    await flushPromises();

    expect(mockedNotify).toBeCalledWith('form.validate.invalid', 'warning');
    expect(wrapper.emitted()).not.toHaveProperty('submit');
  });

  it('validation - name', async () => {
    const wrapper = mount(ClanForm);

    const field = wrapper.findComponent('[data-aq-clan-form-field="name"]');
    const input = wrapper.findComponent('[data-aq-clan-form-input="name"]');

    expect(field.attributes('variant')).not.toBeDefined();
    expect(field.attributes('message')).not.toBeDefined();

    await input.trigger('blur');

    expect(field.attributes('variant')).toEqual('danger');
    expect(field.attributes('message')).toEqual('validations.required');

    await input.setValue('My');

    expect(field.attributes('message')).toEqual('validations.minLength');

    await input.setValue('My Little Pony My Little Pony My Little Pony My Little Pony');

    expect(field.attributes('message')).toEqual('validations.maxLength');

    await input.trigger('focus');

    expect(field.attributes('variant')).not.toBeDefined();
    expect(field.attributes('message')).not.toBeDefined();

    await input.setValue('My Little Pony');
    await input.trigger('blur');

    expect(field.attributes('variant')).not.toBeDefined();
    expect(field.attributes('message')).not.toBeDefined();
  });

  it('validation - tag', async () => {
    const wrapper = mount(ClanForm);

    const field = wrapper.findComponent('[data-aq-clan-form-field="tag"]');
    const input = wrapper.findComponent('[data-aq-clan-form-input="tag"]');

    expect(field.attributes('variant')).not.toBeDefined();
    expect(field.attributes('message')).not.toBeDefined();

    await input.trigger('blur');

    expect(field.attributes('variant')).toEqual('danger');
    expect(field.attributes('message')).toEqual('validations.required');

    await input.setValue('M');

    expect(field.attributes('message')).toEqual('validations.minLength');

    await input.setValue('MLPMLPMLPMLP');

    expect(field.attributes('message')).toEqual('validations.maxLength');

    await input.setValue('!!!!');

    expect(field.attributes('message')).toEqual('validations.clanTagPattern');

    await input.trigger('focus');

    expect(field.attributes('variant')).not.toBeDefined();
    expect(field.attributes('message')).not.toBeDefined();

    await input.setValue('MLP');
    await input.trigger('blur');

    expect(field.attributes('variant')).not.toBeDefined();
    expect(field.attributes('message')).not.toBeDefined();
  });

  it('validation - description', async () => {
    const wrapper = mount(ClanForm);

    const field = wrapper.findComponent('[data-aq-clan-form-field="description"]');
    const input = wrapper.findComponent('[data-aq-clan-form-input="description"]');

    expect(field.attributes('variant')).not.toBeDefined();
    expect(field.attributes('message')).not.toBeDefined();

    await input.trigger('blur');

    expect(field.attributes('variant')).not.toBeDefined();
    expect(field.attributes('message')).not.toBeDefined();

    await input.setValue(
      'Unicorns are one of several kinds of ponies that live in Equestria. They are characterized by their horns and their ability to perform magic. Unicorns are one of several kinds of ponies that live in Equestria. They are characterized by their horns and their ability to perform magic.'
    );

    expect(field.attributes('message')).toEqual('validations.maxLength');

    await input.trigger('focus');

    expect(field.attributes('variant')).not.toBeDefined();
    expect(field.attributes('message')).not.toBeDefined();

    await input.setValue('Unicorns are one of');
    await input.trigger('blur');

    expect(field.attributes('variant')).not.toBeDefined();
    expect(field.attributes('message')).not.toBeDefined();
  });

  it('validation - bannerKey', async () => {
    const wrapper = mount(ClanForm);

    const field = wrapper.findComponent('[data-aq-clan-form-field="bannerKey"]');
    const input = wrapper.findComponent('[data-aq-clan-form-input="bannerKey"]');

    expect(field.attributes('variant')).not.toBeDefined();
    expect(
      wrapper
        .findComponent('[data-aq-i18n-t-stub][keypath="clan.update.bannerKeyGeneratorTools"]')
        .exists()
    ).toBe(true);

    await input.setValue('abc');
    await input.trigger('blur');

    expect(field.find('[data-aq-o-field-stub-message-slot]').text()).toEqual(
      'validations.clanBannerKeyPattern'
    );

    await input.setValue('123');

    expect(field.attributes('variant')).not.toBeDefined();
  });

  it('validation - discord', async () => {
    const wrapper = mount(ClanForm);

    const field = wrapper.findComponent('[data-aq-clan-form-field="discord"]');
    const input = wrapper.findComponent('[data-aq-clan-form-input="discord"]');

    expect(field.attributes('variant')).not.toBeDefined();
    expect(field.attributes('message')).not.toBeDefined();

    await input.trigger('blur');

    expect(field.attributes('variant')).not.toBeDefined();
    expect(field.attributes('message')).not.toBeDefined();

    await input.setValue('google');
    await input.trigger('blur');

    expect(field.attributes('variant')).toBeDefined();
    expect(field.attributes('message')).toEqual('validations.url');

    await input.setValue('https://google.com');

    expect(field.attributes('variant')).toBeDefined();
    expect(field.attributes('message')).toEqual('validations.discordLinkPattern');

    await input.setValue('https://discord.gg/mlp');

    expect(field.attributes('variant')).not.toBeDefined();
    expect(field.attributes('message')).not.toBeDefined();
  });

  it('submit', async () => {
    const wrapper = mount(ClanForm);

    await Promise.all([
      wrapper.find('[data-aq-clan-form-input="name"]').setValue(CLAN.name),
      wrapper.find('[data-aq-clan-form-input="tag"]').setValue(CLAN.tag),
      wrapper.find('[data-aq-clan-form-input="description"]').setValue(CLAN.description),
      wrapper.find(`[data-aq-clan-form-input="region"][value="${CLAN.region}"]`).setValue(),
      wrapper.find('[data-aq-clan-form-input="primaryColor"]').setValue(CLAN.primaryColor),
      wrapper.find('[data-aq-clan-form-input="secondaryColor"]').setValue(CLAN.secondaryColor),
      wrapper.find('[data-aq-clan-form-input="bannerKey"]').setValue(CLAN.bannerKey),
      wrapper.find('[data-aq-clan-form-input="discord"]').setValue(CLAN.discord),
    ]);

    await wrapper.find('[data-aq-clan-form]').trigger('submit.prevent');
    await flushPromises();

    expect(mockedNotify).not.toBeCalled();
    expect(wrapper.emitted('submit')![0][0]).toEqual(CLAN);
  });
});

it('update mode', async () => {
  const NEW_NAME = 'Your Little Pony';
  const NEW_TAG = 'ylp';

  const wrapper = mount(ClanForm, {
    props: {
      clanId: 1,
      clan: CLAN,
    },
  });

  expect(wrapper.find('[data-aq-clan-form-action="create"]').exists()).toBeFalsy();

  expect(wrapper.find('[data-aq-clan-form-action="cancel"]').exists()).toBeTruthy();
  expect(wrapper.find('[data-aq-clan-form-action="save"]').exists()).toBeTruthy();

  await wrapper.find('[data-aq-clan-form-input="name"]').setValue(NEW_NAME);
  await wrapper.find('[data-aq-clan-form-input="tag"]').setValue(NEW_TAG);

  await wrapper.find('[data-aq-clan-form]').trigger('submit.prevent');
  await flushPromises();

  expect(wrapper.emitted('submit')![0][0]).toEqual({ ...CLAN, name: NEW_NAME, tag: NEW_TAG });
});
