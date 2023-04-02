import { type MountingOptions, flushPromises } from '@vue/test-utils';
import { mountWithRouter } from '@/__test__/unit/utils';
import { moderationUserKey } from '@/symbols/moderator';

const { mockedGetActivityLogsWithUsers } = vi.hoisted(() => ({
  mockedGetActivityLogsWithUsers: vi.fn().mockResolvedValue({ logs: [], users: {} }),
}));
vi.mock('@/services/activity-logs-service', () => ({
  getActivityLogsWithUsers: mockedGetActivityLogsWithUsers,
}));

import ActivityLogs from './activity-logs.vue';

const routes = [
  {
    name: 'ModeratorUserIdActivityLogs',
    path: '/moderator/user/:id/activity-logs',
    component: ActivityLogs,
  },
];

const route = {
  name: 'ModeratorUserIdActivityLogs',
  params: { id: 1 },
};

const CURRENT_USER_ID = 1;
const ADDITIONAL_USER_ID_1 = 2; // ex: ActivityLog.TeamHit - userTargetId
const ADDITIONAL_USER_ID_2 = 4; // ex: ActivityLog.TeamHit - userTargetId
const FIND_USER_ID = 3;

const mountOptions = {
  props: { id: String(CURRENT_USER_ID) },
  global: {
    provide: {
      // @ts-ignore
      [moderationUserKey]: {
        id: CURRENT_USER_ID,
      },
    },
    renderStubDefaultSlot: true,
    stubs: {
      UserFinder: {
        template: `<div data-aq-UserFinder-stub><slot name="user-prepend" v-bind="{ id: ${FIND_USER_ID} }"/></div>`,
      },
      ActivityLogItem: true,
      UserMedia: true,
    },
  },
} as MountingOptions<{}>;

beforeEach(() => {
  vi.useFakeTimers();
  vi.setSystemTime('2023-03-30T18:00:00.0000000Z');
});

afterEach(() => {
  vi.useRealTimers();
});

const mockData = {
  logs: [
    {
      createdAt: new Date('2023-03-30T17:56:00.0000000Z'),
      id: 1,
      metadata: {
        gold: '120000',
        heirloomPoints: '3',
      },
      type: 'UserRewarded',
      userId: CURRENT_USER_ID,
    },
    {
      createdAt: new Date('2023-03-30T17:57:00.0000000Z'),
      id: 2,
      metadata: {
        itemId: '123',
      },
      type: 'ItemBroke',
      userId: CURRENT_USER_ID,
    },
  ],
  users: {
    [ADDITIONAL_USER_ID_1]: {
      id: ADDITIONAL_USER_ID_1,
    },
    [ADDITIONAL_USER_ID_2]: {
      id: ADDITIONAL_USER_ID_2,
    },
  },
};

it('basic', async () => {
  mockedGetActivityLogsWithUsers.mockResolvedValue(mockData);
  const { wrapper } = await mountWithRouter(mountOptions, routes, route);

  expect(wrapper).toBeDefined();
  expect(mockedGetActivityLogsWithUsers).toBeCalledWith({
    from: new Date('2023-03-30T17:55:00.0000000Z'),
    to: new Date(),
    type: [],
    userId: [CURRENT_USER_ID],
  });
});

it('add/remove additional user', async () => {
  mockedGetActivityLogsWithUsers.mockResolvedValue(mockData);
  const { wrapper, router } = await mountWithRouter(mountOptions, routes, route);
  const spyRouterPush = vi.spyOn(router, 'push');

  const findUserComponent = wrapper.find('[data-aq-activityLogs-userFinder-addUser-btn]');

  expect(wrapper.findAll('[data-aq-activityLogs-additionalUser]').length).toEqual(0);

  await findUserComponent.trigger('click');
  await flushPromises();

  expect(spyRouterPush).toBeCalledWith({
    query: {
      additionalUsers: [FIND_USER_ID],
    },
  });

  expect(mockedGetActivityLogsWithUsers).toHaveBeenNthCalledWith(2, {
    from: new Date('2023-03-30T17:55:00.0000000Z'),
    to: new Date(),
    type: [],
    userId: [CURRENT_USER_ID, FIND_USER_ID],
  });

  const additionalUsers = wrapper.findAll('[data-aq-activityLogs-additionalUser]');
  expect(additionalUsers.length).toEqual(1);

  const removeUserBtn = additionalUsers.at(0)!.find('[data-aq-activityLogs-additionalUser-remove]');
  await removeUserBtn.trigger('click');
  await flushPromises();

  expect(mockedGetActivityLogsWithUsers).toHaveBeenNthCalledWith(3, {
    from: new Date('2023-03-30T17:55:00.0000000Z'),
    to: new Date(),
    type: [],
    userId: [CURRENT_USER_ID],
  });

  expect(wrapper.findAll('[data-aq-activityLogs-additionalUser]').length).toEqual(0);
});

it('sort', async () => {
  mockedGetActivityLogsWithUsers.mockResolvedValue(mockData);
  const { wrapper, router } = await mountWithRouter(mountOptions, routes, route);
  const spyRouterPush = vi.spyOn(router, 'push');

  console.log();

  expect(
    wrapper.findAllComponents({ name: 'ActivityLogItem' }).at(0)!.props('activityLog').id
  ).toEqual(1);

  await wrapper.find('[data-aq-activityLogs-sort-btn]').trigger('click');
  await flushPromises();

  expect(spyRouterPush).toHaveBeenCalledWith({
    query: {
      sort: 'createdAt_desc',
    },
  });

  expect(
    wrapper.findAllComponents({ name: 'ActivityLogItem' }).at(0)!.props('activityLog').id
  ).toEqual(2);
});
