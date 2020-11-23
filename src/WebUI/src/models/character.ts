import CharacterItems from '@/models/character-items';
import CharacterStatistics from '@/models/character-statistics';
import CharacterGender from '@/models/character-gender';

export default class Character {
  public id: number;
  public name: string;
  public generation: number;
  public level: number;
  public experience: number;
  public nextLevelExperience: number;
  public bodyProperties: string;
  public gender: CharacterGender;
  public statistics: CharacterStatistics;
  public items: CharacterItems;
}
