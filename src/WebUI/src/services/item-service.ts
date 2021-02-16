import { get } from '@/services/crpg-client';
import Item from '@/models/item';
import ItemType from '@/models/item-type';
import DamageType from '@/models/damage-type';
import { ItemDescriptor } from '@/models/item-descriptor';
import WeaponFlags from '@/models/weapon-flags';
import ItemSlot from '@/models/item-slot';
import ItemWeaponComponent from '@/models/item-weapon-component';
import WeaponClass from '@/models/weapon-class';

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
  [WeaponFlags.MeleeWeapon]: 'Melee',
  [WeaponFlags.RangedWeapon]: 'Ranged',
  [WeaponFlags.FirearmAmmo]: null,
  [WeaponFlags.NotUsableWithOneHand]: 'TwoHandOnly',
  [WeaponFlags.NotUsableWithTwoHand]: 'OneHandOnly',
  [WeaponFlags.WideGrip]: 'WideGrip',
  [WeaponFlags.AttachAmmoToVisual]: null,
  [WeaponFlags.Consumable]: null,
  [WeaponFlags.HasHitPoints]: null,
  [WeaponFlags.HasString]: null,
  [WeaponFlags.StringHeldByHand]: null,
  [WeaponFlags.UnloadWhenSheathed]: null,
  [WeaponFlags.AffectsArea]: 'AffectsArea',
  [WeaponFlags.Burning]: 'Burning',
  [WeaponFlags.BonusAgainstShield]: 'BonusAgainstShield',
  [WeaponFlags.CanPenetrateShield]: 'PenetrateShield',
  [WeaponFlags.CantReloadOnHorseback]: 'HorseReload',
  [WeaponFlags.AutoReload]: 'AutoReload',
  [WeaponFlags.CrushThrough]: 'CrushThrough',
  [WeaponFlags.TwoHandIdleOnMount]: 'IdleOnMount',
  [WeaponFlags.NoBlood]: null,
  [WeaponFlags.PenaltyWithShield]: 'PenaltyWithShield',
  [WeaponFlags.CanDismount]: null,
  [WeaponFlags.MissileWithPhysics]: null,
  [WeaponFlags.MultiplePenetration]: 'MultiplePenetration',
  [WeaponFlags.CanKnockDown]: 'KnockDown',
  [WeaponFlags.CanBlockRanged]: 'BlockRanged',
  [WeaponFlags.LeavesTrail]: null,
  [WeaponFlags.UseHandAsThrowBase]: null,
  [WeaponFlags.AmmoBreaksOnBounceBack]: null,
  [WeaponFlags.AmmoCanBreakOnBounceBack]: null,
  [WeaponFlags.AmmoSticksWhenShot]: null,
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
export function getItemDescriptor(item: Item): ItemDescriptor {
  const props: ItemDescriptor = {
    fields: [
      ['Type', itemTypeToStr[item.type]],
      ['Culture', item.culture],
      ['Weight', item.weight],
    ],
    modes: [],
  };

  if (item.armor !== null) {
    if (item.armor.headArmor !== 0) {
      props.fields.push(['Head Armor', item.armor.headArmor]);
    }

    if (item.armor.bodyArmor !== 0) {
      props.fields.push([
        item.type === ItemType.MountHarness ? 'Mount Armor' : 'Body Armor',
        item.armor!.bodyArmor,
      ]);
    }

    if (item.armor.armArmor !== 0) {
      props.fields.push(['Arm Armor', item.armor.armArmor]);
    }

    if (item.armor.legArmor !== 0) {
      props.fields.push(['Leg Armor', item.armor.legArmor]);
    }
  }

  if (item.mount !== null) {
    props.fields.push(
      ['Charge Dmg.', item.mount.chargeDamage],
      ['Speed', item.mount.speed],
      ['Maneuver', item.mount.maneuver],
      ['Hit Points', item.mount.hitPoints]
    );
  }

  // Special cases for item types with only one weapon mode.
  if (item.type === ItemType.Arrows || item.type === ItemType.Bolts) {
    props.fields.push(
      ['Speed', item.weapons[0].missileSpeed],
      [
        'Damage',
        getDamageFieldValue(item.weapons[0].thrustDamage, item.weapons[0].thrustDamageType),
      ],
      ['Length', item.weapons[0].length],
      ['Ammo', item.weapons[0].stackAmount]
    );
  } else if (item.type === ItemType.Shield) {
    props.fields.push(
      ['Speed', item.weapons[0].swingSpeed],
      ['Durability', item.weapons[0].stackAmount],
      ['Armor', item.weapons[0].bodyArmor],
      ['Length', item.weapons[0].length]
    );
  } else if (item.type === ItemType.Bow || item.type === ItemType.Crossbow) {
    props.fields.push(
      [
        'Damage',
        getDamageFieldValue(item.weapons[0].thrustDamage, item.weapons[0].thrustDamageType),
      ],
      ['Fire Rate', item.weapons[0].swingSpeed],
      ['Accuracy', item.weapons[0].accuracy],
      ['Missile Speed', item.weapons[0].missileSpeed],
      ['Length', item.weapons[0].length]
    );
  } else if (item.type === ItemType.Banner) {
    props.fields.push(['Length', item.weapons[0].length]);
  } else {
    item.weapons.forEach(weapon => {
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

export function filterItemsFittingInSlot(items: Item[], slot: ItemSlot): Item[] {
  return items.filter(i => itemTypesBySlot[slot].includes(i.type));
}

export function filterItemsByType(
  items: Item[],
  types: ItemType[]
): { item: Item; weaponIdx: number | undefined }[] {
  if (types.length === 0) {
    return items.map(i => ({ item: i, weaponIdx: undefined }));
  }

  const filteredItems = [];
  // eslint-disable-next-line no-restricted-syntax
  for (const item of items) {
    if (item.weapons.length === 0) {
      if (types.includes(item.type)) {
        filteredItems.push({ item, weaponIdx: undefined });
      }

      continue; // eslint-disable-line no-continue
    }

    const weaponIdx = item.weapons.findIndex(w => types.includes(itemTypeByWeaponClass[w.class]));
    if (weaponIdx !== -1) {
      filteredItems.push({ item, weaponIdx });
    }
  }

  return filteredItems;
}
