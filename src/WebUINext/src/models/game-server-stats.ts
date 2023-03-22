import { Region } from './region';

interface GameStats {
  playingCount: number;
}

export interface GameServerStats {
  total: GameStats;
  regions: Partial<Record<Region, GameStats>>;
}
