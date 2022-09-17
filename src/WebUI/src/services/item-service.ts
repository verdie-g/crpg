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
import { i18n } from '@/main';

export function itemTypeToStr(): Map<ItemType, string> {
  const result = new Map<ItemType, string>();
  result.set(ItemType.Undefined, i18n.t('shopFiltersFormItemTypeUndefined').toString());
  result.set(ItemType.HeadArmor, i18n.t('shopFiltersFormItemTypeHeadArmor').toString());
  result.set(ItemType.ShoulderArmor, i18n.t('shopFiltersFormItemTypeShoulderArmor').toString());
  result.set(ItemType.BodyArmor, i18n.t('shopFiltersFormItemTypeBodyArmor').toString());
  result.set(ItemType.HandArmor, i18n.t('shopFiltersFormItemTypeHandArmor').toString());
  result.set(ItemType.LegArmor, i18n.t('shopFiltersFormItemTypeLegArmor').toString());
  result.set(ItemType.MountHarness, i18n.t('shopFiltersFormItemTypeMountHarness').toString());
  result.set(ItemType.Mount, i18n.t('shopFiltersFormItemTypeMount').toString());
  result.set(ItemType.Shield, i18n.t('shopFiltersFormItemTypeShield').toString());
  result.set(ItemType.Bow, i18n.t('shopFiltersFormItemTypeBow').toString());
  result.set(ItemType.Crossbow, i18n.t('shopFiltersFormItemTypeCrossbow').toString());
  result.set(ItemType.OneHandedWeapon, i18n.t('shopFiltersFormItemTypeOneHandedWeapon').toString());
  result.set(ItemType.TwoHandedWeapon, i18n.t('shopFiltersFormItemTypeTwoHandedWeapon').toString());
  result.set(ItemType.Polearm, i18n.t('shopFiltersFormItemTypePolearm').toString());
  result.set(ItemType.Thrown, i18n.t('shopFiltersFormItemTypeThrown').toString());
  result.set(ItemType.Arrows, i18n.t('shopFiltersFormItemTypeArrows').toString());
  result.set(ItemType.Bolts, i18n.t('shopFiltersFormItemTypeBolts').toString());
  result.set(ItemType.Pistol, i18n.t('shopFiltersFormItemTypePistol').toString());
  result.set(ItemType.Musket, i18n.t('shopFiltersFormItemTypeMusket').toString());
  result.set(ItemType.Bullets, i18n.t('shopFiltersFormItemTypeBullets').toString());
  result.set(ItemType.Banner, i18n.t('shopFiltersFormItemTypeBanner').toString());
  return result;
}

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
      [i18n.t('itemPropertiesType').toString(), itemTypeToStr().get(baseItem.type)],
      [i18n.t('itemPropertiesCulture').toString(), baseItem.culture],
      [i18n.t('itemPropertiesWeight').toString(), baseItem.weight],
    ],
    modes: [],
  };

  if (baseItem.armor !== null) {
    if (baseItem.armor.headArmor !== 0) {
      props.fields.push([i18n.t('itemPropertiesHeadArmor').toString(), baseItem.armor.headArmor]);
    }

    if (baseItem.armor.bodyArmor !== 0) {
      props.fields.push([
        baseItem.type === ItemType.MountHarness
          ? i18n.t('itemPropertiesMountArmor').toString()
          : i18n.t('itemPropertiesBodyArmor').toString(),
        baseItem.armor!.bodyArmor,
      ]);
    }

    if (baseItem.armor.armArmor !== 0) {
      props.fields.push([i18n.t('itemPropertiesArmArmor').toString(), baseItem.armor.armArmor]);
    }

    if (baseItem.armor.legArmor !== 0) {
      props.fields.push([i18n.t('itemPropertiesLegArmor').toString(), baseItem.armor.legArmor]);
    }
  }

  if (baseItem.mount !== null) {
    props.fields.push(
      [i18n.t('itemPropertiesChargeDamage').toString(), baseItem.mount.chargeDamage],
      [i18n.t('itemPropertiesSpeed').toString(), baseItem.mount.speed],
      [i18n.t('itemPropertiesManeuver').toString(), baseItem.mount.maneuver],
      [i18n.t('itemPropertiesHitPoints').toString(), baseItem.mount.hitPoints]
    );
  }

  // Special cases for item types with only one weapon mode.
  if (baseItem.type === ItemType.Arrows || baseItem.type === ItemType.Bolts) {
    props.fields.push(
      [i18n.t('itemPropertiesSpeed').toString(), baseItem.weapons[0].missileSpeed],
      [
        i18n.t('itemPropertiesDamage').toString(),
        getDamageFieldValue(baseItem.weapons[0].thrustDamage, baseItem.weapons[0].thrustDamageType),
      ],
      [i18n.t('itemPropertiesLength').toString(), baseItem.weapons[0].length],
      [i18n.t('itemPropertiesAmmo').toString(), baseItem.weapons[0].stackAmount]
    );
  } else if (baseItem.type === ItemType.Shield) {
    props.fields.push(
      [i18n.t('itemPropertiesSpeed').toString(), baseItem.weapons[0].swingSpeed],
      [i18n.t('itemPropertiesDurability').toString(), baseItem.weapons[0].stackAmount],
      [i18n.t('itemPropertiesArmor').toString(), baseItem.weapons[0].bodyArmor],
      [i18n.t('itemPropertiesLength').toString(), baseItem.weapons[0].length]
    );
  } else if (baseItem.type === ItemType.Bow || baseItem.type === ItemType.Crossbow) {
    baseItem.weapons.forEach(weapon => {
      const weaponFields: [string, any][] = [
        [
          i18n.t('itemPropertiesDamage').toString(),
          getDamageFieldValue(
            baseItem.weapons[0].thrustDamage,
            baseItem.weapons[0].thrustDamageType
          ),
        ],
        [i18n.t('itemPropertiesFireRate').toString(), baseItem.weapons[0].swingSpeed],
        [i18n.t('itemPropertiesAccuracy').toString(), baseItem.weapons[0].accuracy],
        [i18n.t('itemPropertiesMissileSpeed').toString(), baseItem.weapons[0].missileSpeed],
        [i18n.t('itemPropertiesLength').toString(), baseItem.weapons[0].length],
      ];

      props.modes.push({
        name: getWeaponClassShortName(weapon.class),
        fields: weaponFields,
        flags: getWeaponFlags(weapon.flags),
      });
    });
  } else if (baseItem.type === ItemType.Banner) {
    props.fields.push([i18n.t('itemPropertiesLength').toString(), baseItem.weapons[0].length]);
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
          [
            i18n.t('itemPropertiesDamage').toString(),
            getDamageFieldValue(weapon.thrustDamage, weapon.thrustDamageType),
          ],
          [i18n.t('itemPropertiesFireRate').toString(), weapon.missileSpeed],
          [i18n.t('itemPropertiesAccuracy').toString(), weapon.accuracy],
          [i18n.t('itemPropertiesStackAmount').toString(), weapon.stackAmount]
        );
      }

      weaponFields.push(
        [i18n.t('itemPropertiesHandling').toString(), weapon.handling],
        [i18n.t('itemPropertiesLength').toString(), weapon.length]
      );

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
    if (item.weapons.length === 0) {
      if (item.type === type) {
        filteredItems.push({ item, weaponIdx: undefined });
      }

      continue; // eslint-disable-line no-continue
    }

    const weaponIdx = item.weapons.findIndex(w => itemTypeByWeaponClass[w.class] === type);
    if (weaponIdx !== -1) {
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
