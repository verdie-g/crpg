import { flushPromises } from '@vue/test-utils';
import { createTestingPinia } from '@pinia/testing';
import { mountWithRouter } from '@/__test__/unit/utils';
import { type Clan } from '@/models/clan';

const CLAN_ID = 1;
const CLAN_FORM = { tag: 'mlp', name: 'My Little Pony New' } as Omit<Clan, 'id'>;
const mockedUpdateClan = vi.fn().mockResolvedValue({ id: CLAN_ID });
vi.mock('@/services/clan-service', () => ({
  updateClan: mockedUpdateClan,
}));

const mockedNotify = vi.fn();
vi.mock('@/services/notification-service', () => ({
  notify: mockedNotify,
}));

const CLAN = {
  id: CLAN_ID,
  tag: 'mlp',
  name: 'My Little Pony',
};
const mockedLoadClan = vi.fn();
const mockedUseClan = vi.fn().mockImplementation(() => ({
  clanId: computed(() => CLAN_ID),
  clan: computed(() => CLAN),
  loadClan: mockedLoadClan,
}));
vi.mock('@/composables/clan/use-clan', () => ({
  useClan: mockedUseClan,
}));

import { useUserStore } from '@/stores/user';
import Page from './update.vue';
const userStore = useUserStore(createTestingPinia());

const routes = [
  {
    name: 'clans-id-update',
    path: '/clans/:id/update',
    component: Page,
    props: true,
  },
  {
    name: 'clans-id',
    path: '/clans/:id',
    component: {
      template: `<div></div>`,
    },
    props: true,
  },
];
const route = {
  name: 'clans-id-update',
  params: {
    id: CLAN_ID,
  },
};

const mountOptions = {
  global: {
    stubs: ['ClanForm', 'RouterLink'],
  },
};

beforeEach(() => {
  userStore.$reset();
});

it('emit - submit', async () => {
  const { wrapper, router } = await mountWithRouter(mountOptions, routes, route);
  const spyRouterReplace = vi.spyOn(router, 'replace');

  const clanFormComponent = wrapper.findComponent({ name: 'ClanForm' });

  await clanFormComponent.vm.$emit('submit', CLAN_FORM);
  await flushPromises();

  expect(mockedUpdateClan).toBeCalledWith(CLAN_ID, { ...CLAN, ...CLAN_FORM });
  expect(userStore.getUserClan).toBeCalled();
  expect(spyRouterReplace).toBeCalledWith({
    name: 'clans-id',
    params: {
      id: CLAN_ID,
    },
  });
  expect(mockedNotify).toBeCalledWith('clan.update.notify.success');
});
