import {
  itemSellCostCoefs,
  itemRepairCostPerSecond,
  itemBreakChance,
  brokenItemRepairPenaltySeconds,
} from '@root/data/constants.json';
import {
  type Item,
  ItemSlot,
  ItemType,
  WeaponClass,
  ItemFlags,
  DamageType,
  WeaponFlags,
  ItemUsage,
  type ItemFlat,
  ItemFieldFormat,
  ItemFieldCompareRule,
  type CompareItemsResult,
  ItemFamilyType,
  type ItemRank,
} from '@/models/item';
import { type UserItem } from '@/models/user';
import { type AggregationConfig } from '@/models/item-search';
import { type EquippedItemsBySlot } from '@/models/character';
import { Culture } from '@/models/culture';
import { get } from '@/services/crpg-client';
import { aggregationsConfig } from '@/services/item-search-service/aggregations';
import { n, t } from '@/services/translate-service';
import { notify, NotificationType } from '@/services/notification-service';
import { applyPolynomialFunction, roundFLoat } from '@/utils/math';

// TODO: delete mocks
// import {
//   BecDeCorbin,
//   // Longsword,
//   // SimpleShortSpear,
//   // Bow,
//   NobleCavalryLance,
//   Pike,
//   WoodenSword,
// } from '@/services/item-search-service/__tests__/mocks';

// export const getItems = () =>
//   new Promise(res => {
//     res([
//       //
//       BecDeCorbin,
//       // WoodenSword,
//       Pike,
//       NobleCavalryLance,
//     ]);
//   });

export const getItems = () => get<Item[]>('/items');

export const getItemImage = (name: string) => `/items/${name}.png`;

export const getItemUpgrades = (baseId: string) => get<Item[]>(`/items/upgrades/${baseId}`);

export const armorTypes: ItemType[] = [
  ItemType.HeadArmor,
  ItemType.ShoulderArmor,
  ItemType.BodyArmor,
  ItemType.HandArmor,
  ItemType.LegArmor,
];

export const itemTypeByWeaponClass: Record<WeaponClass, ItemType> = {
  [WeaponClass.Undefined]: ItemType.Undefined,
  [WeaponClass.Dagger]: ItemType.OneHandedWeapon,
  [WeaponClass.OneHandedSword]: ItemType.OneHandedWeapon,
  [WeaponClass.OneHandedAxe]: ItemType.OneHandedWeapon,
  [WeaponClass.Mace]: ItemType.OneHandedWeapon,
  [WeaponClass.TwoHandedSword]: ItemType.TwoHandedWeapon,
  [WeaponClass.TwoHandedAxe]: ItemType.TwoHandedWeapon,
  [WeaponClass.Pick]: ItemType.TwoHandedWeapon,
  [WeaponClass.TwoHandedMace]: ItemType.TwoHandedWeapon,
  [WeaponClass.OneHandedPolearm]: ItemType.Polearm,
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

export const WeaponClassByItemUsage: Partial<Record<ItemUsage, WeaponClass>> = {
  [ItemUsage.PolearmCouch]: WeaponClass.OneHandedPolearm,
};

const WeaponClassByItemType: Partial<Record<ItemType, WeaponClass[]>> = {
  [ItemType.OneHandedWeapon]: [
    WeaponClass.OneHandedSword,
    WeaponClass.OneHandedAxe,
    WeaponClass.Mace,
    WeaponClass.Dagger,
  ],
  [ItemType.TwoHandedWeapon]: [
    WeaponClass.TwoHandedSword,
    WeaponClass.TwoHandedAxe,
    WeaponClass.TwoHandedMace,
  ],
  [ItemType.Polearm]: [WeaponClass.TwoHandedPolearm, WeaponClass.OneHandedPolearm],
  [ItemType.Thrown]: [
    WeaponClass.Javelin,
    WeaponClass.ThrowingAxe,
    WeaponClass.ThrowingKnife,
    WeaponClass.Stone,
  ],
  [ItemType.Shield]: [WeaponClass.SmallShield, WeaponClass.LargeShield],
};

export const hasWeaponClassesByItemType = (type: ItemType) =>
  Object.keys(WeaponClassByItemType).includes(type);

export const getWeaponClassesByItemType = (type: ItemType): WeaponClass[] => {
  const weaponClasses = WeaponClassByItemType[type];
  return weaponClasses === undefined ? [] : weaponClasses;
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

export const itemTypesBySlot: Record<ItemSlot, ItemType[]> = {
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

const weaponSlots: ItemSlot[] = [
  ItemSlot.Weapon0,
  ItemSlot.Weapon1,
  ItemSlot.Weapon2,
  ItemSlot.Weapon3,
];

export const itemSlotsByType: Partial<Record<ItemType, ItemSlot[]>> = {
  [ItemType.HeadArmor]: [ItemSlot.Head],
  [ItemType.ShoulderArmor]: [ItemSlot.Shoulder],
  [ItemType.BodyArmor]: [ItemSlot.Body],
  [ItemType.HandArmor]: [ItemSlot.Hand],
  [ItemType.LegArmor]: [ItemSlot.Leg],
  [ItemType.MountHarness]: [ItemSlot.MountHarness],
  [ItemType.Mount]: [ItemSlot.Mount],
  //
  [ItemType.Shield]: weaponSlots,
  [ItemType.Bow]: weaponSlots,
  [ItemType.Crossbow]: weaponSlots,
  [ItemType.OneHandedWeapon]: weaponSlots,
  [ItemType.TwoHandedWeapon]: weaponSlots,
  [ItemType.Polearm]: weaponSlots,
  [ItemType.Thrown]: weaponSlots,
  [ItemType.Arrows]: weaponSlots,
  [ItemType.Bolts]: weaponSlots,
  //
  [ItemType.Banner]: [ItemSlot.WeaponExtra],
};

export const getAvailableSlotsByItem = (
  item: Item,
  equippedItems: EquippedItemsBySlot
): ItemSlot[] => {
  // family type: compatibility with mount and mountHarness
  if (
    item.type === ItemType.MountHarness &&
    ItemSlot.Mount in equippedItems &&
    item.armor!.familyType !== equippedItems[ItemSlot.Mount].item.mount!.familyType
  ) {
    return [];
  }

  if (
    item.type === ItemType.Mount &&
    ItemSlot.MountHarness in equippedItems &&
    item.mount!.familyType !== equippedItems[ItemSlot.MountHarness].item.armor!.familyType
  ) {
    return [];
  }

  // Pikes
  if (item.flags.includes(ItemFlags.DropOnWeaponChange)) {
    return [ItemSlot.WeaponExtra];
  }

  // Banning the use of large shields on horseback
  if (
    (ItemSlot.Mount in equippedItems && isLargeShield(item)) ||
    (item.type === ItemType.Mount &&
      Object.values(equippedItems).some(item => isLargeShield(item.item)))
  ) {
    notify(
      t('character.inventory.item.cantUseOnHorseback.notify.warning'),
      NotificationType.Warning
    );
    return [];
  }

  return itemSlotsByType[item.type]!;
};

export const isLargeShield = (item: Item) =>
  item.type === ItemType.Shield && item.weapons[0].class === WeaponClass.LargeShield;

export const visibleItemFlags: ItemFlags[] = [
  ItemFlags.DropOnWeaponChange,
  ItemFlags.DropOnAnyAction,
  ItemFlags.UseTeamColor,
];

export const visibleWeaponFlags: WeaponFlags[] = [
  // WeaponFlags.AffectsAreaBig,
  // WeaponFlags.AffectsArea,
  // WeaponFlags.AutoReload,
  WeaponFlags.BonusAgainstShield,
  // WeaponFlags.Burning,
  // WeaponFlags.CanBlockRanged,
  WeaponFlags.CanCrushThrough,
  WeaponFlags.CanDismount,
  WeaponFlags.CanHook,
  WeaponFlags.CanKnockDown,
  WeaponFlags.CanPenetrateShield,
  WeaponFlags.CantReloadOnHorseback,
  WeaponFlags.CanReloadOnHorseback, // TODO:
  WeaponFlags.MultiplePenetration,
  WeaponFlags.CantUseOnHorseback,
  // WeaponFlags.TwoHandIdleOnMount, TODO:
];

export const visibleItemUsage: ItemUsage[] = [
  ItemUsage.LongBow,
  ItemUsage.Bow,
  ItemUsage.Crossbow,
  ItemUsage.CrossbowLight,
  ItemUsage.PolearmCouch,
  ItemUsage.PolearmBracing,
  ItemUsage.PolearmPike,
];

export const itemTypeToIcon: Record<ItemType, string> = {
  [ItemType.Undefined]: '',
  [ItemType.HeadArmor]: 'item-type-head-armor',
  [ItemType.ShoulderArmor]: 'item-type-shoulder-armor',
  [ItemType.BodyArmor]: 'item-type-body-armor',
  [ItemType.HandArmor]: 'item-type-hand-armor',
  [ItemType.LegArmor]: 'item-type-leg-armor',
  [ItemType.MountHarness]: 'item-type-mount-harness',
  [ItemType.Mount]: 'item-type-mount',
  [ItemType.Shield]: 'item-type-shield',
  [ItemType.Bow]: 'item-type-bow',
  [ItemType.Crossbow]: 'item-type-crossbow',
  [ItemType.OneHandedWeapon]: 'item-type-one-handed-weapon',
  [ItemType.TwoHandedWeapon]: 'item-type-two-handed-weapon',
  [ItemType.Polearm]: 'item-type-polearm',
  [ItemType.Thrown]: 'item-type-throwing-weapon',
  [ItemType.Arrows]: 'item-type-arrow',
  [ItemType.Bolts]: 'item-type-bolt',
  [ItemType.Banner]: 'item-type-banner',

  [ItemType.Pistol]: '',
  [ItemType.Musket]: '',
  [ItemType.Bullets]: '',
};

export const weaponClassToIcon: Record<WeaponClass, string> = {
  [WeaponClass.Undefined]: '',
  [WeaponClass.Dagger]: 'weapon-class-one-handed-dagger',
  [WeaponClass.OneHandedSword]: 'weapon-class-one-handed-sword',
  [WeaponClass.TwoHandedSword]: 'weapon-class-two-handed-sword',
  [WeaponClass.OneHandedAxe]: 'weapon-class-one-handed-axe',
  [WeaponClass.TwoHandedAxe]: 'weapon-class-two-handed-axe',
  [WeaponClass.Mace]: 'weapon-class-one-handed-mace',
  [WeaponClass.Pick]: '',
  [WeaponClass.TwoHandedMace]: 'weapon-class-two-handed-mace',
  [WeaponClass.OneHandedPolearm]: 'weapon-class-one-handed-polearm',
  [WeaponClass.TwoHandedPolearm]: 'weapon-class-two-handed-polearm',
  [WeaponClass.LowGripPolearm]: '',
  [WeaponClass.Arrow]: '',
  [WeaponClass.Bolt]: '',
  [WeaponClass.Cartridge]: '',
  [WeaponClass.Bow]: '',
  [WeaponClass.Crossbow]: '',
  [WeaponClass.Stone]: 'weapon-class-throwing-stone',
  [WeaponClass.Boulder]: '',
  [WeaponClass.ThrowingAxe]: 'weapon-class-throwing-axe',
  [WeaponClass.ThrowingKnife]: 'weapon-class-throwing-knife',
  [WeaponClass.Javelin]: 'weapon-class-throwing-spear',
  [WeaponClass.Pistol]: '',
  [WeaponClass.Musket]: '',
  [WeaponClass.SmallShield]: 'weapon-class-shield-small',
  [WeaponClass.LargeShield]: 'weapon-class-shield-large',
  [WeaponClass.Banner]: '',
};

export const itemFlagsToIcon: Record<ItemFlags, string | null> = {
  [ItemFlags.ForceAttachOffHandPrimaryItemBone]: null,
  [ItemFlags.ForceAttachOffHandSecondaryItemBone]: null,
  [ItemFlags.NotUsableByFemale]: null,
  [ItemFlags.NotUsableByMale]: null,
  [ItemFlags.DropOnWeaponChange]: 'item-flag-drop-on-change',
  [ItemFlags.DropOnAnyAction]: null,
  [ItemFlags.CannotBePickedUp]: null,
  [ItemFlags.CanBePickedUpFromCorpse]: null,
  [ItemFlags.QuickFadeOut]: null,
  [ItemFlags.WoodenAttack]: null,
  [ItemFlags.WoodenParry]: null,
  [ItemFlags.HeldInOffHand]: null,
  [ItemFlags.HasToBeHeldUp]: null,
  [ItemFlags.UseTeamColor]: 'item-flag-use-team-color',
  [ItemFlags.Civilian]: null,
  [ItemFlags.DoNotScaleBodyAccordingToWeaponLength]: null,
  [ItemFlags.DoesNotHideChest]: null,
  [ItemFlags.NotStackable]: null,
};

export const weaponFlagsToIcon: Partial<Record<WeaponFlags, string | null>> = {
  [WeaponFlags.BonusAgainstShield]: 'item-flag-bonus-against-shield',
  [WeaponFlags.TwoHandIdleOnMount]: 'item-flag-two-hand-idle',
  [WeaponFlags.MultiplePenetration]: 'item-flag-multiply-penetration',
  [WeaponFlags.CanDismount]: 'item-flag-can-dismount',
  [WeaponFlags.CanKnockDown]: 'item-flag-can-knock-down',
  [WeaponFlags.AutoReload]: 'item-flag-auto-reload',
  [WeaponFlags.CanPenetrateShield]: 'item-flag-can-penetrate-shield',
  [WeaponFlags.CantReloadOnHorseback]: 'item-flag-cant-reload-on-horseback',
  [WeaponFlags.CanReloadOnHorseback]: 'item-flag-can-reload-on-horseback',
  [WeaponFlags.CantUseOnHorseback]: 'item-flag-cant-reload-on-horseback',
};

export const itemUsageToIcon: Partial<Record<ItemUsage, string | null>> = {
  [ItemUsage.LongBow]: 'item-flag-longbow',
  [ItemUsage.Bow]: 'item-flag-short-bow',
  [ItemUsage.Crossbow]: 'item-flag-heavy-crossbow',
  [ItemUsage.CrossbowLight]: 'item-flag-light-crossbow',
  [ItemUsage.PolearmBracing]: 'item-flag-brace',
  [ItemUsage.PolearmPike]: 'item-flag-pike',
  [ItemUsage.PolearmCouch]: 'item-flag-couch',
};

export const itemFamilyTypeToIcon: Record<ItemFamilyType, string | null> = {
  [ItemFamilyType.Undefined]: null,
  [ItemFamilyType.Horse]: 'mount-type-horse',
  [ItemFamilyType.Camel]: 'mount-type-camel',
};

export const damageTypeToIcon: Record<DamageType, string | null> = {
  [DamageType.Undefined]: null,
  [DamageType.Blunt]: 'damage-type-blunt',
  [DamageType.Cut]: 'damage-type-cut',
  [DamageType.Pierce]: 'damage-type-pierce',
};

export const itemCultureToIcon: Record<Culture, string | null> = {
  [Culture.Neutral]: 'culture-neutrals',
  [Culture.Aserai]: 'culture-aserai',
  [Culture.Battania]: 'culture-battania',
  [Culture.Empire]: 'culture-empire',
  [Culture.Khuzait]: 'culture-khuzait',
  [Culture.Looters]: 'culture-looters',
  [Culture.Sturgia]: 'culture-sturgia',
  [Culture.Vlandia]: 'culture-vlandia',
};

// TO MODEL:
export enum IconBucketType {
  Asset = 'Asset',
  Svg = 'Svg',
}

export interface IconedBucket {
  type: IconBucketType;
  name: string;
}

export interface HumanBucket {
  icon: IconedBucket | null;
  label: string;
}

const damageTypeFieldByDamageField: Record<
  keyof Pick<ItemFlat, 'damage' | 'thrustDamage' | 'swingDamage'>,
  keyof Pick<ItemFlat, 'thrustDamageType' | 'swingDamageType'>
> = {
  damage: 'thrustDamageType',
  thrustDamage: 'thrustDamageType',
  swingDamage: 'swingDamageType',
};

const createHumanBucket = (label: string, icon: IconedBucket | null): HumanBucket => ({
  label,
  icon,
});

const createIcon = (type: IconBucketType, name: string | null | undefined): IconedBucket | null =>
  name === null || name === undefined
    ? null
    : {
        type,
        name,
      };

export const humanizeBucket = (
  aggregationKey: keyof ItemFlat,
  bucket: any,
  item?: ItemFlat
): HumanBucket => {
  if (bucket === null || bucket === undefined) {
    return createHumanBucket(`${aggregationKey} - invalid bucket`, null);
  }

  const format = aggregationsConfig[aggregationKey]?.format;

  if (aggregationKey === 'type') {
    return createHumanBucket(
      t(`item.type.${bucket as ItemType}`),
      createIcon(IconBucketType.Svg, itemTypeToIcon[bucket as ItemType])
    );
  }

  if (aggregationKey === 'weaponClass') {
    return createHumanBucket(
      t(`item.weaponClass.${bucket as WeaponClass}`),
      createIcon(IconBucketType.Svg, weaponClassToIcon[bucket as WeaponClass])
    );
  }

  if (aggregationKey === 'damageType') {
    return createHumanBucket(
      t(`item.damageType.${bucket}.long`),
      createIcon(IconBucketType.Svg, damageTypeToIcon[bucket as DamageType])
    );
  }

  if (aggregationKey === 'culture') {
    return createHumanBucket(
      t(`item.culture.${bucket as Culture}`),
      createIcon(IconBucketType.Asset, itemCultureToIcon[bucket as Culture])
    );
  }

  if (['mountArmorFamilyType', 'mountFamilyType'].includes(aggregationKey)) {
    return createHumanBucket(
      t(`item.familyType.${bucket as ItemFamilyType}`),
      createIcon(IconBucketType.Svg, itemFamilyTypeToIcon[bucket as ItemFamilyType])
    );
  }

  if (aggregationKey === 'flags') {
    if (Object.values(ItemFlags).includes(bucket as ItemFlags)) {
      return createHumanBucket(
        t(`item.flags.${bucket as ItemFlags}`),
        createIcon(IconBucketType.Svg, itemFlagsToIcon[bucket as ItemFlags])
      );
    }

    if (Object.values(WeaponFlags).includes(bucket as WeaponFlags)) {
      return createHumanBucket(
        t(`item.weaponFlags.${bucket as WeaponFlags}`),
        createIcon(IconBucketType.Svg, weaponFlagsToIcon[bucket as WeaponFlags])
      );
    }

    if (Object.values(ItemUsage).includes(bucket as ItemUsage)) {
      return createHumanBucket(
        t(`item.usage.${bucket as ItemUsage}`),
        createIcon(IconBucketType.Svg, itemUsageToIcon[bucket as ItemUsage])
      );
    }
  }

  if (format === ItemFieldFormat.Damage && item !== undefined) {
    const damageTypeField =
      damageTypeFieldByDamageField[
        aggregationKey as keyof Pick<ItemFlat, 'damage' | 'thrustDamage' | 'swingDamage'>
      ];

    const damageType = item[damageTypeField];

    if (damageType === null || damageType === undefined)
      return createHumanBucket(String(bucket), null);

    return createHumanBucket(
      t('item.damageTypeFormat', {
        value: bucket,
        type: t(`item.damageType.${damageType as DamageType}.short`),
      }),
      null
    );
  }

  if (format === ItemFieldFormat.Requirement) {
    return createHumanBucket(
      t('item.requirementFormat', {
        value: bucket,
      }),
      null
    );
  }

  if (format === ItemFieldFormat.Number) {
    return createHumanBucket(n(bucket as number), null);
  }

  return createHumanBucket(String(bucket), null);
};

export const getCompareItemsResult = (items: ItemFlat[], aggregationsConfig: AggregationConfig) => {
  return (Object.keys(aggregationsConfig) as Array<keyof ItemFlat>)
    .filter(k => aggregationsConfig[k]?.compareRule !== undefined)
    .reduce((out, k) => {
      const values = items.map(fi => fi[k] as number);
      out[k] =
        aggregationsConfig[k]!.compareRule === ItemFieldCompareRule.Less
          ? Math.min(...values)
          : Math.max(...values);
      return out;
    }, {} as CompareItemsResult);
};

export const getItemFieldDiffStr = (
  compareRule: ItemFieldCompareRule,
  value: number,
  bestValue: number
) => {
  if (value === bestValue) return '';

  if (compareRule === ItemFieldCompareRule.Less) {
    if (bestValue > value) return '';

    return `+${n(roundFLoat(Math.abs(value - bestValue)))}`;
  }

  if (bestValue < value) return '';

  return `-${n(roundFLoat(Math.abs(bestValue - value)))}`;
};

export const getItemGraceTimeEnd = (userItem: UserItem) => {
  const graceTimeEnd = new Date(userItem.createdAt);
  graceTimeEnd.setHours(graceTimeEnd.getHours() + 1); // TODO: to constants
  return graceTimeEnd;
};

export const isGraceTimeExpired = (itemGraceTimeEnd: Date) => {
  return itemGraceTimeEnd < new Date();
};

export const computeSalePrice = (userItem: UserItem) => {
  const graceTimeEnd = getItemGraceTimeEnd(userItem);

  if (isGraceTimeExpired(graceTimeEnd)) {
    return {
      price: Math.floor(applyPolynomialFunction(userItem.item.price, itemSellCostCoefs)),
      graceTimeEnd: null,
    };
  }

  // If the item was recently bought it is sold at 100% of its original price.
  return { price: userItem.item.price, graceTimeEnd };
};

export const computeAverageRepairCostPerHour = (price: number) =>
  Math.floor(price * itemRepairCostPerSecond * 3600 * itemBreakChance);

export const computeBrokenItemRepairCost = (price: number) =>
  Math.floor(price * itemRepairCostPerSecond * brokenItemRepairPenaltySeconds);

export const getRankColor = (rank: ItemRank) => {
  // TODO: colors

  switch (rank) {
    case 1:
      return '#1eff00';

    case 2:
      return '#0070dd';

    case 3:
      return '#a335ee';

    // TODO:
    default:
      return '';
  }
};
