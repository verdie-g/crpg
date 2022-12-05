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
import { applyPolynomialFunction } from '@/utils/math';
import Constants from '../../../../data/constants.json';
import ItemFlags from '@/models/item-flags';

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

// Set null flag we don't to display
const itemFlagsStr: Record<ItemFlags, string | null> = {
  [ItemFlags.ForceAttachOffHandPrimaryItemBone]: null,
  [ItemFlags.ForceAttachOffHandSecondaryItemBone]: null,
  [ItemFlags.NotUsableByFemale]: null,
  [ItemFlags.NotUsableByMale]: null,
  [ItemFlags.DropOnWeaponChange]: 'DropOnWeaponChange',
  [ItemFlags.DropOnAnyAction]: 'DropOnAnyAction',
  [ItemFlags.CannotBePickedUp]: null,
  [ItemFlags.CanBePickedUpFromCorpse]: null,
  [ItemFlags.QuickFadeOut]: null,
  [ItemFlags.WoodenAttack]: null,
  [ItemFlags.WoodenParry]: null,
  [ItemFlags.HeldInOffHand]: null,
  [ItemFlags.HasToBeHeldUp]: null,
  [ItemFlags.UseTeamColor]: 'UseTeamColor',
  [ItemFlags.Civilian]: null,
  [ItemFlags.DoNotScaleBodyAccordingToWeaponLength]: null,
  [ItemFlags.DoesNotHideChest]: null,
  [ItemFlags.NotStackable]: 'NotStackable',
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
  ItemType.Banner,
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
  [ItemSlot.WeaponExtra]: [ItemType.Banner],
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
const itemUsageStr = new Map<string, string>([
  ['long_bow', 'Long Bow'],
  ['bow', 'Bow'],
  ['crossbow', 'Heavy Crossbow'],
  ['crossbow_light', 'Regular Crossbow'],
]);

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

function getWeaponModeName(weaponComponent: ItemWeaponComponent): string {
  switch (weaponComponent.itemUsage) {
    case 'polearm_couch':
      return 'Couch';
    case 'polearm_bracing':
      return 'Brace';
  }

  switch (itemTypeByWeaponClass[weaponComponent.class]) {
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

function getItemFlags(flags: ItemFlags[]): string[] {
  return flags.map(flag => itemFlagsStr[flag]).filter(flagStr => flagStr !== null) as string[];
}

function getWeaponFlags(flags: WeaponFlags[]): string[] {
  return flags.map(flag => weaponFlagsStr[flag]).filter(flagStr => flagStr !== null) as string[];
}

export function getItems(): Promise<Item[]> {
  return get('/items');
}

// Inspired by TooltipVMExtensions.UpdateTooltip.
// eslint-disable-next-line @typescript-eslint/no-unused-vars
export function getItemDescriptor(baseItem: Item, rank: number): ItemDescriptor {
  const props: ItemDescriptor = {
    fields: [
      ['Type', itemTypeToStr[baseItem.type]],
      ['Culture', baseItem.culture],
      ['Tier', baseItem.tier.toFixed(1)],
      ['Repair Cost', `${computeAverageRepairCostByHour([baseItem])} / hour`],
    ],
    flags: getItemFlags(baseItem.flags),
    modes: [],
  };
  if (
    baseItem.type == ItemType.Thrown ||
    baseItem.type == ItemType.Bolts ||
    baseItem.type == ItemType.Arrows
  ) {
    props.fields.push(['Unit Weight', baseItem.weight.toFixed(2)]);
    props.fields.push([
      'Stack Weight',
      (baseItem.weight * baseItem.weapons[0].stackAmount).toFixed(2),
    ]);
  } else if (baseItem.type == ItemType.Mount) {
    props.fields.push(['Weight', baseItem.weight.toFixed(0)]);
  } else {
    props.fields.push(['Weight', baseItem.weight.toFixed(2)]);
  }
  switch (baseItem.type) {
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
    props.fields.push(['Class', itemUsageStr.get(baseItem.weapons[0].itemUsage)]);
    baseItem.weapons.forEach(weapon => {
      const weaponFields: [string, any][] = [
        [
          'Damage',
          getDamageFieldValue(
            baseItem.weapons[0].thrustDamage,
            baseItem.weapons[0].thrustDamageType
          ),
        ],
        ['Accuracy', baseItem.weapons[0].accuracy],
        ['Missile Speed', baseItem.weapons[0].missileSpeed],
        ['Length', baseItem.weapons[0].length],
        ['Reload Speed', baseItem.weapons[0].swingSpeed],
        ['Aim Speed', baseItem.weapons[0].thrustSpeed],
      ];

      props.modes.push({
        name: getWeaponModeName(weapon),
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

      weaponFields.push(['Handling', weapon.handling], ['Reach', weapon.length]);

      props.modes.push({
        name: getWeaponModeName(weapon),
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

// In filterUserItemsFittingInSlot we allow the user to chose any weapon for any weapon slot but actually if the weapon
// has the DropOnWeaponChange or DropOnAnyAction it should be moved to the WeaponExtra slot. See method TaleWorlds.Core.Equipment.IsItemFitsToSlot.
export function overrideSelectedSlot(userItem: UserItem, slot: ItemSlot): ItemSlot {
  const item = userItem.baseItem;
  if (!weaponTypes.includes(item.type)) {
    return slot;
  }

  if (
    item.flags.includes(ItemFlags.DropOnWeaponChange) ||
    item.flags.includes(ItemFlags.DropOnAnyAction)
  ) {
    return ItemSlot.WeaponExtra;
  }

  return slot;
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

export function computeSalePrice(userItem: UserItem): { price: number; graceTimeEnd: Date | null } {
  const graceTimeEnd = new Date(userItem.createdAt);
  graceTimeEnd.setHours(graceTimeEnd.getHours() + 1);

  if (graceTimeEnd < new Date(Date.now())) {
    return {
      price: Math.floor(
        applyPolynomialFunction(userItem.baseItem.price, Constants.itemSellCostCoefs)
      ),
      graceTimeEnd: null,
    };
  }

  // If the item was recently bought it is sold at 100% of its original price.
  return { price: userItem.baseItem.price, graceTimeEnd };
}

// TODO: handle upgrade items.
export function computeAverageRepairCostByHour(items: Item[]): number {
  return Math.floor(
    items.reduce(
      (total, item) => total + item.price * Constants.itemRepairCostPerSecond * 3600,
      0
    ) * Constants.itemBreakChance
  );
}
