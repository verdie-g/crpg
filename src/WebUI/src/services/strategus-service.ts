import { get, tryGet, post, put } from './crpg-client';
import SettlementPublic from '@/models/settlement-public';
import BattleDetailed from '@/models/battle-detailed';
import { Result } from '@/models/result';
import StrategusUpdate from '@/models/strategus-update';
import Region from '@/models/region';
import BattlePhase from '@/models/battle-phase';
import Fighters from '@/models/fighters';
import Mercenaries from '@/models/mercenaries';
import Hero from '@/models/hero';
import BattleSide from '@/models/battle-side';
import HeroStatusUpdateRequest from '@/models/hero-status-update-request';
import HeroStatus from '@/models/hero-status';

export const regionToStr: Record<Region, string> = {
  [Region.Europe]: 'Europe',
  [Region.NorthAmerica]: 'NorthAmerica',
  [Region.Asia]: 'Asia',
};

export const inSettlementStatuses = new Set<HeroStatus>([
  HeroStatus.IdleInSettlement,
  HeroStatus.RecruitingInSettlement,
]);

export function getSettlements(): Promise<SettlementPublic> {
  return get('/strategus/settlements');
}

export async function getBattles(region: Region, phases: BattlePhase[]): Promise<BattleDetailed[]> {
  const params = new URLSearchParams();
  phases.forEach(t => params.append(`phase[]`, t));
  const battles: BattleDetailed[] = await get(`/strategus/battles?region=${region}&${params}`);
  return battles.map(b => ({ ...b, createdAt: new Date(b.createdAt) }));
}

export function getBattle(id: number): Promise<BattleDetailed> {
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

export function getFighters(battleId: number): Promise<Fighters[]> {
  return get(`/strategus/battles/${battleId}/fighters`);
}

export function getMercenaries(battleId: number): Promise<Mercenaries[]> {
  return get(`/strategus/battles/${battleId}/mercenaries`);
}

export function applyToBattleAsMercenary(
  battleId: number,
  characterId: number,
  side: BattleSide
): Promise<Mercenaries[]> {
  return post(`/strategus/battles/${battleId}/mercenaries`, { characterId, side });
}
