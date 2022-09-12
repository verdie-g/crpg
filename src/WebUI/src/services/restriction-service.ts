import Restriction from '@/models/restriction';
import RestrictionType from '@/models/restriction-type'
import { get, post } from '@/services/crpg-client';

export async function getRestrictions(): Promise<Restriction[]> {
  const restrictions: Restriction[] = await get('/restrictions');
  return restrictions.map(b => ({ ...b, createdAt: new Date(b.createdAt) }));
}

export function createRestriction(
  restrictedUserId: number,
  type: RestrictionType,
  reason: string,
  duration?: number
): Promise<Restriction> {
  return post('/restrictions', {
    restrictedUserId,
    type,
    reason,
    duration,
  });
}
