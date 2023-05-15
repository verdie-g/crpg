import { type MultiPoint, type Point } from 'geojson';
import { type Platform } from '@/models/platform';
import { type Region } from '@/models/region';
import { type SettlementPublic } from '@/models/strategus/settlement';

export enum PartyStatus {
  Idle = 'Idle',
  IdleInSettlement = 'IdleInSettlement',
  RecruitingInSettlement = 'RecruitingInSettlement',
  MovingToPoint = 'MovingToPoint',
  FollowingParty = 'FollowingParty',
  MovingToSettlement = 'MovingToSettlement',
  MovingToAttackParty = 'MovingToAttackParty',
  MovingToAttackSettlement = 'MovingToAttackSettlement',
  InBattle = 'InBattle',
}

export interface PartyCommon {
  id: number;
  name: string;
  region: Region;
  troops: number;
  position: Point;
  clan: null; // TODO:
}

export interface PartyVisible extends PartyCommon {
  platform: Platform;
  platformUserId: string;
}

export interface Party extends PartyCommon {
  gold: number;
  status: PartyStatus;
  waypoints: MultiPoint;
  targetedParty: PartyVisible | null;
  targetedSettlement: SettlementPublic | null;
}

export interface PartyStatusUpdateRequest {
  status: PartyStatus;
  waypoints: MultiPoint;
  targetedPartyId: number;
  targetedSettlementId: number;
}
