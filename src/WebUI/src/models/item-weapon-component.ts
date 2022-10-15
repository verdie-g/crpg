import WeaponFlags from '@/models/weapon-flags';
import DamageType from '@/models/damage-type';
import WeaponClass from '@/models/weapon-class';

export default interface ItemWeaponComponent {
  class: WeaponClass;
  itemUsage: string;
  accuracy: number;
  missileSpeed: number;
  stackAmount: number;
  length: number;
  handling: number;
  bodyArmor: number;
  flags: WeaponFlags[];

  thrustDamage: number;
  thrustDamageType: DamageType;
  thrustSpeed: number;

  swingDamage: number;
  swingDamageType: DamageType;
  swingSpeed: number;
}
