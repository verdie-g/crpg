import { type FetchSpyInstance, mockGet } from 'vi-fetch';
import { response } from '@/__mocks__/crpg-client';
import { ActivityLogType, type ActivityLog } from '@/models/activity-logs';

const { mockedGetUsersByIds } = vi.hoisted(() => ({
  mockedGetUsersByIds: vi.fn().mockResolvedValue([]),
}));
vi.mock('@/services/users-service', () => {
  return { getUsersByIds: mockedGetUsersByIds };
});

import { getActivityLogs, getActivityLogsWithUsers } from './activity-logs-service';

beforeEach(() => {
  vi.useFakeTimers();
  vi.setSystemTime('2023-03-30T18:00:00.0000000Z');
});

afterEach(() => {
  vi.useRealTimers();
});

describe('getActivityLogs', () => {
  let mock: FetchSpyInstance;

  beforeEach(() => {
    mock = mockGet(/\/activity-logs/).willDo(url => {
      if (url.searchParams.getAll('type[]').length !== 0) {
        return {
          body: response([
            {
              id: 1,
              type: ActivityLogType.UserCreated,
              userId: 123,
              metadata: {},
              createdAt: new Date(),
            },
          ]),
        };
      }

      if (url.searchParams.getAll('userId[]').length !== 0) {
        return {
          body: response([
            {
              id: 1,
              type: ActivityLogType.UserDeleted,
              userId: 123,
              metadata: {},
              createdAt: new Date(),
            },
          ]),
        };
      }

      return {
        body: response([
          {
            id: 1,
            type: ActivityLogType.UserRenamed,
            userId: 123,
            metadata: {},
            createdAt: new Date(),
          },
        ]),
      };
    });
  });

  const payload = {
    from: new Date('2023-03-22T18:16:42.052359Z'),
    to: new Date('2023-04-01T18:16:42.052359Z'),
    userId: [],
  };

  it('base', async () => {
    expect((await getActivityLogs(payload))[0]).toContain({ type: ActivityLogType.UserRenamed });

    expect(mock).toHaveFetched();
    expect(mock).toHaveFetchedWithQuery(
      'from=2023-03-22T18%3A16%3A42.052Z&to=2023-04-01T18%3A16%3A42.052Z'
    );
  });

  it('types', async () => {
    expect(
      (await getActivityLogs({ ...payload, type: [ActivityLogType.UserCreated] }))[0]
    ).toContain({ type: ActivityLogType.UserCreated });

    expect(mock).toHaveFetched();
    expect(mock).toHaveFetchedWithQuery(
      'from=2023-03-22T18%3A16%3A42.052Z&to=2023-04-01T18%3A16%3A42.052Z&type%5B%5D=UserCreated'
    );
  });

  it('userIds', async () => {
    expect((await getActivityLogs({ ...payload, userId: [123, 124] }))[0]).toContain({
      type: ActivityLogType.UserDeleted,
    });

    expect(mock).toHaveFetched();
    expect(mock).toHaveFetchedWithQuery(
      'from=2023-03-22T18%3A16%3A42.052Z&to=2023-04-01T18%3A16%3A42.052Z&userId%5B%5D=123&userId%5B%5D=124'
    );
  });
});

it('getActivityLogsWithUsers', async () => {
  mockedGetUsersByIds.mockResolvedValue([{ id: 2 }, { id: 3 }]);

  const logsResponse = [
    {
      type: ActivityLogType.TeamHit,
      metadata: {
        targetUserId: '2',
      },
    },
    {
      type: ActivityLogType.TeamHit,
      metadata: {
        targetUserId: '2',
      },
    },
    {
      type: ActivityLogType.TeamHit,
      metadata: {
        targetUserId: '3',
      },
    },
    {
      type: ActivityLogType.ChatMessageSent,
      metadata: {},
    },
  ] as Array<Partial<ActivityLog>>;
  mockGet(/\/activity-logs/).willResolve(response(logsResponse));

  const result = await getActivityLogsWithUsers({
    from: new Date(),
    to: new Date(),
    userId: [],
  });

  expect(mockedGetUsersByIds).toBeCalledWith([2, 3]);
  expect(result.users).toEqual({ '2': { id: 2 }, '3': { id: 3 } });
});
