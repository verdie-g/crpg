import CharacterStatistics from '@/models/character-statistics';
import CharacterGender from '@/models/character-gender';
import EquippedItem from '@/models/equipped-item';

export default class Character {
  public id: number;
  public name: string;
  public generation: number;
  public level: number;
  public experience: number;
  public nextLevelExperience: number;
  public autoRepair: boolean;
  public bodyProperties: string;
  public gender: CharacterGender;
  public statistics: CharacterStatistics;
  public equippedItems: EquippedItem[];
}
