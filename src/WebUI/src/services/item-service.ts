import { get } from '@/services/crpg-client';
import Item from '@/models/item';
import ItemType from '@/models/item-type';
import DamageType from '@/models/damage-type';
import { ItemDescriptor } from '@/models/item-descriptor';
import WeaponFlags from '@/models/weapon-flags';
import ItemSlot from '@/models/item-slot';
import ItemWeaponComponent from '@/models/item-weapon-component';
import WeaponClass from '@/models/weapon-class';
import UserItem from '@/models/user-item';
import { applyPolynomialFunction, generalizedMean } from '@/utils/math';
import Constants from '../../../../data/constants.json';
import EquippedItem from '@/models/equipped-item';

export const itemTypeToStr: Record<ItemType, string> = {
  [ItemType.Undefined]: 'Undefined',
  [ItemType.HeadArmor]: 'Head Armor',
  [ItemType.ShoulderArmor]: 'Shoulder Armor',
  [ItemType.BodyArmor]: 'Body Armor',
  [ItemType.HandArmor]: 'Hand Armor',
  [ItemType.LegArmor]: 'Leg Armor',
  [ItemType.MountHarness]: 'Mount Harness',
  [ItemType.Mount]: 'Mount',
  [ItemType.Shield]: 'Shield',
  [ItemType.Bow]: 'Bow',
  [ItemType.Crossbow]: 'Crossbow',
  [ItemType.OneHandedWeapon]: 'One Handed Weapon',
  [ItemType.TwoHandedWeapon]: 'Two Handed Weapon',
  [ItemType.Polearm]: 'Polearm',
  [ItemType.Thrown]: 'Thrown',
  [ItemType.Arrows]: 'Arrows',
  [ItemType.Bolts]: 'Bolts',
  [ItemType.Pistol]: 'Pistol',
  [ItemType.Musket]: 'Musket',
  [ItemType.Bullets]: 'Bullets',
  [ItemType.Banner]: 'Banner',
};

const damageTypeToStr: Record<DamageType, string> = {
  [DamageType.Undefined]: '',
  [DamageType.Cut]: 'c',
  [DamageType.Pierce]: 'p',
  [DamageType.Blunt]: 'b',
};

// Set null flag we don't to display
const weaponFlagsStr: Record<WeaponFlags, string | null> = {
  [WeaponFlags.AffectsAreaBig]: 'AffectsAreaBig',
  [WeaponFlags.AffectsArea]: 'AffectsArea',
  [WeaponFlags.AmmoBreaksOnBounceBack]: null,
  [WeaponFlags.AmmoCanBreakOnBounceBack]: null,
  [WeaponFlags.AmmoSticksWhenShot]: null,
  [WeaponFlags.AttachAmmoToVisual]: null,
  [WeaponFlags.AutoReload]: 'AutoReload',
  [WeaponFlags.BonusAgainstShield]: 'BonusAgainstShield',
  [WeaponFlags.Burning]: 'Burning',
  [WeaponFlags.CanBlockRanged]: 'CanBlockRanged',
  [WeaponFlags.CanCrushThrough]: 'CrushThrough',
  [WeaponFlags.CanDismount]: 'CanDismount',
  [WeaponFlags.CanHook]: 'CanHook',
  [WeaponFlags.CanKnockDown]: 'CanKnockDown',
  [WeaponFlags.CanPenetrateShield]: 'CanPenetrateShield',
  [WeaponFlags.CantReloadOnHorseback]: 'CantReloadOnHorseback',
  [WeaponFlags.Consumable]: null,
  [WeaponFlags.FirearmAmmo]: null,
  [WeaponFlags.HasHitPoints]: null,
  [WeaponFlags.HasString]: null,
  [WeaponFlags.LeavesTrail]: null,
  [WeaponFlags.MeleeWeapon]: 'MeleeWeapon',
  [WeaponFlags.MissileWithPhysics]: null,
  [WeaponFlags.MultiplePenetration]: 'MultiplePenetration',
  [WeaponFlags.NoBlood]: null,
  [WeaponFlags.NotUsableWithOneHand]: 'TwoHandOnly',
  [WeaponFlags.NotUsableWithTwoHand]: 'OneHandOnly',
  [WeaponFlags.PenaltyWithShield]: 'PenaltyWithShield',
  [WeaponFlags.RangedWeapon]: 'RangedWeapon',
  [WeaponFlags.StringHeldByHand]: null,
  [WeaponFlags.TwoHandIdleOnMount]: 'TwoHandIdleOnMount',
  [WeaponFlags.UnloadWhenSheathed]: null,
  [WeaponFlags.UseHandAsThrowBase]: null,
  [WeaponFlags.WideGrip]: 'WideGrip',
};

const weaponTypes: ItemType[] = [
  ItemType.Shield,
  ItemType.Bow,
  ItemType.Crossbow,
  ItemType.OneHandedWeapon,
  ItemType.TwoHandedWeapon,
  ItemType.Polearm,
  ItemType.Thrown,
  ItemType.Arrows,
  ItemType.Bolts,
];

const armorTypes: ItemType[] = [
  ItemType.HeadArmor,
  ItemType.ShoulderArmor,
  ItemType.BodyArmor,
  ItemType.HandArmor,
  ItemType.LegArmor,
];

const itemTypesBySlot: Record<ItemSlot, ItemType[]> = {
  [ItemSlot.Head]: [ItemType.HeadArmor],
  [ItemSlot.Shoulder]: [ItemType.ShoulderArmor],
  [ItemSlot.Body]: [ItemType.BodyArmor],
  [ItemSlot.Hand]: [ItemType.HandArmor],
  [ItemSlot.Leg]: [ItemType.LegArmor],
  [ItemSlot.MountHarness]: [ItemType.MountHarness],
  [ItemSlot.Mount]: [ItemType.Mount],
  [ItemSlot.Weapon0]: weaponTypes,
  [ItemSlot.Weapon1]: weaponTypes,
  [ItemSlot.Weapon2]: weaponTypes,
  [ItemSlot.Weapon3]: weaponTypes,
};

const itemTypeByWeaponClass: Record<WeaponClass, ItemType> = {
  [WeaponClass.Undefined]: ItemType.Undefined,
  [WeaponClass.Dagger]: ItemType.OneHandedWeapon,
  [WeaponClass.OneHandedSword]: ItemType.OneHandedWeapon,
  [WeaponClass.OneHandedAxe]: ItemType.OneHandedWeapon,
  [WeaponClass.Mace]: ItemType.OneHandedWeapon,
  [WeaponClass.TwoHandedSword]: ItemType.TwoHandedWeapon,
  [WeaponClass.TwoHandedAxe]: ItemType.TwoHandedWeapon,
  [WeaponClass.Pick]: ItemType.TwoHandedWeapon,
  [WeaponClass.TwoHandedMace]: ItemType.TwoHandedWeapon,
  [WeaponClass.OneHandedPolearm]: ItemType.OneHandedWeapon,
  [WeaponClass.TwoHandedPolearm]: ItemType.Polearm,
  [WeaponClass.LowGripPolearm]: ItemType.Polearm,
  [WeaponClass.Arrow]: ItemType.Arrows,
  [WeaponClass.Bolt]: ItemType.Bolts,
  [WeaponClass.Cartridge]: ItemType.Bullets,
  [WeaponClass.Bow]: ItemType.Bow,
  [WeaponClass.Crossbow]: ItemType.Crossbow,
  [WeaponClass.Stone]: ItemType.Thrown,
  [WeaponClass.Boulder]: ItemType.Thrown,
  [WeaponClass.ThrowingAxe]: ItemType.Thrown,
  [WeaponClass.ThrowingKnife]: ItemType.Thrown,
  [WeaponClass.Javelin]: ItemType.Thrown,
  [WeaponClass.Pistol]: ItemType.Pistol,
  [WeaponClass.Musket]: ItemType.Musket,
  [WeaponClass.SmallShield]: ItemType.Shield,
  [WeaponClass.LargeShield]: ItemType.Shield,
  [WeaponClass.Banner]: ItemType.Banner,
};

function getDamageFieldValue(damage: number, damageType: DamageType): string {
  return `${damage}${damageTypeToStr[damageType]}`;
}

function getDamageFields(weaponComponent: ItemWeaponComponent): [string, any][] {
  const fields: [string, any][] = [];

  if (weaponComponent.swingDamageType !== DamageType.Undefined) {
    fields.push(
      ['Swing Speed', weaponComponent.swingSpeed],
      [
        'Swing Damage',
        getDamageFieldValue(weaponComponent.swingDamage, weaponComponent.swingDamageType),
      ]
    );
  }

  if (weaponComponent.thrustDamageType !== DamageType.Undefined) {
    fields.push(
      ['Thrust Speed', weaponComponent.thrustSpeed],
      [
        'Thrust Damage',
        getDamageFieldValue(weaponComponent.thrustDamage, weaponComponent.thrustDamageType),
      ]
    );
  }

  return fields;
}

function getWeaponClassShortName(weaponClass: WeaponClass): string {
  switch (itemTypeByWeaponClass[weaponClass]) {
    case ItemType.OneHandedWeapon:
      return '1H';
    case ItemType.TwoHandedWeapon:
    case ItemType.Polearm:
      return '2H';
    case ItemType.Thrown:
      return 'Thrown';
    default:
      return '';
  }
}

function getWeaponFlags(flags: WeaponFlags[]): string[] {
  return flags.map(flag => weaponFlagsStr[flag]).filter(flagStr => flagStr !== null) as string[];
}

export function getItems(): Promise<Item[]> {
  return get('/items');
}

// Inspired by TooltipVMExtensions.UpdateTooltip.
export function getItemDescriptor(baseItem: Item, rank: number): ItemDescriptor {
  const props: ItemDescriptor = {
    fields: [
      ['Type', itemTypeToStr[baseItem.type]],
      ['Culture', baseItem.culture],
      ['Weight', baseItem.weight],
    ],
    modes: [],
  };
  switch (baseItem.type) {
    case ItemType.HeadArmor:
    case ItemType.BodyArmor:
    case ItemType.ShoulderArmor:
    case ItemType.HandArmor:
    case ItemType.LegArmor:
    case ItemType.Crossbow:
      props.fields.push(['Requirement', baseItem.requirement + ' STR']);
      break;
  }
  if (baseItem.armor !== null) {
    if (baseItem.armor.headArmor !== 0) {
      props.fields.push(['Head Armor', baseItem.armor.headArmor]);
    }

    if (baseItem.armor.bodyArmor !== 0) {
      props.fields.push([
        baseItem.type === ItemType.MountHarness ? 'Mount Armor' : 'Body Armor',
        baseItem.armor!.bodyArmor,
      ]);
    }

    if (baseItem.armor.armArmor !== 0) {
      props.fields.push(['Arm Armor', baseItem.armor.armArmor]);
    }

    if (baseItem.armor.legArmor !== 0) {
      props.fields.push(['Leg Armor', baseItem.armor.legArmor]);
    }
  }

  if (baseItem.mount !== null) {
    props.fields.push(
      ['Charge Damage', baseItem.mount.chargeDamage],
      ['Speed', baseItem.mount.speed],
      ['Maneuver', baseItem.mount.maneuver],
      ['Hit Points', baseItem.mount.hitPoints]
    );
  }

  // Special cases for item types with only one weapon mode.
  if (baseItem.type === ItemType.Arrows || baseItem.type === ItemType.Bolts) {
    props.fields.push(
      ['Speed', baseItem.weapons[0].missileSpeed],
      [
        'Damage',
        getDamageFieldValue(baseItem.weapons[0].thrustDamage, baseItem.weapons[0].thrustDamageType),
      ],
      ['Length', baseItem.weapons[0].length],
      ['Ammo', baseItem.weapons[0].stackAmount]
    );
  } else if (baseItem.type === ItemType.Shield) {
    props.fields.push(
      ['Speed', baseItem.weapons[0].swingSpeed],
      ['Durability', baseItem.weapons[0].stackAmount],
      ['Armor', baseItem.weapons[0].bodyArmor],
      ['Length', baseItem.weapons[0].length]
    );
  } else if (baseItem.type === ItemType.Bow || baseItem.type === ItemType.Crossbow) {
    baseItem.weapons.forEach(weapon => {
      const weaponFields: [string, any][] = [
        [
          'Damage',
          getDamageFieldValue(
            baseItem.weapons[0].thrustDamage,
            baseItem.weapons[0].thrustDamageType
          ),
        ],
        ['Fire Rate', baseItem.weapons[0].swingSpeed],
        ['Accuracy', baseItem.weapons[0].accuracy],
        ['Missile Speed', baseItem.weapons[0].missileSpeed],
        ['Length', baseItem.weapons[0].length],
      ];

      props.modes.push({
        name: getWeaponClassShortName(weapon.class),
        fields: weaponFields,
        flags: getWeaponFlags(weapon.flags),
      });
    });
  } else if (baseItem.type === ItemType.Banner) {
    props.fields.push(['Length', baseItem.weapons[0].length]);
  } else {
    baseItem.weapons.forEach(weapon => {
      const itemType = itemTypeByWeaponClass[weapon.class];
      const weaponFields: [string, any][] = [];
      if (
        itemType === ItemType.OneHandedWeapon ||
        itemType === ItemType.TwoHandedWeapon ||
        itemType === ItemType.Polearm
      ) {
        weaponFields.push(...getDamageFields(weapon));
      } else if (itemType === ItemType.Thrown) {
        weaponFields.push(
          ['Damage', getDamageFieldValue(weapon.thrustDamage, weapon.thrustDamageType)],
          ['Fire Rate', weapon.missileSpeed],
          ['Accuracy', weapon.accuracy],
          ['Stack Amount', weapon.stackAmount]
        );
      }

      weaponFields.push(['Handling', weapon.handling], ['Length', weapon.length]);

      props.modes.push({
        name: getWeaponClassShortName(weapon.class),
        fields: weaponFields,
        flags: getWeaponFlags(weapon.flags),
      });
    });
  }

  return props;
}

export function filterUserItemsFittingInSlot(items: UserItem[], slot: ItemSlot): UserItem[] {
  return items.filter(i => itemTypesBySlot[slot].includes(i.baseItem.type));
}

export function getSlotsForUserItem(item: UserItem): ItemSlot[] {
  return Object.entries(itemTypesBySlot)
    .filter(([, value]) => value.includes(item.baseItem.type))
    .flatMap(([key]) => key as ItemSlot);
}

export function filterItemsByType(
  items: Item[],
  type: ItemType | null
): { item: Item; weaponIdx: number | undefined }[] {
  if (type === null) {
    return items.map(i => ({ item: i, weaponIdx: undefined }));
  }

  const filteredItems = [];
  // eslint-disable-next-line no-restricted-syntax
  for (const item of items) {
    const weaponIdx = item.weapons.findIndex(w => itemTypeByWeaponClass[w.class] === type);
    // An expection is made for thrown to also see the throwable polearms.
    if (item.type !== type && (type !== ItemType.Thrown || weaponIdx === -1)) {
      continue; // eslint-disable-line no-continue
    }

    if (item.weapons.length === 0) {
      filteredItems.push({ item, weaponIdx: undefined });
    } else if (weaponIdx !== -1) {
      filteredItems.push({ item, weaponIdx });
    }
  }

  return filteredItems;
}

export function computeSalePrice(item: UserItem): number {
  const salePrice = applyPolynomialFunction(item.baseItem.price, Constants.itemSellCostCoefs);
  // Floor salePrice to match behaviour of backend int typecast
  return Math.floor(salePrice);
}

export function computeArmorSetPieceStrengthRequirement(items: UserItem[]): number {
  const numberOfArmorItemTypes = 5;
  const armorsRequirement = new Array(numberOfArmorItemTypes).fill(0);
  items
    .filter(e => armorTypes.includes(e.baseItem.type))
    .forEach((ei, i) => (armorsRequirement[i] = ei.baseItem.requirement));
  return Math.trunc(
    generalizedMean(Constants.armorSetRequirementPowerMeanPValue, armorsRequirement)
  );
}

export function sortUserItems(
  userItems: UserItem[],
  sortBy: string,
  sortDesc: boolean
): UserItem[] {
  return userItems.sort((i1, i2) => compareItems(i1.baseItem, i2.baseItem, sortBy, sortDesc));
}

export function getSortableProperties(items: Item[]): string[] {
  let sortableItemProperties = ['Price'];
  sortableItemProperties = sortableItemProperties.concat(
    items
      .map(item => getItemDescriptor(item, 0).fields.map(field => field[0]))
      .flat()
      .filter(
        (itemProperty, index, self) =>
          itemProperty !== 'Type' &&
          itemProperty !== 'Culture' &&
          self.indexOf(itemProperty) === index
      )
  );
  const sortableWeaponProperties = items
    .map(item => getItemDescriptor(item, 0).modes.map(mode => mode.fields.map(field => field[0])))
    .flat()
    .flat()
    .filter(
      (itemProperty, index, self) =>
        sortableItemProperties.indexOf(itemProperty) === -1 && self.indexOf(itemProperty) === index
    );
  return sortableItemProperties.concat(sortableWeaponProperties);
}

function compareItems(i1: Item, i2: Item, sortBy: string, sortDesc: boolean): number {
  if (sortBy === 'Price') {
    if (sortDesc) return i2.price - i1.price;
    return i1.price - i2.price;
  }
  const itemDesc1 = getItemDescriptor(i1, 0);
  const itemDesc2 = getItemDescriptor(i2, 0);
  let field1 = itemDesc1.fields.find(field => field[0] === sortBy);
  let field2 = itemDesc2.fields.find(field => field[0] === sortBy);

  if (!field1 && !field2) {
    field1 = itemDesc1.modes
      .map(mode => mode.fields)
      .flatMap(f => f)
      .find(field => field[0] === sortBy);

    field2 = itemDesc2.modes
      .map(mode => mode.fields)
      .flatMap(f => f)
      .find(field => field[0] === sortBy);
  }

  let value1 = !!field1 ? field1[1] : '';
  let value2 = !!field2 ? field2[1] : '';
  if (isNaN(Number(value1))) {
    const value1WithoutLetters = value1.replace(/\D/g, '');
    if (!isNaN(Number(value1WithoutLetters))) value1 = value1WithoutLetters;
  }

  if (isNaN(Number(value2))) {
    const value2WithoutLetters = value2.replace(/\D/g, '');
    if (!isNaN(Number(value2WithoutLetters))) value2 = value2WithoutLetters;
  }

  if (!isNaN(Number(value1)) || !isNaN(Number(value2))) {
    if (sortDesc) return value2 - value1;
    return value1 - value2;
  }

  if (value1 || value2) {
    return value1 > value2 && sortDesc ? -1 : 1;
  }

  return -1;
}

export function itemFitsInFreeWeaponSlot(
  equippedItems: EquippedItem[],
  userItem: UserItem
): boolean {
  const userItemsFittingInWeaponSlot = filterUserItemsFittingInSlot([userItem], ItemSlot.Weapon0);
  if (!userItemsFittingInWeaponSlot.length) return false;
  const weaponSlotsInUse = equippedItems.filter(
    item =>
      item.slot === ItemSlot.Weapon0 ||
      item.slot === ItemSlot.Weapon1 ||
      item.slot === ItemSlot.Weapon2 ||
      item.slot === ItemSlot.Weapon3
  );
  return weaponSlotsInUse.length < 4;
}
