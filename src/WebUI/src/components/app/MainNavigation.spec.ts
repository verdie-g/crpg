import { mount } from '@vue/test-utils';
import { createTestingPinia } from '@pinia/testing';
import { useUserStore } from '@/stores/user';
import Role from '@/models/role';
const userStore = useUserStore(createTestingPinia());
import MainNavigation from './MainNavigation.vue';

const getWrapper = () =>
  mount(MainNavigation, {
    global: {
      stubs: ['RouterLink', 'VTooltip'],
    },
  });

describe('Mod link', () => {
  it('role:user - not exist', () => {
    userStore.$patch({ user: { role: Role.User } });

    const wrapper = getWrapper();

    expect(wrapper.find('[data-aq-main-nav-link="Moderator"]').exists()).toBeFalsy();
  });

  it('role:mod', () => {
    userStore.$patch({ user: { role: Role.Moderator } });

    const wrapper = getWrapper();

    expect(wrapper.find('[data-aq-main-nav-link="Moderator"]').exists()).toBeTruthy();
  });
});

describe('clan explanation', () => {
  it('no clan - exist', () => {
    userStore.$patch({ user: { role: Role.User }, clan: null });

    const wrapper = getWrapper();

    expect(wrapper.find('[data-aq-main-nav-link-tooltip="Explanation"]').exists()).toBeTruthy();
  });

  it('with clan - exist', () => {
    userStore.$patch({ user: { role: Role.User }, clan: { id: 1 } });

    const wrapper = getWrapper();

    expect(wrapper.find('[data-aq-main-nav-link-tooltip="Explanation"]').exists()).toBeFalsy();
  });
});
