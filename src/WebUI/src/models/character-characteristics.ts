import CharacterAttributes from '@/models/character-attributes';
import CharacterSkills from '@/models/character-skills';
import CharacterWeaponProficiencies from '@/models/character-weapon-proficiencies';

export default interface CharacterCharacteristics {
  attributes: CharacterAttributes;
  skills: CharacterSkills;
  weaponProficiencies: CharacterWeaponProficiencies;
}
