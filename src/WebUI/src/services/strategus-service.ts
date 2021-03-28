import { get, tryGet, post } from './crpg-client';
import SettlementPublic from '@/models/settlement-public';
import { Result } from '@/models/result';
import StrategusUpdate from '@/models/strategus-update';
import Region from '@/models/region';
import Hero from '@/models/hero';

export const regionToStr: Record<Region, string> = {
  [Region.Europe]: 'Europe',
  [Region.NorthAmerica]: 'North America',
  [Region.Asia]: 'Asia',
};

export function getSettlements(): Promise<SettlementPublic> {
  return get('/strategus/settlements');
}

export function getUpdate(): Promise<Result<StrategusUpdate>> {
  return tryGet('/strategus/update');
}

export function registerUser(region: Region): Promise<Hero> {
  return post('/strategus/heroes', { region });
}
