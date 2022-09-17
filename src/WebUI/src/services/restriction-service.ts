import Restriction from '@/models/restriction';
import { get } from '@/services/crpg-client';

export async function getRestrictions(): Promise<Restriction[]> {
  const restrictions: Restriction[] = await get('/restrictions');
  return restrictions.map(b => ({ ...b, createdAt: new Date(b.createdAt) }));
}
