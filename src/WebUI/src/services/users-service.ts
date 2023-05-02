import qs from 'qs';
import { type Item } from '@/models/item';
import {
  type User,
  type UserPublic,
  type UserItem,
  type UserItemsByType,
  type UserItemRank,
} from '@/models/user';
import Platform from '@/models/platform';
import { type ClanEdition, type ClanMemberRole } from '@/models/clan';
import { type RestrictionWithActive } from '@/models/restriction';

import { get, post, put, del } from '@/services/crpg-client';
import { getActiveJoinRestriction, mapRestrictions } from '@/services/restriction-service';
import { mapClanResponse } from '@/services/clan-service';

export const getUser = () => get<User>('/users/self');

export const deleteUser = () => del('/users/self');

export const getUserById = (id: number) => get<UserPublic>(`/users/${id}`);

interface UserSearchQuery {
  platform?: Platform;
  platformUserId?: string;
  name?: string;
}

// TODO: SPEC
export const searchUser = async (payload: UserSearchQuery) => {
  return get<UserPublic[]>(`/users/search/?${qs.stringify(payload)}`);
};

export const mapUserItem = (userItem: UserItem): UserItem => ({
  ...userItem,
  createdAt: new Date(userItem.createdAt),
});

export const extractItemFromUserItem = (items: UserItem[]): Item[] => items.map(ui => ui.baseItem);

export const getUserItems = async () =>
  (await get<UserItem[]>('/users/self/items')).map(mapUserItem);

export const buyUserItem = async (itemId: string) =>
  mapUserItem(await post<UserItem>('/users/self/items', { itemId }));

export const upgradeUserItem = async (userItemId: number) =>
  mapUserItem(await put<UserItem>(`/users/self/items/${userItemId}/upgrade`));

export const sellUserItem = (userItemId: number) => del(`/users/self/items/${userItemId}`);

export const getItemRanks = (): UserItemRank[] => [0, 1, 2, 3];

export const groupUserItemsByType = (items: UserItem[]) =>
  items
    .reduce((itemsGroup, ui) => {
      const type = ui.baseItem.type;
      const currentGroup = itemsGroup.find(item => item.type === type);

      if (currentGroup) {
        currentGroup.items.push(ui);
      } else {
        itemsGroup.push({
          type,
          items: [ui],
        });
      }

      return itemsGroup;
    }, [] as UserItemsByType[])
    .sort((a, b) => a.type.localeCompare(b.type));

interface UserClan {
  clan: ClanEdition;
  role: ClanMemberRole;
}

export const getUserClan = async () => {
  const userClan = await get<UserClan | null>('/users/self/clans');
  if (userClan === null || userClan.clan === null) return null;

  // do conversion since argb values are stored as numbers in db and we need strings
  return { clan: mapClanResponse(userClan.clan), role: userClan.role };
};

export const getUserRestrictions = async (id: number) =>
  mapRestrictions(await get<RestrictionWithActive[]>(`/users/${id}/restrictions`));

// TODO: SPEC
export const getUserActiveJoinRestriction = async (id: number) =>
  getActiveJoinRestriction(await getUserRestrictions(id));
