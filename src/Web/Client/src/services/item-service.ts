/* eslint-disable no-bitwise */

import { get } from '@/services/crpg-client';
import Item from '@/models/item';
import ItemType from '@/models/item-type';
import DamageType from '@/models/damage-type';
import { ItemProperties } from '@/models/item-properties';
import WeaponFlags from '@/models/weapon-flags';
import ItemSlot from '@/models/item-slot';

const itemTypeToStr: Record<ItemType, string> = {
  [ItemType.HeadArmor]: 'Head Armor',
  [ItemType.Cape]: 'Cape',
  [ItemType.BodyArmor]: 'Body Armor',
  [ItemType.HandArmor]: 'Hand Armor',
  [ItemType.LegArmor]: 'Leg Armor',
  [ItemType.HorseHarness]: 'Horse Harness',
  [ItemType.Horse]: 'Horse',
  [ItemType.Shield]: 'Shield',
  [ItemType.Bow]: 'Bow',
  [ItemType.Crossbow]: 'Crossbow',
  [ItemType.OneHandedWeapon]: 'One Handed Weapon',
  [ItemType.TwoHandedWeapon]: 'Two Handed Weapon',
  [ItemType.Polearm]: 'Polearm',
  [ItemType.Thrown]: 'Thrown',
  [ItemType.Arrows]: 'Arrows',
  [ItemType.Bolts]: 'Bolts',
};

const damageTypeToStr: Record<DamageType, string> = {
  [DamageType.Cut]: 'Cut',
  [DamageType.Pierce]: 'Pierce',
  [DamageType.Blunt]: 'Blunt',
};

const weaponFlagsStr: Record<WeaponFlags, string> = {
  [WeaponFlags.MeleeWeapon]: 'Melee',
  [WeaponFlags.RangedWeapon]: 'Ranged',
  [WeaponFlags.BonusAgainstShield]: 'BonusAgainstShield',
  [WeaponFlags.CanPenetrateShield]: 'PenetrateShield',
  [WeaponFlags.CantReloadOnHorseback]: 'HorseReload',
  [WeaponFlags.AutoReload]: 'AutoReload',
  [WeaponFlags.CrushThrough]: 'CrushThrough',
  // [WeaponFlags.TwoHandIdleOnMount]:
  [WeaponFlags.PenaltyWithShield]: 'PenaltyWithShield',
  [WeaponFlags.CanKnockDown]: 'KnockDown',
  [WeaponFlags.CanBlockRanged]: 'BlockRanged',
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

const itemTypesBySlot: Record<ItemSlot, ItemType[]> = {
  [ItemSlot.Head]: [ItemType.HeadArmor],
  [ItemSlot.Cape]: [ItemType.Cape],
  [ItemSlot.Body]: [ItemType.BodyArmor],
  [ItemSlot.Hand]: [ItemType.HandArmor],
  [ItemSlot.Leg]: [ItemType.LegArmor],
  [ItemSlot.HorseHarness]: [ItemType.HorseHarness],
  [ItemSlot.Horse]: [ItemType.Horse],
  [ItemSlot.Weapon1]: weaponTypes,
  [ItemSlot.Weapon2]: weaponTypes,
  [ItemSlot.Weapon3]: weaponTypes,
  [ItemSlot.Weapon4]: weaponTypes,
};

function getWeaponFlags(flags: WeaponFlags): string[] {
  return Object.entries<string>(weaponFlagsStr)
    .filter(([flag, _]) => (flags & flag as any) !== 0)
    .map(([a, flagStr]) => flagStr);
}

export function getItems(): Promise<Item[]> {
  return get('/items');
}

export function getItemProperties(item: Item) : ItemProperties {
  const props = new ItemProperties();
  props.common = [
    ['Type', itemTypeToStr[item.type]],
    ['Weight', item.weight],
  ];

  switch (item.type) {
    case ItemType.HeadArmor:
      props.common.push(['Head Armor', item.headArmor]);
      break;
    case ItemType.Cape:
      props.common.push(['Body Armor', item.bodyArmor]);
      break;
    case ItemType.BodyArmor:
      props.common.push(
        ['Head Armor', item.headArmor],
        ['Body Armor', item.bodyArmor],
        ['Arm Armor', item.armArmor],
        ['Leg Armor', item.legArmor],
      );
      break;
    case ItemType.HandArmor:
      props.common.push(['Arm Armor', item.armArmor]);
      break;
    case ItemType.LegArmor:
      props.common.push(['Leg Armor', item.legArmor]);
      break;
    case ItemType.HorseHarness:
      props.common.push(['Body Armor', item.bodyArmor]);
      break;
    case ItemType.Horse:
      props.common.push(
        ['Body Length', item.bodyLength],
        ['Charge Damage', item.chargeDamage],
        ['Maneuver', item.maneuver],
        ['Speed', item.speed],
        ['Hit Points', item.hitPoints],
      );
      break;
    case ItemType.Shield:
      props.common.push(
        ['Hit Points', item.stackAmount],
        ['Weapon Length', item.weaponLength],
      );
      break;
    case ItemType.Bow:
    case ItemType.Crossbow:
      props.common.push(
        ['Accuracy', item.accuracy],
        ['Missile Speed', item.missileSpeed],
        ['Damage', item.primaryThrustDamage],
        ['Speed', item.primaryThrustSpeed],
        ['Damage Type', damageTypeToStr[item.thrustDamageType!]],
        ['Weapon Length', item.weaponLength],
      );
      props.primaryFlags = getWeaponFlags(item.primaryWeaponFlags!);
      break;
    case ItemType.OneHandedWeapon:
      props.common.push(['Weapon Length', item.weaponLength]);
      props.primary = [
        ['Swing Damage', item.primarySwingDamage],
        ['Swing Speed', item.primarySwingSpeed],
        ['Swing Damage Type', damageTypeToStr[item.swingDamageType!]],
        ['Thrust Damage', item.primaryThrustDamage],
        ['Thrust Speed', item.primaryThrustSpeed],
        ['Thrust Damage Type', damageTypeToStr[item.thrustDamageType!]],
      ];
      props.primaryFlags = getWeaponFlags(item.primaryWeaponFlags!);
      break;
    case ItemType.TwoHandedWeapon:
    case ItemType.Polearm:
      props.common.push(['Weapon Length', item.weaponLength]);
      props.primary = [
        ['Swing Damage', item.primarySwingDamage],
        ['Swing Speed', item.primarySwingSpeed],
        ['Swing Damage Type', damageTypeToStr[item.swingDamageType!]],
        ['Thrust Damage', item.primaryThrustDamage],
        ['Thrust Speed', item.primaryThrustSpeed],
        ['Thrust Damage Type', damageTypeToStr[item.thrustDamageType!]],
      ];
      props.secondary = [
        ['Swing Damage', item.secondarySwingDamage],
        ['Swing Speed', item.secondarySwingSpeed],
        ['Swing Damage Type', damageTypeToStr[item.swingDamageType!]],
        ['Thrust Damage', item.secondaryThrustDamage],
        ['Thrust Speed', item.secondaryThrustSpeed],
        ['Thrust Damage Type', damageTypeToStr[item.thrustDamageType!]],
      ];
      props.primaryFlags = getWeaponFlags(item.primaryWeaponFlags!);
      props.secondaryFlags = getWeaponFlags(item.secondaryWeaponFlags!);
      break;
    case ItemType.Thrown:
      props.common.push(['Weapon Length', item.weaponLength]);
      props.primary = [
        ['Swing Damage', item.primarySwingDamage],
        ['Swing Speed', item.primarySwingSpeed],
        ['Swing Damage Type', damageTypeToStr[item.swingDamageType!]],
        ['Thrust Damage', item.primaryThrustDamage],
        ['Thrust Speed', item.primaryThrustSpeed],
        ['Thrust Damage Type', damageTypeToStr[item.thrustDamageType!]],
        ['Accuracy', item.accuracy],
        ['Stack Amount', item.stackAmount],
        ['Missile Speed', item.missileSpeed],
      ];
      props.secondary = [
        ['Swing Damage', item.secondarySwingDamage],
        ['Swing Speed', item.secondarySwingSpeed],
        ['Swing Damage Type', damageTypeToStr[item.swingDamageType!]],
        ['Thrust Damage', item.secondaryThrustDamage],
        ['Thrust Speed', item.secondaryThrustSpeed],
        ['Thrust Damage Type', damageTypeToStr[item.thrustDamageType!]],
      ];
      props.primaryFlags = getWeaponFlags(item.primaryWeaponFlags!);
      props.secondaryFlags = getWeaponFlags(item.secondaryWeaponFlags!);
      break;
    case ItemType.Arrows:
    case ItemType.Bolts:
      props.common.push(
        ['Stack Amount', item.stackAmount],
        ['Missile Speed', item.missileSpeed],
        ['Weapon Length', item.weaponLength],
      );
      break;
    default:
      break;
  }

  return props;
}

export function filterItemsFittingInSlot(items: Item[], slot: ItemSlot) : Item[] {
  return items.filter(i => itemTypesBySlot[slot].includes(i.type));
}
