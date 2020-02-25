import Item from '@/models/item';

export default class Character {
  public id: number;

  public name: string;

  public experience: number;

  public nextLevelExperience: number;

  public level: number;

  public headItem: Item;

  public bodyItem: Item;

  public legItem: Item;

  public handItem: Item;

  public weapon1Item: Item;

  public weapon2Item: Item;

  public weapon3Item: Item;

  public weapon4Item: Item;
}
