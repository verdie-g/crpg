import Equipment from '@/models/equipment';

export default class Character {
  public id: number;

  public name: string;

  public experience: number;

  public level: number;

  public headEquipment: Equipment;

  public bodyEquipment: Equipment;

  public legsEquipment: Equipment;

  public glovesEquipment: Equipment;

  public weaponEquipment: Equipment;
}
