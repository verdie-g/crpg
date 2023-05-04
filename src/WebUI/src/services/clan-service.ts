import {
  type Clan,
  type ClanWithMemberCount,
  type ClanEdition,
  type ClanMember,
  ClanMemberRole,
  type ClanInvitation,
  ClanInvitationType,
  ClanInvitationStatus,
} from '@/models/clan';
import { Region } from '@/models/region';
import { get, post, put, del } from '@/services/crpg-client';
import { rgbHexColorToArgbInt, argbIntToRgbHexColor } from '@/utils/color';

const mapClanRequest = (payload: Omit<Clan, 'id'>): Omit<ClanEdition, 'id'> => {
  return {
    ...payload,
    primaryColor: rgbHexColorToArgbInt(payload.primaryColor),
    secondaryColor: rgbHexColorToArgbInt(payload.secondaryColor),
  };
};

export const mapClanResponse = (payload: ClanEdition): Clan => {
  return {
    ...payload,
    primaryColor: argbIntToRgbHexColor(payload.primaryColor),
    secondaryColor: argbIntToRgbHexColor(payload.secondaryColor),
  };
};

// TODO: backend pagination/region query!
export const getClans = async () => {
  const clans = await get<ClanWithMemberCount<ClanEdition>[]>('/clans');
  return clans.map(c => ({
    ...c,
    clan: mapClanResponse(c.clan),
  }));
};

export const getFilteredClans = (
  clans: ClanWithMemberCount<Clan>[],
  region: Region,
  search: string
) => {
  const searchQuery = search.toLowerCase();
  return clans.filter(
    c =>
      c.clan.region === region &&
      (c.clan.tag.toLowerCase().includes(searchQuery) ||
        c.clan.name.toLowerCase().includes(searchQuery))
  );
};

export const createClan = async (clan: Omit<Clan, 'id'>) =>
  mapClanResponse(await post<ClanEdition>('/clans', mapClanRequest(clan)));

export const updateClan = async (clanId: number, clan: Clan) =>
  mapClanResponse(await put<ClanEdition>(`/clans/${clanId}`, mapClanRequest(clan)));

export const getClan = async (id: number) =>
  mapClanResponse(await get<ClanEdition>(`/clans/${id}`));

export const getClanMembers = async (id: number) => get<ClanMember[]>(`/clans/${id}/members`);

export const updateClanMember = async (clanId: number, memberId: number, role: ClanMemberRole) =>
  put<ClanMember>(`/clans/${clanId}/members/${memberId}`, { role });

export const kickClanMember = async (clanId: number, memberId: number) =>
  del(`/clans/${clanId}/members/${memberId}`);

export const inviteToClan = async (clanId: number, inviteeId: number) =>
  post<ClanInvitation>(`/clans/${clanId}/invitations`, { inviteeId });

export const getClanInvitations = async (
  clanId: number,
  types: ClanInvitationType[],
  statuses: ClanInvitationStatus[]
) => {
  const params = new URLSearchParams();
  types.forEach(t => params.append('type[]', t));
  statuses.forEach(s => params.append('status[]', s));
  return get<ClanInvitation[]>(`/clans/${clanId}/invitations?${params}`);
};

export const respondToClanInvitation = async (
  clanId: number,
  clanInvitationId: number,
  accept: boolean
) => put<ClanInvitation>(`/clans/${clanId}/invitations/${clanInvitationId}/response`, { accept });

// TODO: need a name
export const getClanMember = (clanMembers: ClanMember[], userId: number) =>
  clanMembers.find(m => m.user.id === userId) || null;

export const canManageApplicationsValidate = (role: ClanMemberRole) =>
  [ClanMemberRole.Leader, ClanMemberRole.Officer].includes(role);

export const canUpdateClanValidate = (role: ClanMemberRole) =>
  [ClanMemberRole.Leader].includes(role);

// TODO: Spec
export const canUpdateMemberValidate = (role: ClanMemberRole) =>
  [ClanMemberRole.Leader].includes(role);

export const canKickMemberValidate = (
  selfMember: ClanMember,
  member: ClanMember,
  clanMembersCount: number
) => {
  if (
    member.user.id === selfMember.user.id &&
    (member.role !== ClanMemberRole.Leader || clanMembersCount === 1)
  ) {
    return true;
  }

  return (
    (selfMember.role === ClanMemberRole.Leader &&
      [ClanMemberRole.Officer, ClanMemberRole.Member].includes(member.role)) ||
    (selfMember.role === ClanMemberRole.Officer && member.role === ClanMemberRole.Member)
  );
};
