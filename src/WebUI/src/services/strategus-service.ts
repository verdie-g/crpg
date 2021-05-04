import { get, tryGet, post, put } from './crpg-client';
import SettlementPublic from '@/models/settlement-public';
import BattlePublic from '@/models/battle-detailed';
import { Result } from '@/models/result';
import StrategusUpdate from '@/models/strategus-update';
import Region from '@/models/region';
import Phase from '@/models/phase';
import Hero from '@/models/hero';
import BattleApplyMercenaries from '@/models/battle-apply-mercenaries';
import HeroStatusUpdateRequest from '@/models/hero-status-update-request';
import HeroStatus from '@/models/hero-status';
import { parameterizeArray } from '@/utils/serialize';

export const regionToStr: Record<Region, string> = {
  [Region.Europe]: 'Europe',
  [Region.NorthAmerica]: 'North America',
  [Region.Asia]: 'Asia',
};

export const inSettlementStatuses = new Set<HeroStatus>([
  HeroStatus.IdleInSettlement,
  HeroStatus.RecruitingInSettlement,
]);

export function getSettlements(): Promise<SettlementPublic> {
  return get('/strategus/settlements');
}

export async function getBattles(region: Region, phases: Phase[]): Promise<BattlePublic[]> {
  const battles: BattlePublic[] = await get(`/strategus/battles?region=${region}&${parameterizeArray('phase',phases)}`);
  return battles.map(b => ({ ...b, createdAt: new Date(b.createdAt) }));
}

export function getUpdate(): Promise<Result<StrategusUpdate>> {
  return tryGet('/strategus/update');
}

export function updateHeroStatus(update: HeroStatusUpdateRequest): Promise<Hero> {
  return put('/strategus/heroes/self/status', update);
}

export function registerUser(region: Region): Promise<Hero> {
  return post('/strategus/heroes', { region });
}
