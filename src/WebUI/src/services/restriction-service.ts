import Restriction from '@/models/restriction';
import { get, post } from '@/services/crpg-client';
import RestrictionCreation from '@/models/restriction-creation';

export async function getRestrictions(): Promise<Restriction[]> {
  return get('/restrictions');
}

export function restrictUser(payload: RestrictionCreation): Promise<Restriction> {
  return post('/restrictions', payload);
}
