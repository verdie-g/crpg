import { flushPromises } from '@vue/test-utils';
import { createTestingPinia } from '@pinia/testing';
import { mountWithRouter } from '@/__test__/unit/utils';
import { type Clan } from '@/models/clan';
import { type User } from '@/models/user';

const CLAN_FORM = { tag: 'mlp', name: 'My Little Pony New' } as Omit<Clan, 'id'>;
const { CLAN_ID } = vi.hoisted(() => ({ CLAN_ID: 1 }));
const { CLAN } = vi.hoisted(() => ({
  CLAN: {
    id: CLAN_ID,
    tag: 'mlp',
    name: 'My Little Pony',
  },
}));

const { mockedUpdateClan, mockedKickClanMember } = vi.hoisted(() => ({
  mockedUpdateClan: vi.fn().mockResolvedValue({ id: CLAN_ID }),
  mockedKickClanMember: vi.fn(),
}));
vi.mock('@/services/clan-service', () => ({
  updateClan: mockedUpdateClan,
  kickClanMember: mockedKickClanMember,
}));

const { mockedNotify } = vi.hoisted(() => ({ mockedNotify: vi.fn() }));
vi.mock('@/services/notification-service', () => ({
  notify: mockedNotify,
}));

const { mockedUseClan } = vi.hoisted(() => ({
  mockedUseClan: vi.fn().mockImplementation(() => ({
    clanId: computed(() => CLAN_ID),
    clan: computed(() => CLAN),
    loadClan: vi.fn(),
  })),
}));
vi.mock('@/composables/clan/use-clan', () => ({
  useClan: mockedUseClan,
}));

const { mockedUseClanMembers } = vi.hoisted(() => ({
  mockedUseClanMembers: vi.fn().mockImplementation(() => ({
    isLastMember: computed(() => false),
    loadClanMembers: vi.fn(),
  })),
}));
vi.mock('@/composables/clan/use-clan-members', () => ({
  useClanMembers: mockedUseClanMembers,
}));

import { useUserStore } from '@/stores/user';
import Page from './update.vue';

const userStore = useUserStore(createTestingPinia());

const routes = [
  {
    name: 'ClansIdUpdate',
    path: '/clans/:id/update',
    component: Page,
    props: true,
  },
  {
    name: 'ClansId',
    path: '/clans/:id',
    component: {
      template: `<div></div>`,
    },
    props: true,
  },
  {
    name: 'Clans',
    path: '/clans',
    component: {
      template: `<div></div>`,
    },
  },
];
const route = {
  name: 'ClansIdUpdate',
  params: {
    id: String(CLAN_ID),
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
  expect(userStore.getUserClanAndRole).toBeCalled();
  expect(spyRouterReplace).toBeCalledWith({
    name: 'ClansId',
    params: {
      id: CLAN_ID,
    },
  });
  expect(mockedNotify).toBeCalledWith('clan.update.notify.success');
});

describe('delete clan', () => {
  it("it shouldn't be possible to delete a clan if the member is the only", async () => {
    const { wrapper } = await mountWithRouter(mountOptions, routes, route);

    expect(wrapper.find('[data-aq-clan-delete-required-message]').exists()).toStrictEqual(true);
    expect(wrapper.find('[data-aq-clan-delete-confirm-action-form]').exists()).toStrictEqual(false);
  });

  it("it should be possible to delete a clan if the member isn't the only", async () => {
    userStore.user = { id: 1 } as User;

    mockedUseClanMembers.mockImplementationOnce(() => ({
      isLastMember: computed(() => true),
      loadClanMembers: vi.fn(),
    }));

    const { wrapper, router } = await mountWithRouter(mountOptions, routes, route);
    const spyRouterReplace = vi.spyOn(router, 'replace');

    const ConfirmActionForm = wrapper.findComponent({ name: 'ConfirmActionForm' });

    expect(ConfirmActionForm.exists()).toStrictEqual(true);
    expect(wrapper.find('[data-aq-clan-delete-required-message]').exists()).toStrictEqual(false);

    await ConfirmActionForm.vm.$emit('confirm');
    await flushPromises();

    expect(userStore.getUserClanAndRole).toBeCalled();
    expect(mockedNotify).toBeCalledWith('clan.delete.notify.success');
    expect(spyRouterReplace).toBeCalledWith({
      name: 'Clans',
    });
    expect(mockedKickClanMember).toBeCalledWith(1, 1);
  });
});
