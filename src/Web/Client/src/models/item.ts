import ItemType from '@/models/item-type';
import DamageType from '@/models/damage-type';
import WeaponFlags from '@/models/weapon-flags';

export default class Item {
  public id: number;
  public mbId: string;
  public name: string;
  public image: string;
  public value: number;
  public type: ItemType;
  public weight: number;

  // Armor
  public headArmor?: number;
  public bodyArmor?: number; // not null for shields
  public armArmor?: number;
  public legArmor?: number;

  // Horse
  public bodyLength?: number;
  public chargeDamage?: number;
  public maneuver?: number;
  public speed?: number;
  public hitPoints?: number;

  // Weapon
  public thrustDamageType?: DamageType;
  public swingDamageType?: DamageType;
  public accuracy?: number;
  public missileSpeed?: number;
  public stackAmount?: number;
  public weaponLength?: number;

  public primaryThrustDamage?: number;
  public primaryThrustSpeed?: number;
  public primarySwingDamage?: number;
  public primarySwingSpeed?: number;
  public primaryHandling?: number;
  public primaryWeaponFlags?: WeaponFlags;

  public secondaryThrustDamage?: number;
  public secondaryThrustSpeed?: number;
  public secondarySwingDamage?: number;
  public secondarySwingSpeed?: number;
  public secondaryHandling?: number;
  public secondaryWeaponFlags?: WeaponFlags;
}
