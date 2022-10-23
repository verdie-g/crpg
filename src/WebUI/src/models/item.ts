import ItemType from '@/models/item-type';
import ItemArmorComponent from '@/models/item-armor-component';
import ItemMountComponent from '@/models/item-mount-component';
import ItemWeaponComponent from '@/models/item-weapon-component';
import Culture from '@/models/culture';
import ItemFlags from '@/models/item-flags';

export default interface Item {
  id: string;
  name: string;
  price: number;
  type: ItemType;
  culture: Culture;
  weight: number;
  requirement: number;
  tier: number;
  flags: ItemFlags[];

  armor: ItemArmorComponent | null;
  mount: ItemMountComponent | null;
  weapons: ItemWeaponComponent[];
}
