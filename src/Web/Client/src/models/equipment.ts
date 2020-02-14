import EquipmentType from '@/models/equipment-type';

export default class Equipment {
  public id: number;

  public name: string;

  public price: number;

  public type: EquipmentType;
}
