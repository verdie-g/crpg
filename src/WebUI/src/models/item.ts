import ItemType from '@/models/item-type';
import ItemArmorComponent from '@/models/item-armor-component';
import ItemHorseComponent from '@/models/item-horse-component';
import ItemWeaponComponent from '@/models/item-weapon-component';

export default class Item {
  public id: number;
  public mbId: string;
  public name: string;
  public value: number;
  public type: ItemType;
  public weight: number;
  public rank: number;

  public armor: ItemArmorComponent | null;
  public horse: ItemHorseComponent | null;
  public weapons: ItemWeaponComponent[];
}
