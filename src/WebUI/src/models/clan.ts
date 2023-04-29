import { Region } from '@/models/region';
import { type UserPublic } from '@/models/user';

export interface UserClan {
  clan: ClanEdition;
  role: ClanMemberRole;
}

export interface Clan {
  id: number;
  tag: string;
  primaryColor: string;
  secondaryColor: string;
  name: string;
  bannerKey: string;
  region: Region;
  discord: string | null;
  description: string;
}

// TODO: rename
export interface ClanEdition extends Omit<Clan, 'primaryColor' | 'secondaryColor'> {
  primaryColor: number;
  secondaryColor: number;
}

export interface ClanWithMemberCount<T> {
  clan: T;
  memberCount: number;
}

export interface ClanMember {
  user: UserPublic;
  role: ClanMemberRole;
}

export enum ClanMemberRole {
  Member = 'Member',
  Officer = 'Officer',
  Leader = 'Leader',
}

export enum ClanInvitationType {
  Request = 'Request',
  Offer = 'Offer',
}

export enum ClanInvitationStatus {
  Pending = 'Pending',
  Declined = 'Declined',
  Accepted = 'Accepted',
}

export interface ClanInvitation {
  id: number;
  invitee: UserPublic;
  inviter: UserPublic;
  type: ClanInvitationType;
  status: ClanInvitationStatus;
}
