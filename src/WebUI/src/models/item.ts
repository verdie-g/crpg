import { Culture } from '@/models/culture';

export enum ArmorMaterialType {
  Undefined = 'Undefined',
  Cloth = 'Cloth',
  Leather = 'Leather',
  Chainmail = 'Chainmail',
  Plate = 'Plate',
}

export interface ItemArmorComponent {
  headArmor: number;
  bodyArmor: number;
  armArmor: number;
  legArmor: number;
  materialType: ArmorMaterialType;
  familyType: number;
}

export interface ItemMountComponent {
  bodyLength: number;
  chargeDamage: number;
  maneuver: number;
  speed: number;
  hitPoints: number;
  familyType: number;
}

export enum WeaponClass {
  Undefined = 'Undefined',
  OneHandedSword = 'OneHandedSword',
  TwoHandedSword = 'TwoHandedSword',
  OneHandedAxe = 'OneHandedAxe',
  TwoHandedAxe = 'TwoHandedAxe',
  Mace = 'Mace',
  Dagger = 'Dagger',
  Pick = 'Pick',
  TwoHandedMace = 'TwoHandedMace',
  TwoHandedPolearm = 'TwoHandedPolearm',
  OneHandedPolearm = 'OneHandedPolearm',
  LowGripPolearm = 'LowGripPolearm',
  Arrow = 'Arrow',
  Bolt = 'Bolt',
  Cartridge = 'Cartridge',
  Bow = 'Bow',
  Crossbow = 'Crossbow',
  Boulder = 'Boulder',
  Javelin = 'Javelin',
  ThrowingAxe = 'ThrowingAxe',
  ThrowingKnife = 'ThrowingKnife',
  Stone = 'Stone',
  Pistol = 'Pistol',
  Musket = 'Musket',
  SmallShield = 'SmallShield',
  LargeShield = 'LargeShield',
  Banner = 'Banner',
}

export enum WeaponFlags {
  MeleeWeapon = 'MeleeWeapon',
  RangedWeapon = 'RangedWeapon',
  FirearmAmmo = 'FirearmAmmo',
  NotUsableWithOneHand = 'NotUsableWithOneHand',
  NotUsableWithTwoHand = 'NotUsableWithTwoHand',
  WideGrip = 'WideGrip',
  AttachAmmoToVisual = 'AttachAmmoToVisual',
  Consumable = 'Consumable',
  HasHitPoints = 'HasHitPoints',
  HasString = 'HasString',
  StringHeldByHand = 'StringHeldByHand',
  UnloadWhenSheathed = 'UnloadWhenSheathed',
  AffectsArea = 'AffectsArea',
  AffectsAreaBig = 'AffectsAreaBig',
  Burning = 'Burning',
  BonusAgainstShield = 'BonusAgainstShield',
  CanPenetrateShield = 'CanPenetrateShield',
  CantReloadOnHorseback = 'CantReloadOnHorseback',
  CanReloadOnHorseback = 'CanReloadOnHorseback', // TODO: custom flag
  CantUseOnHorseback = 'CantUseOnHorseback', // TODO: custom flag
  AutoReload = 'AutoReload',
  TwoHandIdleOnMount = 'TwoHandIdleOnMount',
  NoBlood = 'NoBlood',
  PenaltyWithShield = 'PenaltyWithShield',
  CanDismount = 'CanDismount',
  CanHook = 'CanHook',
  MissileWithPhysics = 'MissileWithPhysics',
  MultiplePenetration = 'MultiplePenetration',
  CanKnockDown = 'CanKnockDown',
  CanBlockRanged = 'CanBlockRanged',
  LeavesTrail = 'LeavesTrail',
  CanCrushThrough = 'CanCrushThrough',
  UseHandAsThrowBase = 'UseHandAsThrowBase',
  AmmoBreaksOnBounceBack = 'AmmoBreaksOnBounceBack',
  AmmoCanBreakOnBounceBack = 'AmmoCanBreakOnBounceBack',
  AmmoSticksWhenShot = 'AmmoSticksWhenShot',
}

export enum DamageType {
  Undefined = 'Undefined',
  Cut = 'Cut',
  Pierce = 'Pierce',
  Blunt = 'Blunt',
}

export interface ItemWeaponComponent {
  class: WeaponClass;
  itemUsage: ItemUsage;
  accuracy: number;
  missileSpeed: number;
  stackAmount: number;
  length: number;
  handling: number;
  bodyArmor: number;
  flags: WeaponFlags[];

  thrustDamage: number;
  thrustDamageType: DamageType;
  thrustSpeed: number;

  swingDamage: number;
  swingDamageType: DamageType;
  swingSpeed: number;
}

export enum ItemFlags {
  ForceAttachOffHandPrimaryItemBone = 'ForceAttachOffHandPrimaryItemBone',
  ForceAttachOffHandSecondaryItemBone = 'ForceAttachOffHandSecondaryItemBone',
  NotUsableByFemale = 'NotUsableByFemale',
  NotUsableByMale = 'NotUsableByMale',
  DropOnWeaponChange = 'DropOnWeaponChange',
  DropOnAnyAction = 'DropOnAnyAction',
  CannotBePickedUp = 'CannotBePickedUp',
  CanBePickedUpFromCorpse = 'CanBePickedUpFromCorpse',
  QuickFadeOut = 'QuickFadeOut',
  WoodenAttack = 'WoodenAttack',
  WoodenParry = 'WoodenParry',
  HeldInOffHand = 'HeldInOffHand',
  HasToBeHeldUp = 'HasToBeHeldUp',
  UseTeamColor = 'UseTeamColor',
  Civilian = 'Civilian',
  DoNotScaleBodyAccordingToWeaponLength = 'DoNotScaleBodyAccordingToWeaponLength',
  DoesNotHideChest = 'DoesNotHideChest',
  NotStackable = 'NotStackable',
}

export enum ItemType {
  Undefined = 'Undefined',
  OneHandedWeapon = 'OneHandedWeapon',
  TwoHandedWeapon = 'TwoHandedWeapon',
  Polearm = 'Polearm',
  Thrown = 'Thrown',
  Bow = 'Bow',
  Crossbow = 'Crossbow',
  Arrows = 'Arrows',
  Bolts = 'Bolts',

  Shield = 'Shield',

  HeadArmor = 'HeadArmor',
  ShoulderArmor = 'ShoulderArmor',
  BodyArmor = 'BodyArmor',
  HandArmor = 'HandArmor',
  LegArmor = 'LegArmor',

  Mount = 'Mount',
  MountHarness = 'MountHarness',

  Pistol = 'Pistol',
  Musket = 'Musket',
  Bullets = 'Bullets',
  Banner = 'Banner',
}

export enum ItemUsage {
  LongBow = 'long_bow',
  Bow = 'bow',
  Crossbow = 'crossbow',
  CrossbowLight = 'crossbow_light',
  PolearmCouch = 'polearm_couch',
  PolearmBracing = 'polearm_bracing',
  PolearmPike = 'polearm_pike',
}

export enum ItemFamilyType {
  Undefined = 0,
  Horse = 1,
  Camel = 2,
}

export type ItemRank = 0 | 1 | 2 | 3;

export interface Item {
  baseId: string;
  id: string;
  rank: ItemRank;
  name: string;
  price: number;
  type: ItemType;
  culture: Culture;
  weight: number;
  requirement: number;
  tier: number;
  flags: ItemFlags[];
  armor: ItemArmorComponent | null;
  mount: ItemMountComponent | null;
  weapons: ItemWeaponComponent[];
}

export enum WeaponUsage {
  Primary = 'Primary',
  Secondary = 'Secondary',
}

export interface ItemFlat {
  id: string;
  baseId: string;
  rank: ItemRank;
  modId: string;
  name: string;
  price: number;
  upkeep: number;
  type: ItemType;
  culture: Culture;
  requirement: number;
  tier: number;
  flags: Array<ItemFlags | WeaponFlags | ItemUsage>;
  weight: number | null;
  // Armor
  headArmor: number | null;
  bodyArmor: number | null;
  armArmor: number | null;
  legArmor: number | null;
  materialType: ArmorMaterialType | null;
  // weapons
  weaponClass: WeaponClass | null;
  itemUsage: ItemUsage[];
  weaponFlags: WeaponFlags[];
  weaponUsage: WeaponUsage[];
  weaponPrimaryClass: WeaponClass | null;

  accuracy: number | null;
  missileSpeed: number | null;
  stackAmount: number | null;
  length: number | null;
  handling: number | null;
  thrustDamage: number | null | undefined;
  thrustDamageType: DamageType | null | undefined;
  thrustSpeed: number | null | undefined;
  swingDamage: number | null | undefined;
  swingDamageType: DamageType | null | undefined;
  swingSpeed: number | null | undefined;

  // Shield
  shieldSpeed: number | null;
  shieldDurability: number | null;
  shieldArmor: number | null;
  // Mount
  bodyLength: number | null;
  chargeDamage: number | null;
  maneuver: number | null;
  speed: number | null;
  hitPoints: number | null;
  mountFamilyType: number | null;
  // MountHarness
  mountArmor: number | null;
  mountArmorFamilyType: number | null;
  // Bow/XBow
  reloadSpeed: number | null;
  aimSpeed: number | null;
  // Arrows/Bolts
  stackWeight: number | null;
  damage: number | null;
  damageType: DamageType | null | undefined;
}

export type ItemDescriptorField = [string, string | number];

export interface ItemDescriptor {
  fields: ItemDescriptorField[];
  flags: string[];
  modes: ItemMode[];
}

export interface ItemMode {
  name: string;
  fields: ItemDescriptorField[];
  flags: string[];
}

export enum ItemSlot {
  Head = 'Head',
  Shoulder = 'Shoulder',
  Body = 'Body',
  Hand = 'Hand',
  Leg = 'Leg',
  MountHarness = 'MountHarness',
  Mount = 'Mount',
  Weapon0 = 'Weapon0',
  Weapon1 = 'Weapon1',
  Weapon2 = 'Weapon2',
  Weapon3 = 'Weapon3',
  WeaponExtra = 'WeaponExtra',
}

export enum ItemFieldFormat {
  List = 'List',
  Damage = 'Damage',
  Requirement = 'Requirement',
  Number = 'Number',
}

export enum ItemFieldCompareRule {
  Bigger = 'Bigger',
  Less = 'Less',
}

export type CompareItemsResult = Partial<Record<keyof ItemFlat, number>>;

export enum ItemCompareMode {
  Absolute = 'Absolute', // The items compared to each other, and the best one is chosen.
  Relative = 'Relative', // The items compared relative to the selected
}
