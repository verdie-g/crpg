import { createTestingPinia } from '@pinia/testing';
import { mount } from '@vue/test-utils';
import { useUserStore } from '@/stores/user';
import { login } from '@/services/auth-service';
import { Platform } from '@/models/platform';

// Test may seem fragile because it depends on @/models/platform. If you wish, you can rewrite it using mock:
// vi.mock('@/models/platform', () => ({
//   get Platform() {
//     return { Steam: 'Steam', MyLittlePonyGamePlatform: 'MyLittlePonyGamePlatform' };
//   },
// }));

/*
  TODO: FIXME:
  All the mocks are broken (in other tests as well), figure it out: https://github.com/vitest-dev/vitest
  Error: [vitest] There was an error when mocking a module. If you are using "vi.mock" factory, make sure there are no top level variables inside, since this call is hoisted to top of the file. Read more: https://vitest.dev/api/#vi-mock
*/
vi.mock('@/services/auth-service', () => ({
  login: vi.fn(),
}));

import Login from './Login.vue';
const userStore = useUserStore(createTestingPinia());

describe('not logged in', () => {
  it('should be exist login btn', () => {
    const wrapper = mount(Login);

    expect(wrapper.find('[data-aq-login-btn]').exists()).toBeTruthy();
    expect(wrapper.find('[data-aq-character-link]').exists()).toBeFalsy();
  });

  it('should called login method when login btn clicked', async () => {
    const wrapper = mount(Login);

    const loginBtn = wrapper.find('[data-aq-login-btn]');
    expect(wrapper.find('[data-aq-character-link]').exists()).toBeFalsy();

    await loginBtn.trigger('click');
    expect(login).toBeCalledWith(Platform.Steam); // default platform
  });

  it('should render a few platform items', () => {
    const wrapper = mount(Login);

    const platformItems = wrapper.findAll('[data-aq-platform-item]');

    expect(platformItems.at(0)!.attributes('checked')).toEqual('true');
    expect(platformItems.at(1)!.attributes('checked')).toEqual('false');
  });

  it('change platform, then login', async () => {
    const wrapper = mount(Login);

    const platformItems = wrapper.findAll('[data-aq-platform-item]');

    await platformItems.at(1)!.trigger('click');

    const loginBtn = wrapper.find('[data-aq-login-btn]');
    await loginBtn.trigger('click');
    expect(login).toBeCalledWith(Platform.EpicGames);
  });
});

describe('logged in', () => {
  it("should be exist link to character's page", () => {
    userStore.$patch({ user: { id: 1 } });

    const wrapper = mount(Login);

    expect(wrapper.find('[data-aq-login-btn]').exists()).toBeFalsy();
    expect(wrapper.find('[data-aq-character-link]').exists()).toBeTruthy();
  });
});
