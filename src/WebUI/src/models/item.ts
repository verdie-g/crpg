import ItemType from '@/models/item-type';
import ItemArmorComponent from '@/models/item-armor-component';
import ItemMountComponent from '@/models/item-mount-component';
import ItemWeaponComponent from '@/models/item-weapon-component';

export default class Item {
  public id: number;
  public templateMbId: string;
  public name: string;
  public value: number;
  public type: ItemType;
  public weight: number;
  public rank: number;

  public armor: ItemArmorComponent | null;
  public mount: ItemMountComponent | null;
  public weapons: ItemWeaponComponent[];
}
