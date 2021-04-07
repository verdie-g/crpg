import { get } from './crpg-client';
import Clan from '@/models/clan';
import ClanWithMembers from '@/models/clan-with-members';

export function getClan(id: number): Promise<ClanWithMembers> {
  return get(`/clans/${id}`);
}

export function getClans(): Promise<Clan[]> {
  return get('/clans');
}
