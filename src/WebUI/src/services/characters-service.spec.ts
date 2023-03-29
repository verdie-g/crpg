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
} from '@/models/character';
import { type Item } from '@/models/item';

vi.mock('@/services/auth-service', () => ({
  getToken: vi.fn().mockResolvedValue('mockedToken'),
}));

import {
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
  getExperienceMultiplierBonus,
  getCharacterKDARatio,
  getRespecCapability,
  validateItemNotMeetRequirement,
} from './characters-service';

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
  [32, 1],
  [33, 2],
  [34, 2],
  [35, 3],
  [36, 3],
])('getHeirloomPointByLevel - level: %s', (level, expectation) => {
  expect(getHeirloomPointByLevel(level)).toEqual(expectation);
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

it.todo('TODO: getRespecCapability', () => {});

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
