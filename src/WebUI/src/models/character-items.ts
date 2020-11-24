import Item from '@/models/item';

export default class CharacterItems {
  public headItem: Item | null;
  public shoulderItem: Item | null;
  public bodyItem: Item | null;
  public handItem: Item | null;
  public legItem: Item | null;
  public mountItem: Item | null;
  public mountHarnessItem: Item | null;
  public weapon1Item: Item | null;
  public weapon2Item: Item | null;
  public weapon3Item: Item | null;
  public weapon4Item: Item | null;
  public autoRepair: boolean;
}
