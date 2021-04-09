import { get, post, del } from './crpg-client';
import Clan from '@/models/clan';
import ClanWithMembers from '@/models/clan-with-members';
import ClanCreation from '@/models/clan-creation';

export function getClan(id: number): Promise<ClanWithMembers> {
  return get(`/clans/${id}`);
}

export function getClans(): Promise<Clan[]> {
  return get('/clans');
}

export function createClan(clan: ClanCreation): Promise<ClanWithMembers> {
  return post('/clans', clan);
}

export function kickClanMember(clanId: number, userId: number): Promise<void> {
  return del(`/clans/${clanId}/members/${userId}`);
}
