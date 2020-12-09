import User from '@/models/user';
import Character from '@/models/character';
import Item from '@/models/item';
import { UpdateCharacterRequest } from '@/models/update-character-request';
import CharacterStatistics from '@/models/character-statistics';
import StatisticConversion from '@/models/statistic-conversion';
import Ban from '@/models/ban';
import EquippedItem from '@/models/equipped-item';
import EquippedItemId from '@/models/equipped-item-id';
import {
  get, post, put, del,
} from './crpg-client';

export function getUser(): Promise<User> {
  return get('/users/self');
}

export function deleteUser(): Promise<void> {
  return del('/users/self');
}

export function getOwnedItems(): Promise<Item[]> {
  return get('/users/self/items');
}

export function updateCharacter(characterId: number, req: UpdateCharacterRequest) {
  return put(`/users/self/characters/${characterId}`, req);
}

export function retireCharacter(characterId: number): Promise<Character> {
  return put(`/users/self/characters/${characterId}/retire`);
}

export function respecializeCharacter(characterId: number): Promise<Character> {
  return put(`/users/self/characters/${characterId}/respecialize`);
}

export function deleteCharacter(characterId: number): Promise<void> {
  return del(`/users/self/characters/${characterId}`);
}

export function updateCharacterItems(characterId: number, items: EquippedItemId[]): Promise<EquippedItem[]> {
  return put(`/users/self/characters/${characterId}/items`, { items });
}

export function switchCharacterAutoRepair(characterId: number, autoRepair: boolean): Promise<void> {
  return put(`/users/self/characters/${characterId}/auto-repair`, { autoRepair });
}

export function updateCharacterStats(characterId: number, req: CharacterStatistics): Promise<CharacterStatistics> {
  return put(`/users/self/characters/${characterId}/statistics`, req);
}

export function convertCharacterStats(characterId: number, conversion: StatisticConversion): Promise<CharacterStatistics> {
  return put(`/users/self/characters/${characterId}/statistics/convert`, { conversion });
}

export function buyItem(itemId: number): Promise<Item> {
  return post('/users/self/items', { itemId });
}

export function upgradeItem(itemId: number): Promise<Item> {
  return put(`/users/self/items/${itemId}/upgrade`);
}

export function getCharacters(): Promise<Character[]> {
  return get('/users/self/characters');
}

export async function getUserBans(): Promise<Ban[]> {
  const bans: Ban[] = await get('/users/self/bans');
  return bans.map(b => ({ ...b, createdAt: new Date(b.createdAt) }));
}
