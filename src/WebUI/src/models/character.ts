import { type UserItem } from '@/models/user';
import { type ItemSlot } from '@/models/item';

export interface Character {
  id: number;
  name: string;
  generation: number;
  level: number;
  experience: number;
  forTournament: boolean;
  class: CharacterClass;
}

export enum CharacterClass {
  Peasant = 'Peasant',
  Infantry = 'Infantry',
  ShockInfantry = 'ShockInfantry',
  Skirmisher = 'Skirmisher',
  Crossbowman = 'Crossbowman',
  Archer = 'Archer',
  Cavalry = 'Cavalry',
  MountedArcher = 'MountedArcher',
}

export interface CharacterAttributes {
  points: number;
  strength: number;
  agility: number;
}

export interface CharacterSkills {
  points: number;
  ironFlesh: number;
  powerStrike: number;
  powerDraw: number;
  powerThrow: number;
  athletics: number;
  riding: number;
  weaponMaster: number;
  mountedArchery: number;
  shield: number;
}

export interface CharacterWeaponProficiencies {
  points: number;
  oneHanded: number;
  twoHanded: number;
  polearm: number;
  bow: number;
  throwing: number;
  crossbow: number;
}

export interface CharacterCharacteristics {
  attributes: CharacterAttributes;
  skills: CharacterSkills;
  weaponProficiencies: CharacterWeaponProficiencies;
}

export type CharacteristicSectionKey = keyof CharacterCharacteristics;
export type AttributeKey = keyof CharacterAttributes;
export type SkillKey = keyof CharacterSkills;
export type WeaponProficienciesKey = keyof CharacterWeaponProficiencies;
export type CharacteristicKey = AttributeKey | SkillKey | WeaponProficienciesKey;

export enum CharacteristicConversion {
  AttributesToSkills = 'AttributesToSkills',
  SkillsToAttributes = 'SkillsToAttributes',
}

export interface CharacterLimitations {
  lastRespecializeAt: Date;
}

export interface CharacterStatistics {
  kills: number;
  deaths: number;
  assists: number;
  playTime: number;
}

export interface CharacterRating {
  value: number;
  deviation: number;
  volatility: number;
  competitiveValue: number;
}

export interface CharacterSpeedStats {
  weightReductionFactor: number;
  freeWeight: number;
  perceivedWeight: number;
  nakedSpeed: number;
  currentSpeed: number;
  timeToMaxSpeed: number;

  maxWeaponLength: number;
  movementSpeedPenaltyWhenAttacking: number;
}

export interface UpdateCharacterRequest {
  name: string;
}

export interface EquippedItem {
  slot: ItemSlot;
  userItem: UserItem;
}

export type EquippedItemsBySlot = Record<ItemSlot, UserItem>;

export interface EquippedItemId {
  slot: ItemSlot;
  userItemId: number | null;
}

export interface CharacterOverallItemsStats {
  averageRepairCostByHour: number;
  price: number;
  weight: number;
  armArmor: number;
  bodyArmor: number;
  headArmor: number;
  legArmor: number;
  mountArmor: number;
  longestWeaponLength: number;
}

export enum CharacterArmorOverallKey {
  ArmArmor = 'ArmArmor',
  BodyArmor = 'BodyArmor',
  HeadArmor = 'HeadArmor',
  LegArmor = 'LegArmor',
  MountArmor = 'MountArmor',
}

export interface CharacterArmorOverall {
  key: CharacterArmorOverallKey;
  value: number;
}
