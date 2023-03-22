import { mount } from '@vue/test-utils';
import Platform from '@/models/platform';
import { ClanMemberRole } from '@/models/clan';
import ClanMemberDetail from './ClanMemberDetail.vue';

const mockedHide = vi.fn();

const getWrapper = (props: any) => {
  return mount(ClanMemberDetail, {
    shallow: true,
    props,
    global: {
      renderStubDefaultSlot: true,
      stubs: {
        'i18n-t': {
          template: `<div data-aq-i18n-t-stub>
                      <slot name="memberLink"/>
                      <slot name="memberName"/>
                    </div>`,
        },

        Modal: {
          setup() {
            return {
              hide: mockedHide,
            };
          },
          template: `<div data-aq-modal-stub>
                      <slot v-bind="{ hide }" />
                      <slot name="popper" v-bind="{ hide }" />
                    </div>`,
        },
      },
    },
  });
};

const USER = {
  id: 1,
  platform: Platform.Steam,
  platformUserId: '12345',
  name: 'Fluttershy',
  avatar: '',
};

const MEMBER = {
  user: USER,
  role: ClanMemberRole.Member,
};

const OFFICER = {
  user: USER,
  role: ClanMemberRole.Officer,
};

const LEADER = {
  user: USER,
  role: ClanMemberRole.Leader,
};

it('Member, canUpdate:false, canKick:true - kick, cancelKick should be emitted', async () => {
  const wrapper = getWrapper({
    member: MEMBER,
    canUpdate: false,
    canKick: true,
  });

  await wrapper.find('[data-aq-clan-member-action="kick"]').trigger('click');
  expect(wrapper.emitted('kick')).toHaveLength(1);

  await wrapper.find('[data-aq-clan-member-action="kick-cancel"]').trigger('click');
  expect(wrapper.emitted('kick')).toHaveLength(1);

  expect(mockedHide).toHaveBeenCalledTimes(2);
});

describe('Member, canUpdate:true, canKick:true - update role', () => {
  it('Member -> Officer - should be emitted update', async () => {
    const wrapper = getWrapper({
      member: MEMBER,
      canUpdate: true,
      canKick: true,
    });

    await wrapper.find('input[type=radio][value=Officer]').setValue();
    await wrapper.find('[data-aq-clan-member-action="save"]').trigger('click');
    expect(wrapper.emitted().update[0]).toEqual(['Officer']);
  });

  it('Member -> Leader - should be open confirm dialog', async () => {
    const wrapper = getWrapper({
      member: MEMBER,
      canUpdate: true,
      canKick: true,
    });

    const ConfirmActionForm = wrapper.findComponent({ name: 'ConfirmActionForm' });

    expect(
      wrapper.find('[data-aq-clan-member-action="confirm-transfer-dialog"]').attributes('shown')
    ).toEqual('false');

    await wrapper.find('input[type=radio][value=Leader]').setValue();
    await wrapper.find('[data-aq-clan-member-action="save"]').trigger('click');

    expect(
      wrapper.find('[data-aq-clan-member-action="confirm-transfer-dialog"]').attributes('shown')
    ).toEqual('true');

    expect(ConfirmActionForm.props('name')).toEqual(USER.name);

    await ConfirmActionForm.trigger('confirm');

    expect(wrapper.emitted().update[0]).toEqual(['Leader']);
    expect(mockedHide).toHaveBeenCalledTimes(1);

    await ConfirmActionForm.trigger('cancel');
    expect(mockedHide).toHaveBeenCalledTimes(2);
  });

  it('Member -> Member - save btn should be disabled', async () => {
    const wrapper = getWrapper({
      member: MEMBER,
      canUpdate: true,
      canKick: true,
    });

    await wrapper.find('input[type=radio][value=Member]').setValue();
    expect(
      wrapper.find('[data-aq-clan-member-action="save"]').attributes('disabled')
    ).toBeDefined();
  });
});

it('cancel btn - cancel should be emitted', async () => {
  const wrapper = getWrapper({
    member: MEMBER,
    canUpdate: true,
    canKick: true,
  });

  await wrapper.find('[data-aq-clan-member-action="close-detail"]').trigger('click');
  expect(wrapper.emitted('cancel')).toHaveLength(1);
});
