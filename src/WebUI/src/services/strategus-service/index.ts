import { type SettlementPublic } from '@/models/strategus/settlement';
import { type StrategusUpdate } from '@/models/strategus';
import { type Party, type PartyStatusUpdateRequest, PartyStatus } from '@/models/strategus/party';

import { Region } from '@/models/region';

import { get, tryGet, post, put } from '@/services/crpg-client';

export const getSettlements = () => get<SettlementPublic[]>('/settlements');

export const getUpdate = () => tryGet<StrategusUpdate>('/parties/self/update');

export const updatePartyStatus = (update: PartyStatusUpdateRequest) =>
  put<Party>('/parties/self/status', update);

export const registerUser = (region: Region) => post<Party>('/parties', { region });

export const inSettlementStatuses = new Set<PartyStatus>([
  PartyStatus.IdleInSettlement,
  PartyStatus.RecruitingInSettlement,
]);
