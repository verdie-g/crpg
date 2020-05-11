/* eslint-disable no-bitwise */

import { get } from '@/services/crpg-client';
import Item from '@/models/item';
import ItemType from '@/models/item-type';
import DamageType from '@/models/damage-type';
import { ItemDescriptor } from '@/models/item-descriptor';
import WeaponFlags from '@/models/weapon-flags';
import ItemSlot from '@/models/item-slot';

export const itemTypeToStr: Record<ItemType, string> = {
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

function getDamageFields(item: Item, primary: boolean): [string, any][] {
  const fields: [string, any][] = [];

  if (primary) {
    if (item.primarySwingSpeed !== null) {
      fields.push(
        ['Swing Spd.', item.primarySwingSpeed],
        ['Swing Dmg.', item.primarySwingDamage],
        ['Swing Dmg. Type', damageTypeToStr[item.swingDamageType!]],
      );
    }

    if (item.primaryThrustSpeed !== null) {
      fields.push(
        ['Thrust Spd.', item.primaryThrustSpeed],
        ['Thrust Dmg.', item.primaryThrustDamage],
        ['Thrust Dmg. Type', damageTypeToStr[item.thrustDamageType!]],
      );
    }
  } else {
    if (item.secondarySwingSpeed !== null) {
      fields.push(
        ['Swing Spd.', item.secondarySwingSpeed],
        ['Swing Dmg.', item.secondarySwingDamage],
        ['Swing Dmg. Type', damageTypeToStr[item.swingDamageType!]],
      );
    }

    if (item.secondaryThrustSpeed !== null) {
      fields.push(
        ['Thrust Spd.', item.secondaryThrustSpeed],
        ['Thrust Dmg.', item.secondaryThrustDamage],
        ['Thrust Dmg. Type', damageTypeToStr[item.thrustDamageType!]],
      );
    }
  }

  return fields;
}

function getWeaponFlags(flags: WeaponFlags): string[] {
  return Object.entries<string>(weaponFlagsStr)
    .filter(([flag]) => (flags & flag as any) !== 0)
    .map(([, flagStr]) => flagStr);
}

export function getItems(): Promise<Item[]> {
  return get('/items');
}

export function getItemDescriptor(item: Item): ItemDescriptor {
  const props: ItemDescriptor = {
    fields: [
      ['Type', itemTypeToStr[item.type]],
      ['Weight', item.weight],
    ],
    modes: [],
  };

  switch (item.type) {
    case ItemType.HeadArmor:
      props.fields.push(['Head Armor', item.headArmor]);
      break;
    case ItemType.Cape:
      props.fields.push(['Body Armor', item.bodyArmor]);
      break;
    case ItemType.BodyArmor:
      props.fields.push(
        ['Head Armor', item.headArmor],
        ['Body Armor', item.bodyArmor],
        ['Arm Armor', item.armArmor],
        ['Leg Armor', item.legArmor],
      );
      break;
    case ItemType.HandArmor:
      props.fields.push(['Arm Armor', item.armArmor]);
      break;
    case ItemType.LegArmor:
      props.fields.push(['Leg Armor', item.legArmor]);
      break;
    case ItemType.HorseHarness:
      props.fields.push(['Body Armor', item.bodyArmor]);
      break;
    case ItemType.Horse:
      props.fields.push(
        ['Charge Dmg.', item.chargeDamage],
        ['Speed', item.speed],
        ['Maneuver', item.maneuver],
        ['Hit Points', item.hitPoints],
      );
      break;
    case ItemType.Shield:
      props.fields.push(
        ['Speed', item.primarySwingSpeed],
        ['Durability', item.stackAmount],
        ['Armor', item.bodyArmor],
        ['Length', item.weaponLength],
      );
      break;
    case ItemType.Bow:
    case ItemType.Crossbow:
      props.modes.push({
        name: 'One Handed',
        fields: [
          ['Length', item.weaponLength],
          ['Damage', item.primaryThrustDamage],
          ['Damage Type', damageTypeToStr[item.thrustDamageType!]],
          ['Fire Rate', item.primaryThrustSpeed],
          ['Accuracy', item.accuracy],
          ['Missile Spd.', item.missileSpeed],
        ],
        flags: getWeaponFlags(item.primaryWeaponFlags!),
      });
      break;
    case ItemType.OneHandedWeapon:
      props.fields.push(['Length', item.weaponLength]);
      props.modes.push({
        name: 'One Handed',
        fields: [
          ...getDamageFields(item, true),
          ['Handling', item.primaryHandling],
        ],
        flags: getWeaponFlags(item.primaryWeaponFlags!),
      });
      break;
    case ItemType.TwoHandedWeapon:
    case ItemType.Polearm:
      props.fields.push(['Length', item.weaponLength]);
      props.modes.push({
        name: 'Two Handed',
        fields: [
          ...getDamageFields(item, true),
          ['Handling', item.primaryHandling],
        ],
        flags: getWeaponFlags(item.primaryWeaponFlags!),
      });
      if (item.secondarySwingDamage !== null) {
        props.modes.push({
          name: 'One H.',
          fields: [
            ...getDamageFields(item, false),
            ['Handling', item.primaryHandling],
          ],
          flags: getWeaponFlags(item.secondaryWeaponFlags!),
        });
      }
      break;
    case ItemType.Thrown:
      props.fields.push(['Length', item.weaponLength]);
      props.modes.push({
        name: 'Thrown',
        fields: [
          ...getDamageFields(item, true),
          ['Fire Rate', item.missileSpeed],
          ['Accuracy', item.accuracy],
          ['Stack Amnt.', item.stackAmount],
        ],
        flags: getWeaponFlags(item.primaryWeaponFlags!),
      });

      props.modes.push({
        name: 'One handed',
        fields: [
          ...getDamageFields(item, false),
          ['Handling', item.primaryHandling],
        ],
        flags: getWeaponFlags(item.secondaryWeaponFlags!),
      });
      break;
    case ItemType.Arrows:
    case ItemType.Bolts:
      props.fields.push(
        ['Ammo', item.stackAmount],
        ['Missile Spd.', item.missileSpeed],
        ['Length', item.weaponLength],
      );
      break;
    default:
      break;
  }

  return props;
}

export function filterItemsFittingInSlot(items: Item[], slot: ItemSlot): Item[] {
  return items.filter(i => itemTypesBySlot[slot].includes(i.type));
}
