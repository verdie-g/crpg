import { CharacterClass, type CharacterRating } from '@/models/character';
import { UserPublic } from '@/models/user';

export interface CharacterCompetitive {
  id: number;
  level: number;
  class: CharacterClass;
  rating: CharacterRating;
  user: UserPublic;
}

export interface CharacterCompetitiveNumbered extends CharacterCompetitive {
  position: number;
}

export interface Rank {
  groupTitle: string;
  title: string;
  color: string;
  min: number;
  max: number;
}

export enum RankGroup {
  'Iron' = 'Iron',
  'Copper' = 'Copper',
  'Bronze' = 'Bronze',
  'Silver' = 'Silver',
  'Gold' = 'Gold',
  'Platinum' = 'Platinum',
  'Diamond' = 'Diamond',
  'Champion' = 'Champion',
}
