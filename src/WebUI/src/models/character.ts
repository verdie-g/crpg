import CharacterItems from '@/models/character-items';
import CharacterStatistics from '@/models/character-statistics';

export default class Character {
  public id: number;
  public name: string;
  public experience: number;
  public nextLevelExperience: number;
  public level: number;
  public statistics: CharacterStatistics;
  public items: CharacterItems;
}
