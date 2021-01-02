import WeaponFlags from '@/models/weapon-flags';
import DamageType from '@/models/damage-type';
import WeaponClass from '@/models/weapon-class';

export default class ItemWeaponComponent {
  public class: WeaponClass;
  public accuracy: number;
  public missileSpeed: number;
  public stackAmount: number;
  public length: number;
  public handling: number;
  public bodyArmor: number;
  public flags: WeaponFlags[];

  public thrustDamage: number;
  public thrustDamageType: DamageType;
  public thrustSpeed: number;

  public swingDamage: number;
  public swingDamageType: DamageType;
  public swingSpeed: number;
}
