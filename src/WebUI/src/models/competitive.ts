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
  title: string;
  color: string;
  min: number;
  max: number;
}
