import { flushPromises } from '@vue/test-utils';
import { createTestingPinia } from '@pinia/testing';
import { mountWithRouter } from '@/__test__/unit/utils';
import { type User } from '@/models/user';
import { type Clan } from '@/models/clan';

const CLAN_LEADER_USER_ID = 1;
const CLAN_OFFICER_USER_ID = 2;
const CLAN_MEMBER_USER_ID = 3;
const NO_CLAN_USER_ID = 4;

const CLAN_MEMBERS = [
  {
    user: {
      id: CLAN_LEADER_USER_ID,
      platform: 'Steam',
      platformUserId: '122341242562',
      name: 'Rarity',
      avatar: 'test-avatar',
    },
    role: 'Leader',
  },
  {
    user: {
      id: CLAN_OFFICER_USER_ID,
      platform: 'Steam',
      platformUserId: '121313',
      name: 'Fluttershy',
      avatar: 'test-avatar-2',
    },
    role: 'Officer',
  },
  {
    user: {
      id: CLAN_MEMBER_USER_ID,
      platform: 'Steam',
      platformUserId: '121313',
      name: 'Applejack',
      avatar: 'test-avatar-3',
    },
    role: 'Member',
  },
];

const CLAN_LEADER = CLAN_MEMBERS.find(m => m.user.id === CLAN_LEADER_USER_ID)!;
const CLAN_OFFICER = CLAN_MEMBERS.find(m => m.user.id === CLAN_OFFICER_USER_ID)!;
const CLAN_MEMBER = CLAN_MEMBERS.find(m => m.user.id === CLAN_MEMBER_USER_ID)!;

const mockedGetClanMembers = vi.fn().mockResolvedValue(CLAN_MEMBERS);
const mockedGetClanMember = vi.fn().mockReturnValue(null);
const mockedCanManageApplicationsValidate = vi.fn().mockReturnValue(false);
const mockedCanUpdateClanValidate = vi.fn().mockReturnValue(false);
const mockedCanKickMemberValidate = vi.fn().mockReturnValue(false);
const mockedInviteToClan = vi.fn();
const mockedUpdateClanMember = vi.fn();
const mockedKickClanMember = vi.fn();
const mockedCanUpdateMemberValidate = vi.fn().mockReturnValue(false);
vi.mock('@/services/clan-service', () => ({
  getClanMembers: mockedGetClanMembers,
  getClanMember: mockedGetClanMember,
  canManageApplicationsValidate: mockedCanManageApplicationsValidate,
  canUpdateClanValidate: mockedCanUpdateClanValidate,
  inviteToClan: mockedInviteToClan,
  updateClanMember: mockedUpdateClanMember,
  canKickMemberValidate: mockedCanKickMemberValidate,
  kickClanMember: mockedKickClanMember,
  canUpdateMemberValidate: mockedCanUpdateMemberValidate,
}));

const mockedNotify = vi.fn();
vi.mock('@/services/notification-service', () => ({
  notify: mockedNotify,
}));

const CLAN_ID = 1;
const CLAN = {
  id: CLAN_ID,
  tag: 'mlp',
  primaryColor: '4278190080',
  secondaryColor: '4278190080',
  name: 'My Little Pony',
  bannerKey: '123456',
  region: 'Eu',
  discord: 'https://discord.gg',
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

const mockedLoadClanApplications = vi.fn();
const mockedUseClanApplications = vi.fn().mockImplementation(() => ({
  applicationsCount: computed(() => 2),
  loadClanApplications: mockedLoadClanApplications,
}));
vi.mock('@/composables/clan/use-clan-applications', () => ({
  useClanApplications: mockedUseClanApplications,
}));

const mockUsePagination = vi.fn().mockImplementation(() => ({
  perPage: 2,
  pageModel: ref(1),
}));
vi.mock('@/composables/use-pagination', () => ({
  usePagination: mockUsePagination,
}));

const mountOptions = {
  global: {
    renderStubDefaultSlot: true,
    stubs: {
      ClanTagIcon: true,
      UserMedia: true,
      ResultNotFound: true,
      ClanMemberDetail: true,
    },
    plugins: [createTestingPinia()],
  },
};

import { useUserStore } from '@/stores/user';
import Page from './index.vue';

const userStore = useUserStore();

const routes = [
  {
    name: 'clans-id',
    path: '/clans/:id',
    component: Page,
    props: true,
  },
];
const route = {
  name: 'clans-id',
  params: {
    id: CLAN_ID,
  },
};

beforeEach(() => {
  userStore.$reset();
  userStore.user = {
    id: NO_CLAN_USER_ID,
  } as User;
  userStore.clan = null;
});

describe('common', () => {
  it('should visible clan`s info fields', async () => {
    const { wrapper } = await mountWithRouter(mountOptions, routes, route);

    expect(wrapper.find('[data-aq-clan-info="name"]').text()).toEqual(CLAN.name);
    expect(wrapper.find('[data-aq-clan-info="tag"]').text()).toEqual(CLAN.tag);
    expect(wrapper.find('[data-aq-clan-info="region"]').text()).toEqual('region.Eu');
    expect(wrapper.find('[data-aq-clan-info="member-count"]').text()).toEqual('3');
    expect(wrapper.find('[data-aq-clan-info="description"]').exists()).toBeFalsy();
  });
});

describe('user not in a clan', () => {
  it('should visible "Apply to join" btn', async () => {
    const { wrapper } = await mountWithRouter(mountOptions, routes, route);

    expect(wrapper.find('[data-aq-clan-action="apply-to-join"]').exists()).toBeTruthy();
    expect(wrapper.find('[data-aq-clan-action="application-sent"]').exists()).toBeFalsy();
    expect(wrapper.find('[data-aq-clan-action="clan-application"]').exists()).toBeFalsy();
    expect(wrapper.find('[data-aq-clan-action="clan-update"]').exists()).toBeFalsy();

    await wrapper.find('[data-aq-clan-action="apply-to-join"]').trigger('click');

    expect(wrapper.find('[data-aq-clan-action="apply-to-join"]').exists()).toBeFalsy();
    expect(wrapper.find('[data-aq-clan-action="application-sent"]').exists()).toBeTruthy();
  });

  it('try to open member detail modal', async () => {
    const { wrapper } = await mountWithRouter(mountOptions, routes, route);

    const detailModal = wrapper.find('[data-aq-clan-member-detail-modal]');
    const rows = wrapper.findAll('tr');

    await rows.at(1)!.trigger('click');
    expect(detailModal.attributes('shown')).toEqual('false');

    await rows.at(2)!.trigger('click');
    expect(detailModal.attributes('shown')).toEqual('false');
  });
});

describe('user in another clan', () => {
  beforeEach(() => {});

  it('should`t visible "Apply to join" btn', async () => {
    userStore.clan = {
      id: 112,
    } as Clan;
    mockedGetClanMember.mockReturnValue(null);
    mockedCanManageApplicationsValidate.mockReturnValue(false);
    mockedCanUpdateClanValidate.mockReturnValue(false);
    mockedCanUpdateMemberValidate.mockReturnValue(false);
    mockedCanKickMemberValidate.mockReturnValue(false);

    const { wrapper } = await mountWithRouter(mountOptions, routes, route);

    expect(wrapper.find('[data-aq-clan-action="apply-to-join"]').exists()).toBeFalsy();
    expect(wrapper.find('[data-aq-clan-action="application-sent"]').exists()).toBeFalsy();

    expect(wrapper.find('[data-aq-clan-action="clan-application"]').exists()).toBeFalsy();
    expect(wrapper.find('[data-aq-clan-action="clan-update"]').exists()).toBeFalsy();
  });

  it('try to open member detail modal', async () => {
    const { wrapper } = await mountWithRouter(mountOptions, routes, route);

    const detailModal = wrapper.find('[data-aq-clan-member-detail-modal]');
    const rows = wrapper.findAll('tr');

    await rows.at(1)!.trigger('click');
    expect(detailModal.attributes('shown')).toEqual('false');

    await rows.at(2)!.trigger('click');
    expect(detailModal.attributes('shown')).toEqual('false');
  });
});

describe('user in the clan', () => {
  describe('Leader', () => {
    beforeEach(() => {
      mockedGetClanMember.mockReturnValue(CLAN_LEADER);
      mockedCanManageApplicationsValidate.mockReturnValue(true);
      mockedCanUpdateClanValidate.mockReturnValue(true);
      mockedCanUpdateMemberValidate.mockReturnValue(true);
      mockedCanKickMemberValidate.mockReturnValue(true);
    });

    it('should visible "Clan applications" & "Clan Update" btns', async () => {
      mockedCanKickMemberValidate.mockReturnValue(false);
      const { wrapper } = await mountWithRouter(mountOptions, routes, route);

      expect(wrapper.find('[data-aq-clan-action="clan-application"]').exists()).toBeTruthy();
      expect(wrapper.find('[data-aq-clan-action="clan-update"]').exists()).toBeTruthy();

      expect(wrapper.find('[data-aq-clan-action="apply-to-join"]').exists()).toBeFalsy();
      expect(wrapper.find('[data-aq-clan-action="leave-clan"]').exists()).toBeFalsy();
    });

    it('open member detail modal', async () => {
      const { wrapper } = await mountWithRouter(mountOptions, routes, route);

      const detailModal = wrapper.find('[data-aq-clan-member-detail-modal]');
      const rows = wrapper.findAll('tr');

      await rows.at(1)!.trigger('click');
      expect(detailModal.attributes('shown')).toEqual('false'); // cannot open a detailed window of self

      await rows.at(2)!.trigger('click');
      expect(detailModal.attributes('shown')).toEqual('true');

      const ClanMemberDetail = wrapper.findComponent({ name: 'ClanMemberDetail' });

      expect(ClanMemberDetail.props()).toEqual({
        canUpdate: true,
        canKick: true,
        member: CLAN_MEMBERS[1],
      });
    });

    it('kick some member', async () => {
      const { wrapper } = await mountWithRouter(mountOptions, routes, route);

      const rows = wrapper.findAll('tr');

      await rows.at(2)!.trigger('click');

      wrapper.findComponent({ name: 'ClanMemberDetail' }).vm.$emit('kick');
      await flushPromises();

      expect(mockedKickClanMember).toBeCalledWith(CLAN_ID, CLAN_MEMBERS[1].user.id);
      expect(mockedGetClanMembers).toHaveBeenNthCalledWith(2, CLAN_ID);
      expect(userStore.getUserClanAndRole).not.toHaveBeenCalled();
      expect(mockedNotify).toBeCalledWith('clan.member.kick.notify.success');
    });

    it('update some member', async () => {
      const NEW_ROLE = 'Member';

      const { wrapper } = await mountWithRouter(mountOptions, routes, route);
      const rows = wrapper.findAll('tr');

      await rows.at(2)!.trigger('click');

      wrapper.findComponent({ name: 'ClanMemberDetail' }).vm.$emit('update', NEW_ROLE);
      await flushPromises();

      expect(mockedUpdateClanMember).toBeCalledWith(CLAN_ID, CLAN_MEMBERS[1].user.id, NEW_ROLE);
      expect(mockedGetClanMembers).toHaveBeenNthCalledWith(2, CLAN_ID);
      expect(mockedNotify).toBeCalledWith('clan.member.update.notify.success');
    });
  });

  describe('Officer', () => {
    beforeEach(() => {
      mockedGetClanMember.mockReturnValue(CLAN_OFFICER);
      mockedCanManageApplicationsValidate.mockReturnValue(true);
      mockedCanUpdateClanValidate.mockReturnValue(false);
      mockedCanUpdateMemberValidate.mockReturnValue(true);
      mockedCanKickMemberValidate.mockReturnValue(true);
    });

    it('leave from clan', async () => {
      const { wrapper } = await mountWithRouter(mountOptions, routes, route);

      expect(wrapper.find('[data-aq-clan-action="leave-clan"]').exists()).toBeTruthy();

      await wrapper.find('[data-aq-clan-action="leave-clan-confirm"]').trigger('click');
      await flushPromises();

      expect(mockedKickClanMember).toBeCalledWith(CLAN_ID, CLAN_MEMBERS[1].user.id);
      expect(mockedGetClanMembers).toHaveBeenNthCalledWith(2, CLAN_ID);
      expect(userStore.getUserClanAndRole).toHaveBeenCalled();
      expect(mockedNotify).toBeCalledWith('clan.member.leave.notify.success');
    });
  });
});
