import { get, post, put, del } from './crpg-client';
import Clan from '@/models/clan';
import ClanWithMemberCount from '@/models/clan-with-member-count';
import ClanInvitation from '@/models/clan-invitation';
import ClanInvitationStatus from '@/models/clan-invitation-status';
import ClanInvitationType from '@/models/clan-invitation-type';
import ClanMember from '@/models/clan-member';
import ClanMemberRole from '@/models/clan-member-role';
import ClanDTO from '@/models/clan-dto';

import { rgbHexColorToArgbInt, argbIntToRgbHexColor } from '@/utils/color';

const mapClanRequest = (payload: Omit<Clan, 'id'>): Omit<ClanDTO, 'id'> => {
  return {
    ...payload,
    primaryColor: rgbHexColorToArgbInt(payload.primaryColor),
    secondaryColor: rgbHexColorToArgbInt(payload.secondaryColor),
  };
};

const mapClanResponse = (payload: ClanDTO): Clan => {
  return {
    ...payload,
    primaryColor: argbIntToRgbHexColor(payload.primaryColor),
    secondaryColor: argbIntToRgbHexColor(payload.secondaryColor),
  };
};

export async function getClan(id: number): Promise<Clan> {
  const response = (await get(`/clans/${id}`)) as ClanDTO;

  return mapClanResponse(response);
}

export function getClanMembers(id: number): Promise<ClanMember[]> {
  return get(`/clans/${id}/members`);
}

export function updateClanMember(
  clanId: number,
  memberId: number,
  role: ClanMemberRole
): Promise<ClanMember> {
  return put(`/clans/${clanId}/members/${memberId}`, { role });
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
  return put(`/clans/${clanId}/invitations/${clanInvitationId}/response`, { accept });
}

export function getClans(): Promise<ClanWithMemberCount[]> {
  return get('/clans');
}

export async function createClan(clan: Omit<Clan, 'id'>): Promise<Clan> {
  const response = (await post('/clans', mapClanRequest(clan))) as ClanDTO;

  return mapClanResponse(response);
}

export async function updateClan(clanId: number, clan: Clan): Promise<Clan> {
  const response = (await put(`/clans/${clanId}`, mapClanRequest(clan))) as ClanDTO;

  return mapClanResponse(response);
}

export function kickClanMember(clanId: number, memberId: number): Promise<void> {
  return del(`/clans/${clanId}/members/${memberId}`);
}
