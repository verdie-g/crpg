import ItemType from '@/models/item-type';

export default class Item {
  public id: number;

  public name: string;

  public price: number;

  public type: ItemType;
}
