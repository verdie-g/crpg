import { mockGet, mockPut, mockDelete } from 'vi-fetch';
import { PartialDeep } from 'type-fest';

import { response } from '@/__mocks__/crpg-client';
import mockCharacters from '@/__mocks__/characters.json';
import mockCharacterCharacteristics from '@/__mocks__/character-characteristics.json';
import mockCharacterStatistics from '@/__mocks__/character-statistics.json';
import {
  type Character,
  type CharacterStatistics,
  type CharacterCharacteristics,
  CharacteristicConversion,
  type CharacterLimitations,
} from '@/models/character';
import { ItemType, type Item } from '@/models/item';

vi.mock('@/services/auth-service', () => ({
  getToken: vi.fn().mockResolvedValue('mockedToken'),
}));

import {
  type RespecCapability,
  getCharacters,
  updateCharacter,
  activateCharacter,
  deleteCharacter,
  respecializeCharacter,
  canSetCharacterForTournamentValidate,
  setCharacterForTournament,
  retireCharacter,
  canRetireValidate,
  getCharacterStatistics,
  getCharacterCharacteristics,
  convertCharacterCharacteristics,
  updateCharacterCharacteristics,
  getExperienceForLevel,
  attributePointsForLevel,
  skillPointsForLevel,
  wppForLevel,
  wppForAgility,
  wppForWeaponMaster,
  createCharacteristics,
  computeHealthPoints,
  getHeirloomPointByLevel,
  getHeirloomPointByLevelAggregation,
  getExperienceMultiplierBonus,
  getCharacterKDARatio,
  getRespecCapability,
  validateItemNotMeetRequirement,
  computeOverallWeight,
} from './characters-service';

beforeEach(() => {
  vi.useFakeTimers();
});

afterEach(() => {
  vi.useRealTimers();
});

it('getCharacters', async () => {
  mockGet('/users/self/characters').willResolve(response(mockCharacters));

  expect(await getCharacters()).toEqual(mockCharacters);
});

it('updateCharacter', async () => {
  mockPut('/users/self/characters/123').willResolve(response(mockCharacters[0]));

  expect(await updateCharacter(123, { name: 'Twilight Sparkle' })).toEqual(mockCharacters[0]);
});

it('deleteCharacter', async () => {
  mockDelete('/users/self/characters/123').willResolve(null, 204);

  expect(await deleteCharacter(123)).toEqual(null);
});

it('respecializeCharacter', async () => {
  mockPut('/users/self/characters/123/respecialize').willResolve(response(mockCharacters[0]));

  expect(await respecializeCharacter(123)).toEqual(mockCharacters[0]);
});

it.todo('TODO: getCharacterLimitations', () => {});

it.each<[Partial<Character>, boolean]>([
  [{ forTournament: false, generation: 0, level: 29 }, true],
  [{ forTournament: false, generation: 1, level: 29 }, false],
  [{ forTournament: false, generation: 0, level: 31 }, false],
  [{ forTournament: true, generation: 0, level: 29 }, false],
  [{ forTournament: false, generation: 0, level: 30 }, true],
  [{ forTournament: false, generation: 0, level: 1 }, true],
  [{ forTournament: true, generation: 1, level: 31 }, false],
])('canSetCharacterForTournamentValidate - character: %j', (character, expectation) => {
  expect(canSetCharacterForTournamentValidate(character as Character)).toEqual(expectation);
});

it('setCharacterForTournament', async () => {
  mockPut('/users/self/characters/123/tournament').willResolve(response(mockCharacters[0]));

  expect(await setCharacterForTournament(123)).toEqual(mockCharacters[0]);
});

it('retireCharacter', async () => {
  mockPut('/users/self/characters/123/retire').willResolve(response(mockCharacters[0]));

  expect(await retireCharacter(123)).toEqual(mockCharacters[0]);
});

it.each([
  [0, false],
  [30, false],
  [31, true],
  [38, true],
])('canRetireValidate - level: %s', (level, expectation) => {
  expect(canRetireValidate(level)).toEqual(expectation);
});

it('activateCharacter', async () => {
  mockPut('/users/self/characters/2/active').willResolve(null, 204);

  expect(await activateCharacter(2, true)).toEqual(null);
});

it('getCharacterStatistics', async () => {
  mockGet('/users/self/characters/12/statistics').willResolve(
    response<CharacterStatistics>(mockCharacterStatistics)
  );

  expect(await getCharacterStatistics(12)).toEqual(mockCharacterStatistics);
});

it('getCharacterCharacteristics', async () => {
  mockGet('/users/self/characters/5/characteristics').willResolve(
    response<CharacterCharacteristics>(mockCharacterCharacteristics)
  );

  expect(await getCharacterCharacteristics(5)).toEqual(mockCharacterCharacteristics);
});

it('convertCharacterCharacteristics', async () => {
  mockPut('/users/self/characters/6/characteristics/convert').willResolve(
    response<CharacterCharacteristics>(mockCharacterCharacteristics)
  );

  expect(
    await convertCharacterCharacteristics(6, CharacteristicConversion.AttributesToSkills)
  ).toEqual(mockCharacterCharacteristics);
});

it('updateCharacterCharacteristics', async () => {
  mockPut('/users/self/characters/4/characteristics').willResolve(
    response<CharacterCharacteristics>(mockCharacterCharacteristics)
  );

  expect(await updateCharacterCharacteristics(4, mockCharacterCharacteristics)).toEqual(
    mockCharacterCharacteristics
  );
});

it.each([
  [0, 0],
  [1, 0],
  [2, 388],
  [30, 4420824],
  [31, 8841648], // 30 lvl *2^1
  [36, 282932736], // 30 lvl * 2^6
  [38, 1131730944], // 30 lvl * 2^8
])('getExperienceForLevel - level: %s', (level, expectation) => {
  expect(getExperienceForLevel(level)).toEqual(expectation);
});

it.each([
  [0, 0],
  [1, 0],
  [2, 1],
  [10, 9],
  [38, 37],
])('attributePointsForLevel - level: %s', (level, expectation) => {
  expect(attributePointsForLevel(level)).toEqual(expectation);
});

it.each([
  [0, 2],
  [1, 2],
  [2, 3],
  [10, 11],
  [38, 39],
])('skillPointsForLevel - level: %s', (level, expectation) => {
  expect(skillPointsForLevel(level)).toEqual(expectation);
});

it.each([
  [0, 52],
  [1, 57],
  [2, 62],
  [10, 111],
  [38, 382],
])('wppForLevel - level: %s', (level, expectation) => {
  expect(wppForLevel(level)).toEqual(expectation);
});

it.each([
  [0, 0],
  [1, 14],
  [2, 28],
  [10, 140],
  [30, 420],
])('wppForAgility - agility: %s', (agility, expectation) => {
  expect(wppForAgility(agility)).toEqual(expectation);
});

it.each([
  [0, 0],
  [1, 75],
  [2, 170],
  [10, 1650],
])('wppForWeaponMaster - wm: %s', (wm, expectation) => {
  expect(wppForWeaponMaster(wm)).toEqual(expectation);
});

it('createCharacteristics', () => {
  expect(
    createCharacteristics({
      attributes: { points: 3 },
      skills: {
        ironFlesh: 3,
      },
      weaponProficiencies: {
        bow: 44,
      },
    })
  ).toEqual({
    attributes: {
      points: 3,
      strength: 0,
      agility: 0,
    },
    skills: {
      points: 0,
      ironFlesh: 3,
      powerStrike: 0,
      powerDraw: 0,
      powerThrow: 0,
      athletics: 0,
      riding: 0,
      weaponMaster: 0,
      mountedArchery: 0,
      shield: 0,
    },
    weaponProficiencies: {
      points: 0,
      oneHanded: 0,
      twoHanded: 0,
      polearm: 0,
      bow: 44,
      throwing: 0,
      crossbow: 0,
    },
  });
});

it.each([
  [0, 3, 63],
  [1, 3, 66],
])('computeHealthPoints - wm: %s', (ironFlesh, strength, expectation) => {
  expect(computeHealthPoints(ironFlesh, strength)).toEqual(expectation);
});

it.each([
  [1, 0],
  [30, 0],
  [31, 1],
  [32, 2],
  [33, 4],
  [34, 8],
  [35, 16],
  [36, 32],
])('getHeirloomPointByLevel - level: %s', (level, expectation) => {
  expect(getHeirloomPointByLevel(level)).toEqual(expectation);
});

it('getHeirloomPointByLevelAggregation', () => {
  expect(getHeirloomPointByLevelAggregation()).toEqual([
    { points: 1, level: [31] },
    { points: 2, level: [32] },
    { points: 4, level: [33] },
    { points: 8, level: [34] },
    { points: 16, level: [35] },
    { points: 32, level: [36] },
    { points: 64, level: [37] },
    { points: 128, level: [38] },
  ]);
});

it.each([
  [0, 0.03],
  [1, 0.03],
  [1.47, 0.03],
  [1.48, 0],
  [2, 0],
])('getExperienceMultiplierBonus - multiplier: %s', (multiplier, expectation) => {
  expect(getExperienceMultiplierBonus(multiplier)).toEqual(expectation);
});

it.each<[Partial<CharacterStatistics>, number]>([
  [{ kills: 0, deaths: 0, assists: 0 }, 0],
  [{ kills: 2, deaths: 6, assists: 3 }, 0.83],
  [{ kills: 1, deaths: 1, assists: 1 }, 2],
  [{ kills: 1, deaths: 0, assists: 1 }, 2],
])('getCharacterKDARatio - characterStatistics: %j', (characterStatistics, expectation) => {
  expect(getCharacterKDARatio(characterStatistics as CharacterStatistics)).toEqual(expectation);
});

it.each<[Partial<Character>, CharacterLimitations, number, RespecCapability]>([
  // tournament char - respec always is free
  [
    { forTournament: true, experience: 100000 },
    {
      lastRespecializeAt: new Date('2023-03-23T18:00:00.0000000Z'),
    },
    10,
    {
      price: 0,
      nextFreeAt: { days: 0, hours: 0, minutes: 5 },
      enabled: true,
    },
  ],
  // Respec was exactly a week ago + 5 minutes (5 minute margin just in case)
  [
    { forTournament: false, experience: 10 },
    {
      lastRespecializeAt: new Date('2023-03-23T17:55:00.0000000Z'),
    },
    100000,
    {
      price: 0,
      nextFreeAt: { days: 0, hours: 0, minutes: 0 },
      enabled: true,
    },
  ],
])(
  'getRespecCapability - character: %j, limitations: %j, gold: %n',
  (character, charLimitations, gold, expectation) => {
    vi.setSystemTime('2023-03-30T18:00:00.0000000Z');

    expect(getRespecCapability(character as Character, charLimitations, gold)).toEqual(expectation);
  }
);

it('getRespecCapability - Exponential Decay', () => {
  vi.setSystemTime('2023-03-30T18:00:00.0000000Z');

  const exp = 100000000; // 100m
  const gold = 1000000;

  const result1 = getRespecCapability(
    { forTournament: false, experience: exp } as Character,
    { lastRespecializeAt: new Date('2023-03-30T17:00:00.0000000Z') },
    gold
  );

  const result2 = getRespecCapability(
    { forTournament: false, experience: exp } as Character,
    { lastRespecializeAt: new Date('2023-03-30T16:00:00.0000000Z') },
    gold
  );

  expect(result2.price < result1.price).toBeTruthy();
});

it.each<[PartialDeep<Item>, PartialDeep<CharacterCharacteristics>, boolean]>([
  [{ requirement: 18 }, { attributes: { strength: 18 } }, false],
  [{ requirement: 17 }, { attributes: { strength: 18 } }, false],
  [{ requirement: 19 }, { attributes: { strength: 18 } }, true],
  [{ requirement: 0 }, { attributes: { strength: 18 } }, false],
])(
  'validateItemNotMeetRequirement - item: %j, characterCharacteristics: %j, ',
  (item, characterCharacteristics, expectation) => {
    expect(
      validateItemNotMeetRequirement(
        item as Item,
        characterCharacteristics as CharacterCharacteristics
      )
    ).toEqual(expectation);
  }
);

it.each<[PartialDeep<Item[]>, number]>([
  [
    [
      {
        weight: 0.1,
        weapons: [{ stackAmount: 12 }],
        type: ItemType.Bolts,
      },
    ],
    1.2,
  ],
  [
    [
      {
        weight: 3.8,
        weapons: [{ stackAmount: 70 }],
        type: ItemType.Shield,
      },
    ],
    3.8,
  ],
])('computeOverallWeight - items: %j, expectedWeight: %j, ', (items, expectation) => {
  expect(computeOverallWeight(items as Item[])).toEqual(expectation);
});
