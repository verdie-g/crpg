import CharacterItems from '@/models/character-items';

export default class Character {
  public id: number;
  public name: string;
  public experience: number;
  public nextLevelExperience: number;
  public level: number;
  public items: CharacterItems;
}
