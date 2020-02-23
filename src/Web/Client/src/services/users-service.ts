import User from '@/models/user';
import Character from '@/models/character';
import Item from '@/models/item';
import { get, post } from './crpg-client';

export function getUser(): Promise<User> {
  return get('/users/self');
}

export function getOwnedItems(): Promise<Item[]> {
  return get('/users/self/items');
}

export function buyItem(itemId: number) {
  return post('/users/self/items', { itemId });
}

export function getCharacters(): Promise<Character[]> {
  return get('/users/self/characters');
}
