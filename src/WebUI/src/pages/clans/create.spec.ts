import { flushPromises } from '@vue/test-utils';
import { createTestingPinia } from '@pinia/testing';
import { mountWithRouter } from '@/__test__/unit/utils';
import { type Clan } from '@/models/clan';

const NEW_CLAN_ID = 2;
const NEW_CLAN_FORM = { tag: 'mlp', name: 'My Little Pony' } as Omit<Clan, 'id'>;
const mockedCreateClan = vi.fn().mockResolvedValue({ id: NEW_CLAN_ID });
vi.mock('@/services/clan-service', () => ({
  createClan: mockedCreateClan,
}));

const mockedNotify = vi.fn();
vi.mock('@/services/notification-service', () => ({
  notify: mockedNotify,
}));

import { useUserStore } from '@/stores/user';
import Page from './create.vue';
const userStore = useUserStore(createTestingPinia());

const routes = [
  {
    name: 'clans-create',
    path: '/clans-create',
    component: Page,
  },
  {
    name: 'clans-id',
    path: '/clans/:id',
    component: {
      template: `<div></div>`,
    },
  },
];

const route = {
  name: 'clans-create',
};

const mountOptions = {
  global: {
    stubs: ['ClanForm'],
  },
};

it('emit - submit', async () => {
  const { wrapper, router } = await mountWithRouter(mountOptions, routes, route);
  const spyRouterReplace = vi.spyOn(router, 'replace');

  const clanFormComponent = wrapper.findComponent({ name: 'ClanForm' });

  await clanFormComponent.vm.$emit('submit', NEW_CLAN_FORM);
  await flushPromises();

  expect(mockedCreateClan).toBeCalledWith(NEW_CLAN_FORM);
  expect(userStore.getUserClanAndRole).toBeCalled();
  expect(spyRouterReplace).toBeCalledWith({
    name: 'ClansId',
    params: {
      id: NEW_CLAN_ID,
    },
  });
  expect(mockedNotify).toBeCalledWith('clan.create.notify.success');
});
