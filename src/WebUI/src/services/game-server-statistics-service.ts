import { get } from '@/services/crpg-client';
import { GameServerStats } from '@/models/game-server-stats';

export function getGameServerStats(): Promise<GameServerStats> {
  return get(`/game-server-statistics`);
}
