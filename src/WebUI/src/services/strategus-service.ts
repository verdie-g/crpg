import { get, tryGet, post, put } from './crpg-client';
import SettlementPublic from '@/models/settlement-public';
import { Result } from '@/models/result';
import StrategusUpdate from '@/models/strategus-update';
import Region from '@/models/region';
import Party from '@/models/party';
import PartyStatusUpdateRequest from '@/models/party-status-update-request';
import PartyStatus from '@/models/party-status';

export const regionToStr: Record<Region, string> = {
  [Region.Europe]: 'Europe',
  [Region.NorthAmerica]: 'North America',
  [Region.Asia]: 'Asia',
};

export const inSettlementStatuses = new Set<PartyStatus>([
  PartyStatus.IdleInSettlement,
  PartyStatus.RecruitingInSettlement,
]);

export function getSettlements(): Promise<SettlementPublic> {
  return get('/settlements');
}

export function getUpdate(): Promise<Result<StrategusUpdate>> {
  return tryGet('/parties/self/update');
}

export function updatePartyStatus(update: PartyStatusUpdateRequest): Promise<Party> {
  return put('/parties/self/status', update);
}

export function registerUser(region: Region): Promise<Party> {
  return post('/parties', { region });
}
