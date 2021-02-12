import CharacterStatistics from '@/models/character-statistics';
import EquippedItem from '@/models/equipped-item';

export default class Character {
  public id: number;
  public name: string;
  public generation: number;
  public level: number;
  public experience: number;
  public autoRepair: boolean;
  public statistics: CharacterStatistics;
  public equippedItems: EquippedItem[];
}
