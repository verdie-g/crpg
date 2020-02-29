import Item from '@/models/item';

export default class Character {
  public id: number;
  public name: string;
  public experience: number;
  public nextLevelExperience: number;
  public level: number;
  public headItem: Item | null;
  public capeItem: Item | null;
  public bodyItem: Item | null;
  public handItem: Item | null;
  public legItem: Item | null;
  public horseItem: Item | null;
  public horseHarnessItem: Item | null;
  public weapon1Item: Item | null;
  public weapon2Item: Item | null;
  public weapon3Item: Item | null;
  public weapon4Item: Item | null;
}
