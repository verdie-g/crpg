import { PartialDeep } from 'type-fest';
import { mockGet, mockPost, mockPut, mockDelete } from 'vi-fetch';
import { response } from '@/__mocks__/crpg-client';
import { Region } from '@/models/region';
import {
  type ClanInvitation,
  ClanInvitationStatus,
  ClanInvitationType,
  type ClanMember,
  ClanMemberRole,
  type ClanWithMemberCount,
  type Clan,
  type ClanEdition,
} from '@/models/clan';

vi.mock('@/services/auth-service', () => ({
  getToken: vi.fn().mockResolvedValue('mockedToken'),
}));

const { ARGB_INT, HEX_COLOR } = vi.hoisted(() => ({
  ARGB_INT: 'ArgbInt',
  HEX_COLOR: 'HexColor',
}));
const { mockedRgbHexColorToArgbInt, mockedArgbIntToRgbHexColor } = vi.hoisted(() => ({
  mockedRgbHexColorToArgbInt: vi.fn().mockReturnValue(ARGB_INT),
  mockedArgbIntToRgbHexColor: vi.fn().mockReturnValue(HEX_COLOR),
}));
vi.mock('@/utils/color', () => ({
  rgbHexColorToArgbInt: mockedRgbHexColorToArgbInt,
  argbIntToRgbHexColor: mockedArgbIntToRgbHexColor,
}));

import {
  mapClanResponse,
  getClans,
  getFilteredClans,
  createClan,
  updateClan,
  getClan,
  getClanMembers,
  updateClanMember,
  kickClanMember,
  inviteToClan,
  getClanInvitations,
  respondToClanInvitation,
  canManageApplicationsValidate,
  canUpdateClanValidate,
  canUpdateMemberValidate,
  canKickMemberValidate,
  getClanMember,
} from './clan-service';

it('mapClanResponse', () => {
  const clan = {
    tag: 'mlp',
    primaryColor: 4278190080,
    secondaryColor: 4278190080,
  } as ClanEdition;

  expect(mapClanResponse(clan)).toEqual({
    tag: 'mlp',
    primaryColor: HEX_COLOR,
    secondaryColor: HEX_COLOR,
  });

  expect(mockedArgbIntToRgbHexColor).toBeCalledTimes(2);
});

it('getClans', async () => {
  const mockGetClans = [
    {
      clan: {
        tag: 'mlp',
        primaryColor: 4278190080,
        secondaryColor: 4278190080,
        name: 'My Little Pony',
      },
      memberCount: 4,
    },
  ];

  mockGet('/clans').willResolve(response(mockGetClans));
  await getClans();
  expect(mockedArgbIntToRgbHexColor).toBeCalledTimes(2);
});

it.each<[Region, string, number[]]>([
  [Region.Eu, '', [1, 3]],
  [Region.Na, '', [2]],
  [Region.Na, 'UNI', [2]],
  [Region.Na, 'FOAL', []],
  [Region.Eu, 'Unicorns', []],
  [Region.Eu, 'Po', [3]],
])('getFilteredClans - region: %s, searchQuery: %s', (region, searchQuery, expectation) => {
  const clans = [
    { clan: { id: 1, region: Region.Eu, tag: 'FOAL', name: 'Foals' } },
    { clan: { id: 2, region: Region.Na, tag: 'UNIC', name: 'Unicorns' } },
    { clan: { id: 3, region: Region.Eu, tag: 'PONY', name: 'Ponies' } },
  ] as ClanWithMemberCount<Clan>[];

  expect(getFilteredClans(clans, region, searchQuery).map(c => c.clan.id)).toEqual(expectation);
});

it('createClan', async () => {
  const newClan = {
    region: Region.Eu,
    name: 'My Little Pony Clan',
    tag: 'MLP',
    primaryColor: '#fff',
    secondaryColor: '#fff',
    bannerKey: '22222222',
    discord: 'gg.mlp.gg',
  } as Omit<Clan, 'id'>;

  const mock = mockPost('/clans').willResolve(response(newClan));

  expect(await createClan(newClan)).toEqual({
    ...newClan,
    primaryColor: HEX_COLOR,
    secondaryColor: HEX_COLOR,
  });

  expect(mock).toHaveFetchedWithBody({
    ...newClan,
    primaryColor: ARGB_INT,
    secondaryColor: ARGB_INT,
  });
});

it('updateClan', async () => {
  const clanId = 1;
  const clan = {
    id: 1,
    region: Region.Eu,
    name: 'My Little Pony Clan',
    tag: 'MLP',
    primaryColor: '4278190080',
    secondaryColor: '4278190080',
    bannerKey: '22222222',
    discord: 'gg.mlp.gg',
  } as Clan;

  const mock = mockPut(`/clans/${clanId}`).willResolve(response(clan));

  expect(await updateClan(clanId, clan)).toEqual({
    ...clan,
    primaryColor: HEX_COLOR,
    secondaryColor: HEX_COLOR,
  });

  expect(mock).toHaveFetchedWithBody({
    ...clan,
    primaryColor: ARGB_INT,
    secondaryColor: ARGB_INT,
  });
});

it('getClan', async () => {
  const clanId = 1;
  const clan = {
    id: 1,
    region: Region.Eu,
    name: 'My Little Pony Clan',
    tag: 'MLP',
    primaryColor: 4278190080,
    secondaryColor: 4278190080,
    bannerKey: '22222222',
    discord: 'gg.mlp.gg',
  };

  mockGet(`/clans/${clanId}`).willResolve(response(clan));

  expect(await getClan(clanId)).toEqual({
    ...clan,
    primaryColor: HEX_COLOR,
    secondaryColor: HEX_COLOR,
  });
});

it('getClanMembers', async () => {
  const clanId = 1;
  const members = [
    {
      user: {
        id: 1,
        name: 'Rarity',
      },
      role: 'Leader',
    },
  ];

  mockGet(`/clans/${clanId}/members`).willResolve(response(members));

  expect(await getClanMembers(clanId)).toEqual(members);
});

it('updateClanMember', async () => {
  const clanId = 1;
  const memberId = 4;
  const newRole = ClanMemberRole.Officer;
  const updatedMember = {
    user: {
      id: memberId,
      name: 'Rarity',
    },
    role: newRole,
  };

  const mock = mockPut(`/clans/${clanId}/members/${memberId}`).willResolve(response(updatedMember));

  expect(await updateClanMember(clanId, memberId, newRole)).toEqual(updatedMember);

  expect(mock).toHaveFetchedWithBody({ role: newRole });
});

it('kickClanMember', async () => {
  const clanId = 1;
  const memberId = 4;

  mockDelete(`/clans/${clanId}/members/${memberId}`).willResolve(null, 204);

  expect(await kickClanMember(clanId, memberId)).toEqual(null);
});

it('inviteToClan', async () => {
  const clanId = 1;
  const inviteeId = 3;
  const newInvitation = {
    id: 1,
    invitee: {},
    inviter: {},
    type: 'Request',
    status: 'Pending',
  } as ClanInvitation;

  const mock = mockPost(`/clans/${clanId}/invitations`).willResolve(response(newInvitation));

  expect(await inviteToClan(clanId, inviteeId)).toEqual(newInvitation);

  expect(mock).toHaveFetchedWithBody({ inviteeId: inviteeId });
});

it('getClanInvitations', async () => {
  const clanId = 1;
  const clanInvitations = [
    {
      id: 1,
      invitee: {},
      inviter: {},
      type: 'Request',
      status: 'Pending',
    },
  ] as ClanInvitation[];

  mockGet(`/clans/${clanId}/invitations?type%5B%5D=Request&status%5B%5D=Pending`, true).willResolve(
    response(clanInvitations)
  );

  expect(
    await getClanInvitations(clanId, [ClanInvitationType.Request], [ClanInvitationStatus.Pending])
  ).toEqual(clanInvitations);
});

it('respondToClanInvitation', async () => {
  const clanId = 1;
  const clanInvitationId = 4;
  const clanInvitation = {
    id: 1,
    invitee: {},
    inviter: {},
    type: 'Request',
    status: 'Accepted',
  } as ClanInvitation;

  const mock = mockPut(`/clans/${clanId}/invitations/${clanInvitationId}/response`).willResolve(
    response(clanInvitation)
  );

  expect(await respondToClanInvitation(clanId, clanInvitationId, true)).toEqual(clanInvitation);
  expect(mock).toHaveFetchedWithBody({ accept: true });
});

it.each<[PartialDeep<ClanMember[]>, number, PartialDeep<ClanMember> | null]>([
  [[], 1, null],
  [[{ user: { id: 1 } }], 1, { user: { id: 1 } }],
  [[{ user: { id: 1 } }, { user: { id: 2 } }], 2, { user: { id: 2 } }],
  [[{ user: { id: 2 } }], 1, null],
])('getClanMember - members: %j, userId: %s', (members, userId, expectation) => {
  expect(getClanMember(members as ClanMember[], userId)).toEqual(expectation);
});

it.each<[ClanMemberRole, boolean]>([
  [ClanMemberRole.Leader, true],
  [ClanMemberRole.Officer, true],
  [ClanMemberRole.Member, false],
])('canManageApplicationsValidate - role: %j', (role, expectation) => {
  expect(canManageApplicationsValidate(role)).toEqual(expectation);
});

it.each<[ClanMemberRole, boolean]>([
  [ClanMemberRole.Leader, true],
  [ClanMemberRole.Officer, false],
  [ClanMemberRole.Member, false],
])('canUpdateClanValidate - role: %j', (role, expectation) => {
  expect(canUpdateClanValidate(role)).toEqual(expectation);
});

it.each<[ClanMemberRole, boolean]>([
  [ClanMemberRole.Leader, true],
  [ClanMemberRole.Officer, false],
  [ClanMemberRole.Member, false],
])('canUpdateMemberValidate - role: %j', (role, expectation) => {
  expect(canUpdateMemberValidate(role)).toEqual(expectation);
});

it.each<[PartialDeep<ClanMember>, PartialDeep<ClanMember>, number, boolean]>([
  // You can't leave the clan if there is more than one person
  [
    { role: ClanMemberRole.Leader, user: { id: 1 } },
    { role: ClanMemberRole.Leader, user: { id: 1 } },
    2,
    false,
  ],
  [
    { role: ClanMemberRole.Leader, user: { id: 1 } },
    { role: ClanMemberRole.Leader, user: { id: 1 } },
    1,
    true,
  ],
  // You can leave the clan
  [
    { role: ClanMemberRole.Officer, user: { id: 1 } },
    { role: ClanMemberRole.Officer, user: { id: 1 } },
    2,
    true,
  ],
  [
    { role: ClanMemberRole.Leader, user: { id: 1 } },
    { role: ClanMemberRole.Officer, user: { id: 2 } },
    2,
    true,
  ],
  // an officer cannot kick a leader
  [
    { role: ClanMemberRole.Officer, user: { id: 1 } },
    { role: ClanMemberRole.Leader, user: { id: 2 } },
    2,
    false,
  ],

  // an officer can kick a member
  [
    { role: ClanMemberRole.Officer, user: { id: 1 } },
    { role: ClanMemberRole.Member, user: { id: 2 } },
    2,
    true,
  ],
])(
  'canKickMemberValidate - selfMember: %j, member: %j, clanMembersCount: %s',
  (selfMember, member, clanMembersCount, expectation) => {
    expect(
      canKickMemberValidate(selfMember as ClanMember, member as ClanMember, clanMembersCount)
    ).toEqual(expectation);
  }
);
