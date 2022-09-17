import Restriction from '@/models/restriction';
import RestrictionType from '@/models/restriction-type'
import { get, post } from '@/services/crpg-client';
import RestrictionCreation from '@/models/restriction-creation';

export async function getRestrictions(): Promise<Restriction[]> {
  const restrictions: Restriction[] = await get('/restrictions');
  return restrictions.map(b => ({ ...b, createdAt: new Date(b.createdAt) }));
}

export function restrictUser(payload: RestrictionCreation): Promise<Restriction> {
  return post('/restrictions', payload);
}
