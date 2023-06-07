import { Platform } from './platform';
import Role from './role';
import { Region } from './region';
import { ItemSlot, ItemType, type Item } from './item';

export interface User {
  id: number;
  platform: Platform;
  platformUserId: string;
  name: string;
  gold: number;
  heirloomPoints: number;
  role: Role;
  avatar: string;
  activeCharacterId: number | null;
  region: Region | null;
  experienceMultiplier: number;
  isDonor: boolean;
}

export interface UserPublic extends Pick<User, 'id' | 'platform' | 'platformUserId' | 'name'> {
  avatar: string;
}

export type UserItemRank = -1 | 0 | 1 | 2 | 3;

export interface UserItem {
  id: number;
  item: Item;
  rank: UserItemRank;
  isBroken: boolean;
  createdAt: Date;
}

export interface UserItemsByType {
  type: ItemType;
  items: UserItem[];
}

export type UserItemsBySlot = Record<ItemSlot, UserItem>;
