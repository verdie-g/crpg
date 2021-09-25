import { get, post, put, del } from './crpg-client';
import Clan from '@/models/clan';
import ClanWithMemberCount from '@/models/clan-with-member-count';
import ClanCreation from '@/models/clan-creation';
import ClanInvitation from '@/models/clan-invitation';
import ClanInvitationStatus from '@/models/clan-invitation-status';
import ClanInvitationType from '@/models/clan-invitation-type';
import ClanMember from '@/models/clan-member';
import ClanMemberRole from '@/models/clan-member-role';

export function getClan(id: number): Promise<Clan> {
  return get(`/clans/${id}`);
}

export function getClanMembers(id: number): Promise<ClanMember[]> {
  return get(`/clans/${id}/members`);
}

export function updateClanMember(
  clanId: number,
  userId: number,
  role: ClanMemberRole
): Promise<ClanMember> {
  return put(`/clans/${clanId}/members/${userId}`, { role });
}

export function getClanInvitations(
  clanId: number,
  types: ClanInvitationType[],
  statuses: ClanInvitationStatus[]
): Promise<ClanInvitation[]> {
  const params = new URLSearchParams();
  types.forEach(t => params.append('type[]', t));
  statuses.forEach(s => params.append('status[]', s));

  return get(`/clans/${clanId}/invitations?${params}`);
}

export function inviteToClan(clanId: number, inviteeId: number): Promise<ClanInvitation> {
  return post(`/clans/${clanId}/invitations`, { inviteeId });
}

export function respondToClanInvitation(
  clanId: number,
  clanInvitationId: number,
  accept: boolean
): Promise<ClanInvitation> {
  return put(`/clans/${clanId}/invitations/${clanInvitationId}/responses`, { accept });
}

export function getClans(): Promise<ClanWithMemberCount[]> {
  return get('/clans');
}

export function createClan(clan: ClanCreation): Promise<Clan> {
  return post('/clans', clan);
}

export function kickClanMember(clanId: number, userId: number): Promise<void> {
  return del(`/clans/${clanId}/members/${userId}`);
}
