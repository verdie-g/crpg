import { UserPublic } from '@/models/user';

export enum RestrictionType {
  Join = 'Join',
  Chat = 'Chat',
  ALL = 'All',
}

export interface Restriction {
  id: number;
  restrictedUser: UserPublic;
  duration: number;
  type: RestrictionType;
  reason: string;
  restrictedByUser: UserPublic;
  createdAt: Date;
}

export interface RestrictionWithActive extends Restriction {
  active: boolean;
}

export interface RestrictionCreation {
  restrictedUserId: number;
  type: RestrictionType;
  reason: string;
  duration: number;
}
