import { get } from './crpg-client';
import Settlement from '@/models/settlement';

export function getSettlements(): Promise<Settlement> {
  return get('/strategus/settlements');
}
