import Region from '@/models/region';

export interface GameServerStats {
  total: GameStats;
  regions: Record<Region, GameStats>;
}

export interface GameStats {
  playingCount: number;
}
