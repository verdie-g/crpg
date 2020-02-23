import { get } from '@/services/crpg-client';
import Item from '@/models/item';

export function getItems(): Promise<Item[]> {
  return get('/items');
}
