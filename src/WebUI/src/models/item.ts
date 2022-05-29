import ItemType from '@/models/item-type';
import ItemArmorComponent from '@/models/item-armor-component';
import ItemMountComponent from '@/models/item-mount-component';
import ItemWeaponComponent from '@/models/item-weapon-component';
import Culture from '@/models/culture';

export default interface Item {
  id: number;
  templateMbId: string;
  name: string;
  price: number;
  type: ItemType;
  culture: Culture;
  weight: number;
  rank: number;

  armor: ItemArmorComponent | null;
  mount: ItemMountComponent | null;
  weapons: ItemWeaponComponent[];
}
