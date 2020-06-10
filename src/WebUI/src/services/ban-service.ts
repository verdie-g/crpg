import Ban from '@/models/ban';
import { get } from '@/services/crpg-client';

export async function getBans(): Promise<Ban[]> {
  const bans: Ban[] = await get('/bans');
  return bans.map(b => ({ ...b, createdAt: new Date(b.createdAt) }));
}
