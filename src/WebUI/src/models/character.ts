import CharacterStatistics from '@/models/character-statistics';
import EquippedItem from '@/models/equipped-item';

export default interface Character {
  id: number;
  name: string;
  generation: number;
  level: number;
  experience: number;
  autoRepair: boolean;
  statistics: CharacterStatistics;
  equippedItems: EquippedItem[];
}
