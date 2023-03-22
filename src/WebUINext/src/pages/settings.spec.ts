import { createTestingPinia } from '@pinia/testing';
import { mount } from '@vue/test-utils';
import { Region } from '@/models/region';
import { User } from '@/models/user';

const mockedNotify = vi.fn();
vi.mock('@/services/notification-service', () => ({
  notify: mockedNotify,
}));

const mockedDeleteUser = vi.fn();
vi.mock('@/services/users-service', () => ({
  deleteUser: mockedDeleteUser,
}));

const mockedLogout = vi.fn();
vi.mock('@/services/auth-service', () => ({
  logout: mockedLogout,
}));

import { useUserStore } from '@/stores/user';

const userStore = useUserStore(createTestingPinia());
userStore.user = { region: Region.Eu } as User;

import Page from './settings.vue';

const mockedHide = vi.fn();

it('delete user', async () => {
  const USER_NAME = 'Fluttershy';
  userStore.user = { name: USER_NAME } as User;
  const wrapper = mount(Page, {
    shallow: true,
    global: {
      stubs: {
        'i18n-t': {
          template: `<div data-aq-i18n-t-stub>
                      <slot name="link"/>
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

  const ConfirmActionForm = wrapper.findComponent({ name: 'ConfirmActionForm' });
  expect(ConfirmActionForm.props('name')).toEqual(USER_NAME);

  await ConfirmActionForm.trigger('cancel');
  expect(mockedHide).toHaveBeenCalledTimes(1);

  await ConfirmActionForm.trigger('confirm');

  expect(mockedDeleteUser).toBeCalled();
  expect(mockedNotify).toBeCalledWith('user.settings.delete.notify.success');
  expect(mockedLogout).toBeCalled();
  expect(mockedHide).toHaveBeenCalledTimes(2);
});
