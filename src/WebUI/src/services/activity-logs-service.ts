import qs from 'qs';
import { UserPublic } from '@/models/user';
import { ActivityLogType, type ActivityLog } from '@/models/activity-logs';
import { get } from '@/services/crpg-client';
import { getUsersByIds } from '@/services/users-service';

export interface ActivityLogsPayload {
  from: Date;
  to: Date;
  userId: number[];
  type?: ActivityLogType[];
}

export const getActivityLogs = async (payload: ActivityLogsPayload) => {
  const params = qs.stringify(payload, {
    strictNullHandling: true,
    arrayFormat: 'brackets',
    skipNulls: true,
  });

  return (await get<ActivityLog[]>(`/activity-logs?${params}`)).map(al => ({
    ...al,
    createdAt: new Date(al.createdAt),
  }));
};

const extractUsersFromLogs = (logs: ActivityLog[]) =>
  logs.reduce((out, l) => {
    if ('targetUserId' in l.metadata) {
      out.push(Number(l.metadata.targetUserId));
    }

    return [...new Set(out)];
  }, [] as number[]);

export const getActivityLogsWithUsers = async (payload: ActivityLogsPayload) => {
  const logs = await getActivityLogs(payload);
  const users = (
    await getUsersByIds([...new Set([...payload.userId, ...extractUsersFromLogs(logs)])])
  ).reduce((out, user) => {
    out[user.id] = user;
    return out;
  }, {} as Record<number, UserPublic>);

  return {
    logs,
    users,
  };
};
