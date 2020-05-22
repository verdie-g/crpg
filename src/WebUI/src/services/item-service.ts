/* eslint-disable no-bitwise */

import { get } from '@/services/crpg-client';
import Item from '@/models/item';
import ItemType from '@/models/item-type';
import DamageType from '@/models/damage-type';
import { ItemDescriptor } from '@/models/item-descriptor';
import WeaponFlags from '@/models/weapon-flags';
import ItemSlot from '@/models/item-slot';
import ItemWeaponComponent from '@/models/item-weapon-component';

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

function getDamageFields(weaponComponent: ItemWeaponComponent): [string, any][] {
  const fields: [string, any][] = [];

  if (weaponComponent.swingDamage !== 0) {
    fields.push(
      ['Swing Spd.', weaponComponent.swingSpeed],
      ['Swing Dmg.', weaponComponent.swingDamage],
      ['Swing Dmg. Type', damageTypeToStr[weaponComponent.swingDamageType]],
    );
  }

  if (weaponComponent.thrustDamage !== 0) {
    fields.push(
      ['Thrust Spd.', weaponComponent.thrustSpeed],
      ['Thrust Dmg.', weaponComponent.thrustDamage],
      ['Thrust Dmg. Type', damageTypeToStr[weaponComponent.thrustDamageType]],
    );
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
      props.fields.push(['Head Armor', item.armor!.headArmor]);
      break;
    case ItemType.Cape:
      props.fields.push(['Body Armor', item.armor!.bodyArmor]);
      break;
    case ItemType.BodyArmor:
      props.fields.push(
        ['Head Armor', item.armor!.headArmor],
        ['Body Armor', item.armor!.bodyArmor],
        ['Arm Armor', item.armor!.armArmor],
        ['Leg Armor', item.armor!.legArmor],
      );
      break;
    case ItemType.HandArmor:
      props.fields.push(['Arm Armor', item.armor!.armArmor]);
      break;
    case ItemType.LegArmor:
      props.fields.push(['Leg Armor', item.armor!.legArmor]);
      break;
    case ItemType.HorseHarness:
      props.fields.push(['Body Armor', item.armor!.bodyArmor]);
      break;
    case ItemType.Horse:
      props.fields.push(
        ['Charge Dmg.', item.horse!.chargeDamage],
        ['Speed', item.horse!.speed],
        ['Maneuver', item.horse!.maneuver],
        ['Hit Points', item.horse!.hitPoints],
      );
      break;
    case ItemType.Shield:
      props.fields.push(
        ['Speed', item.weapons[0].swingSpeed],
        ['Durability', item.weapons[0].stackAmount],
        ['Armor', item.weapons[0].bodyArmor],
        ['Length', item.weapons[0].length],
      );
      break;
    case ItemType.Bow:
    case ItemType.Crossbow:
      props.modes.push({
        name: 'One Handed',
        fields: [
          ['Length', item.weapons[0].length],
          ['Damage', item.weapons[0].thrustDamage],
          ['Damage Type', damageTypeToStr[item.weapons[0].thrustDamageType]],
          ['Fire Rate', item.weapons[0].thrustSpeed],
          ['Accuracy', item.weapons[0].accuracy],
          ['Missile Spd.', item.weapons[0].missileSpeed],
        ],
        flags: getWeaponFlags(item.weapons[0].flags),
      });
      break;
    case ItemType.OneHandedWeapon:
      props.modes.push({
        name: 'One Handed',
        fields: [
          ...getDamageFields(item.weapons[0]),
          ['Length', item.weapons[0].length],
          ['Handling', item.weapons[0].handling],
        ],
        flags: getWeaponFlags(item.weapons[0].flags),
      });
      break;
    case ItemType.TwoHandedWeapon:
      props.modes.push({
        name: '2H',
        fields: [
          ...getDamageFields(item.weapons[0]),
          ['Length', item.weapons[0].length],
          ['Handling', item.weapons[0].handling],
        ],
        flags: getWeaponFlags(item.weapons[0].flags),
      });

      if (item.weapons.length > 1) {
        props.modes.push({
          name: '1H',
          fields: [
            ...getDamageFields(item.weapons[1]),
            ['Length', item.weapons[1].length],
            ['Handling', item.weapons[1].handling],
          ],
          flags: getWeaponFlags(item.weapons[1].flags),
        });
      }
      break;
    case ItemType.Polearm:
      props.modes.push({
        name: '1H',
        fields: [
          ...getDamageFields(item.weapons[0]),
          ['Length', item.weapons[0].length],
          ['Handling', item.weapons[0].handling],
        ],
        flags: getWeaponFlags(item.weapons[0].flags),
      });

      if (item.weapons.length > 1) {
        props.modes.push({
          name: '2H',
          fields: [
            ...getDamageFields(item.weapons[1]),
            ['Length', item.weapons[1].length],
            ['Handling', item.weapons[1].handling],
          ],
          flags: getWeaponFlags(item.weapons[1].flags),
        });
      }

      if (item.weapons.length > 2) {
        if (item.weapons[2].stackAmount !== 0) { // if thrown
          props.modes.push({
            name: 'Thrown',
            fields: [
              ['Length', item.weapons[2].length],
              ['Damage', item.weapons[2].thrustDamage],
              ['Fire Rate', item.weapons[2].missileSpeed],
              ['Accuracy', item.weapons[2].accuracy],
              ['Stack Amnt.', item.weapons[2].stackAmount],
            ],
            flags: getWeaponFlags(item.weapons[2].flags),
          });
        } else { // else couchable
          props.modes.push({
            name: 'Couch',
            fields: [
              ...getDamageFields(item.weapons[2]),
              ['Length', item.weapons[2].length],
              ['Handling', item.weapons[2].handling],
            ],
            flags: getWeaponFlags(item.weapons[2].flags),
          });
        }
      }
      break;
    case ItemType.Thrown:
      props.modes.push({
        name: 'Thrown',
        fields: [
          ['Length', item.weapons[0].length],
          ['Damage', item.weapons[0].thrustDamage],
          ['Fire Rate', item.weapons[0].missileSpeed],
          ['Accuracy', item.weapons[0].accuracy],
          ['Stack Amnt.', item.weapons[0].stackAmount],
        ],
        flags: getWeaponFlags(item.weapons[0].flags),
      });

      if (item.weapons.length > 1) {
        props.modes.push({
          name: 'One handed',
          fields: [
            ...getDamageFields(item.weapons[1]),
            ['Length', item.weapons[1].length],
            ['Handling', item.weapons[1].handling],
          ],
          flags: getWeaponFlags(item.weapons[1].flags),
        });
      }
      break;
    case ItemType.Arrows:
    case ItemType.Bolts:
      props.fields.push(
        ['Speed', item.weapons[0].missileSpeed],
        ['Damage', item.weapons[0].thrustDamage],
        ['Damage Type', damageTypeToStr[item.weapons[0].thrustDamageType]],
        ['Length', item.weapons[0].length],
        ['Ammo', item.weapons[0].stackAmount],
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
