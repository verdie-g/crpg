import { get, tryGet, post, put } from './crpg-client';
import SettlementPublic from '@/models/settlement-public';
import BattlePublic from '@/models/battle-detailed';
import { Result } from '@/models/result';
import StrategusUpdate from '@/models/strategus-update';
import Region from '@/models/region';
import Phase from '@/models/phase';
import Fighters from '@/models/fighters';
import Mercenaries from '@/models/mercenaries';
import Hero from '@/models/hero';
import Side from '@/models/side';
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

export function getBattle(id: String): Promise<BattlePublic> {
  return get(`/strategus/battles/${id}`);
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

export function getFighters(battleId: String): Promise<Fighters[]> {
  return get(`/strategus/battles/${battleId}/fighters`);
}

export function getMercenaries(battleId: String): Promise<Mercenaries[]> {
  return get(`/strategus/battles/${battleId}/mercenaries`);
}

export function applyToBattleAsMercenary(battleId: number, characterId: number, side: Side) {
  return post(`/strategus​/battles​/${battleId}​/mercenaries`, {characterId, side});
}
