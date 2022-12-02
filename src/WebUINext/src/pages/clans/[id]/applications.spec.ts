import { flushPromises } from '@vue/test-utils';
import { mountWithRouter } from '@/__test__/unit/utils';

const CLAN_ID = 1;
const mockedGetClanInvitations = vi.fn().mockResolvedValue([
  {
    id: 1,
    invitee: {
      id: 13,
      platform: 'Steam',
      platformUserId: '',
      name: 'Applejack',
      avatar: '',
    },
    inviter: {},
    type: 'Request',
    status: 'Pending',
  },
  {
    id: 2,
    invitee: {
      id: 12,
      platform: 'Steam',
      platformUserId: '',
      name: 'Fluttershy',
      avatar: '',
    },
    inviter: {},
    type: 'Request',
    status: 'Pending',
  },
]);

const mockedRespondToClanInvitation = vi.fn();
vi.mock('@/services/clan-service', () => ({
  getClanInvitations: mockedGetClanInvitations,
  respondToClanInvitation: mockedRespondToClanInvitation,
}));

const mockedUseClan = vi.fn().mockImplementation(() => ({
  clanId: computed(() => CLAN_ID),
}));
vi.mock('@/composables/clan/use-clan', () => ({
  useClan: mockedUseClan,
}));

const mockedNotify = vi.fn();
vi.mock('@/services/notification-service', () => ({
  notify: mockedNotify,
}));

import Page from './applications.vue';

const routes = [
  {
    name: 'clans-id-applications',
    path: '/clans/:id/applications',
    component: Page,
    props: true,
  },
];
const route = {
  name: 'clans-id-applications',
  params: {
    id: CLAN_ID,
  },
};

const mountOptions = {
  global: {
    stubs: ['RouterLink', 'UserMedia'],
  },
};

it('respond - apply', async () => {
  const { wrapper } = await mountWithRouter(mountOptions, routes, route);

  const rows = wrapper.findAll('tr');
  await rows.at(1)!.find('[data-aq-clan-application-action="apply"]').trigger('click');
  await flushPromises();

  expect(mockedRespondToClanInvitation).toBeCalledWith(CLAN_ID, 1, true);
  expect(mockedGetClanInvitations).toBeCalledTimes(2);
  expect(mockedNotify).toBeCalledWith('clan.application.respond.accept.notify.success');
});

it('respond - decline', async () => {
  const { wrapper } = await mountWithRouter(mountOptions, routes, route);

  const rows = wrapper.findAll('tr');
  await rows.at(2)!.find('[data-aq-clan-application-action="decline"]').trigger('click');
  await flushPromises();

  expect(mockedRespondToClanInvitation).toBeCalledWith(CLAN_ID, 2, false);
  expect(mockedGetClanInvitations).toBeCalledTimes(2);
  expect(mockedNotify).toBeCalledWith('clan.application.respond.decline.notify.success');
});
