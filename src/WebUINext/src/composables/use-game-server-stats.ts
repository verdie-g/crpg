import { getGameServerStats } from '@/services/game-server-statistics-service';
import { Region } from '@/models/region';

export const useGameServerStats = () => {
  const { state: gameServerStats, execute: loadGameServerStats } = useAsyncState(
    () => getGameServerStats(),
    {
      total: { playingCount: 0 },
      regions: {
        [Region.Eu]: {
          playingCount: 0,
        },
        [Region.Na]: {
          playingCount: 0,
        },
        [Region.As]: {
          playingCount: 0,
        },
        [Region.Oc]: {
          playingCount: 0,
        },
      },
    },
    {
      immediate: false,
    }
  );

  return {
    gameServerStats,
    loadGameServerStats,
  };
};
