// TODO: update SPEC!!!

import {
  DamageType,
  ItemFlat,
  ItemType,
  type ItemWeaponComponent,
  WeaponClass,
  WeaponFlags,
  type Item,
  WeaponUsage,
  ItemUsage,
} from '@/models/item';
import {
  itemTypeByWeaponClass,
  WeaponClassByItemUsage,
  visibleWeaponFlags,
  visibleItemFlags,
  visibleItemUsage,
  isLargeShield,
} from '@/services/item-service';
import { roundFLoat } from '@/utils/math';

const emptyWeapon = {
  weaponClass: null,
  weaponPrimaryClass: null,
  itemUsage: [],
  weaponFlags: [],
  accuracy: null,
  missileSpeed: null,
  stackAmount: null,
  length: null,
  handling: null,
  thrustDamage: null,
  thrustDamageType: null,
  thrustSpeed: null,
  swingDamage: null,
  swingDamageType: null,
  swingSpeed: null,
  shieldSpeed: null,
  shieldDurability: null,
  shieldArmor: null,
  reloadSpeed: null,
  aimSpeed: null,
  damage: null,
  damageType: null,
};

const mapWeaponProps = (item: Item) => {
  if (item.weapons.length === 0) {
    return emptyWeapon;
  }

  const [originalWeapon] = item.weapons;

  const weapon = {
    ...emptyWeapon,
    weaponClass: originalWeapon.class,
    weaponPrimaryClass: originalWeapon.class,
    itemUsage: [originalWeapon.itemUsage],
    weaponFlags: originalWeapon.flags,
    accuracy: originalWeapon.accuracy,
    missileSpeed: originalWeapon.missileSpeed,
    stackAmount: originalWeapon.stackAmount,
    length: originalWeapon.length,
    handling: originalWeapon.handling,

    thrustSpeed: originalWeapon.thrustSpeed,
    thrustDamage: originalWeapon.thrustDamage,
    thrustDamageType:
      originalWeapon.thrustDamageType === DamageType.Undefined || originalWeapon.thrustDamage === 0
        ? undefined
        : originalWeapon.thrustDamageType,

    swingSpeed: originalWeapon.swingSpeed,
    swingDamage: originalWeapon.swingDamage,
    swingDamageType:
      originalWeapon.swingDamageType === DamageType.Undefined || originalWeapon.swingDamage === 0
        ? undefined
        : originalWeapon.swingDamageType,
  };

  if (item.type === ItemType.Shield) {
    return {
      ...weapon,
      shieldSpeed: originalWeapon.swingSpeed,
      shieldDurability: originalWeapon.stackAmount,
      shieldArmor: originalWeapon.bodyArmor,
    };
  }

  if ([ItemType.Bow, ItemType.Crossbow].includes(item.type)) {
    // add custom flag
    if (
      [ItemType.Crossbow].includes(item.type) &&
      !originalWeapon.flags.includes(WeaponFlags.CantReloadOnHorseback)
    ) {
      weapon.weaponFlags.push(WeaponFlags.CanReloadOnHorseback);
    }

    return {
      ...weapon,
      reloadSpeed: originalWeapon.swingSpeed,
      aimSpeed: originalWeapon.thrustSpeed,
      damage: originalWeapon.thrustDamage,
    };
  }

  if ([ItemType.Bolts, ItemType.Arrows, ItemType.Thrown].includes(item.type)) {
    return {
      ...weapon,
      damage: originalWeapon.thrustDamage,
      damageType:
        originalWeapon.thrustDamageType === DamageType.Undefined
          ? undefined
          : originalWeapon.thrustDamageType,
    };
  }

  return weapon;
};

const mapArmorProps = (item: Item) => {
  if (item.armor === null) {
    return {
      headArmor: null,
      bodyArmor: null,
      armArmor: null,
      legArmor: null,
      materialType: null,
      mountArmor: null,
      mountArmorFamilyType: null,
    };
  }

  if (item.type === ItemType.MountHarness) {
    return {
      ...item.armor,
      mountArmor: item.armor.bodyArmor,
      mountArmorFamilyType: item.armor.familyType,
    };
  }

  return {
    ...item.armor,
    mountArmor: null,
    mountArmorFamilyType: null,
  };
};

const mapWeight = (item: Item) => {
  if ([ItemType.Thrown, ItemType.Bolts, ItemType.Arrows].includes(item.type)) {
    const [weapon] = item.weapons;

    return {
      weight: null,
      stackWeight: roundFLoat(item.weight * weapon.stackAmount),
    };
  }

  return {
    weight: roundFLoat(item.weight),
    stackWeight: null,
  };
};

const mapMountProps = (item: Item) => {
  if (item.mount === null) {
    return {
      bodyLength: null,
      chargeDamage: null,
      maneuver: null,
      speed: null,
      hitPoints: null,
      mountFamilyType: null,
    };
  }

  return {
    ...item.mount,
    mountFamilyType: item.mount.familyType,
  };
};

const itemToFlat = (item: Item): ItemFlat => {
  const weaponProps = mapWeaponProps(item);

  const flags = [
    ...item.flags.filter(flag => visibleItemFlags.includes(flag)),
    ...weaponProps.weaponFlags.filter(wf => visibleWeaponFlags.includes(wf)),
    ...weaponProps.itemUsage.filter(iu => visibleItemUsage.includes(iu)),
  ];

  if (isLargeShield(item.id)) {
    flags.push(WeaponFlags.CantUsageOnHorseback);
  }

  return {
    id: item.id,
    modId: generateModId(item, weaponProps?.weaponClass ?? undefined),
    name: item.name,
    price: item.price,
    type: item.type,
    culture: item.culture,
    requirement: item.requirement,
    tier: roundFLoat(item.tier),
    weaponUsage: [WeaponUsage.Primary],
    flags,
    ...mapWeight(item),
    ...mapArmorProps(item),
    ...mapMountProps(item),
    ...weaponProps,
  };
};

const generateModId = (item: Item, weaponClass?: WeaponClass) => {
  return `${item.id}_${item.type}${weaponClass !== undefined ? `_${weaponClass}` : ''}`;
};

const normalizeWeaponClass = (itemType: ItemType, weapon: ItemWeaponComponent) => {
  if (weapon.itemUsage in WeaponClassByItemUsage) {
    return WeaponClassByItemUsage[weapon.itemUsage]!;
  }

  return weapon.class;
};

const checkWeaponIsPrimaryUsage = (
  itemType: ItemType,
  weapon: ItemWeaponComponent,
  weapons: ItemWeaponComponent[]
) => {
  let isPrimaryUsage = false;

  const weaponClass = normalizeWeaponClass(itemType, weapon);

  if (itemType === ItemType.Polearm) {
    const hasCouch = weapons.some(w => w.itemUsage === ItemUsage.PolearmCouch);
    // const hasBrace = weapons.some(w => w.itemUsage === ItemUsage.PolearmBracing);
    // const hasPike = weapons.some(w => w.itemUsage === ItemUsage.PolearmPike);

    // console.log({itemType, 'weapon.class': weapon.class, weaponClass, hasCouch });

    if (hasCouch && weapon.class !== WeaponClass.OneHandedPolearm) {
      return false;
    }
  }

  isPrimaryUsage = itemType === itemTypeByWeaponClass[weaponClass];

  return isPrimaryUsage;
};

const getPrimaryWeaponClass = (item: Item) => {
  const primaryWeapon = item.weapons.find(w =>
    checkWeaponIsPrimaryUsage(item.type, w, item.weapons)
  );

  if (primaryWeapon !== undefined) {
    return primaryWeapon.class;
  }

  return null;
};

// TODO: FIXME: SPEC cloneMultipleUsageWeapon param
export const createItemIndex = (items: Item[], cloneMultipleUsageWeapon = false): ItemFlat[] => {
  const result = items.reduce((out, item) => {
    if (item.weapons.length > 1) {
      item.weapons.forEach((w, idx) => {
        let weaponClass = normalizeWeaponClass(item.type, w);

        const isPrimaryUsage = checkWeaponIsPrimaryUsage(item.type, w, item.weapons);

        // fixes a duplicate class, ex. Hoe: 1h/2h/1h
        const itemTypeAlreadyExistIdx = out.findIndex(fi => {
          // console.table({
          //   fiType: fi.type,
          //   itemType: item.type,
          //   fiModId: fi.modId,
          //   itemId: generateModId(item, w.class),
          //   exist:
          //     fi.modId ===
          //     generateModId({ ...item, type: itemTypeByWeaponClass[w.class] }, w.class),
          // });

          return (
            fi.modId ===
            generateModId({ ...item, type: itemTypeByWeaponClass[weaponClass] }, weaponClass)
          );
        });

        // console.table({
        //   idx,
        //   out: JSON.stringify(out.map(dd => ({ class: dd.weaponClass, type: dd.type }))),
        //   type: item.type,
        //   class: w.class,
        //   itemTypeByWeaponClass: itemTypeByWeaponClass[w.class],
        //   itemTypeAlreadyExistIdx,
        //   isPrimaryUsage,
        //   modId: generateModId(item, w.class),
        // });

        // merge itemUsage, if the weapon has several of the same class
        if (itemTypeAlreadyExistIdx != -1) {
          if (visibleItemUsage.includes(w.itemUsage)) {
            out[itemTypeAlreadyExistIdx].flags.push(w.itemUsage);
          }

          return;
        }

        if (isPrimaryUsage || cloneMultipleUsageWeapon) {
          out.push({
            ...itemToFlat({
              ...item,
              type: itemTypeByWeaponClass[weaponClass],
              weapons: [{ ...w, class: weaponClass }], // TODO:
            }),
            weaponUsage: [isPrimaryUsage ? WeaponUsage.Primary : WeaponUsage.Secondary],
            weaponPrimaryClass: isPrimaryUsage ? weaponClass : getPrimaryWeaponClass(item),
          });
        }
      });
    }
    //
    else {
      out.push(itemToFlat(item));
    }

    return out;
  }, [] as ItemFlat[]);

  // console.log(result);

  return result;
};
