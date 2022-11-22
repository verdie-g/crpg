import queryString from 'query-string';

import User from '@/models/user';
import UserPublic from '@/models/user-public';
import Character from '@/models/character';
import { UpdateCharacterRequest } from '@/models/update-character-request';
import CharacterCharacteristics from '@/models/character-characteristics';
import CharacterStatistics from '@/models/character-statistics';
import CharacteristicConversion from '@/models/characteristic-conversion';
import Restriction from '@/models/restriction';
import EquippedItem from '@/models/equipped-item';
import EquippedItemId from '@/models/equipped-item-id';
import { get, post, put, del } from './crpg-client';
import Clan from '@/models/clan';
import UserItem from '@/models/user-item';
import Platform from '@/models/platform';

export function getUserByUserId(id: number): Promise<UserPublic> {
  return get(`/users/${id}`);
}

interface UserSearcyQuery {
  platform?: Platform;
  platformUserId?: string;
  name?: string;
}

export function searchUser(payload: UserSearcyQuery): Promise<UserPublic[]> {
  const query = queryString.stringify(payload, { skipEmptyString: true, skipNull: true });
  return get(`/users?${query}`);
}

export function getUserRestrictions(id: number): Promise<Restriction[]> {
  return get(`/users/${id}/restrictions`);
}

export function getUser(): Promise<User> {
  return get('/users/self');
}

export function deleteUser(): Promise<void> {
  return del('/users/self');
}

export function getUserItems(): Promise<UserItem[]> {
  return get('/users/self/items');
}

export function getUserClan(): Promise<Clan | null> {
  return get('/users/self/clans');
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

export function skipTheFunCharacter(characterId: number): Promise<Character> {
  return put(`/users/self/characters/${characterId}/skip-the-fun`);
}

export function deleteCharacter(characterId: number): Promise<void> {
  return del(`/users/self/characters/${characterId}`);
}

export function getCharacterItems(characterId: number): Promise<EquippedItem[]> {
  return get(`/users/self/characters/${characterId}/items`);
}

export function updateCharacterItems(
  characterId: number,
  items: EquippedItemId[]
): Promise<EquippedItem[]> {
  return put(`/users/self/characters/${characterId}/items`, { items });
}

export function switchCharacterAutoRepair(characterId: number, autoRepair: boolean): Promise<void> {
  return put(`/users/self/characters/${characterId}/auto-repair`, { autoRepair });
}

export function activateCharacter(characterId: number, active: boolean): Promise<void> {
  return put(`/users/self/characters/${characterId}/active`, { active });
}

export function getCharacterCharacteristics(
  characterId: number
): Promise<CharacterCharacteristics> {
  return get(`/users/self/characters/${characterId}/characteristics`);
}

export function getCharacterStatistics(characterId: number): Promise<CharacterStatistics> {
  return get(`/users/self/characters/${characterId}/statistics`);
}

export function updateCharacterCharacteristics(
  characterId: number,
  req: CharacterCharacteristics
): Promise<CharacterCharacteristics> {
  return put(`/users/self/characters/${characterId}/characteristics`, req);
}

export function convertCharacterCharacteristics(
  characterId: number,
  conversion: CharacteristicConversion
): Promise<CharacterCharacteristics> {
  return put(`/users/self/characters/${characterId}/characteristics/convert`, { conversion });
}

export function buyItem(itemId: string): Promise<UserItem> {
  return post('/users/self/items', { itemId });
}

export function upgradeUserItem(userItemId: number): Promise<UserItem> {
  return put(`/users/self/items/${userItemId}/upgrade`);
}

export function sellUserItem(userItemId: number): Promise<UserItem> {
  return del(`/users/self/items/${userItemId}`);
}

export function getCharacters(): Promise<Character[]> {
  return get('/users/self/characters');
}
