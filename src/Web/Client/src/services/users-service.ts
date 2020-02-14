import User from '@/models/user';
import Character from '@/models/character';
import { get } from './trpg-client';

export function getUser(): Promise<User> {
  return get('/users/self');
}

export function getCharacters(): Promise<Character[]> {
  return get('/users/self/characters');
}
