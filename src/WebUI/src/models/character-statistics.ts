import CharacterAttributes from '@/models/character-attributes';
import CharacterSkills from '@/models/character-skills';
import CharacterWeaponProficiencies from '@/models/character-weapon-proficiencies';

export default class CharacterStatistics {
  public attributes: CharacterAttributes;
  public skills: CharacterSkills;
  public weaponProficiencies: CharacterWeaponProficiencies;
}
