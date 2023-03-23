import { getGameServerStats } from '@/services/game-server-statistics-service';
import { Region } from '@/models/region';

export const useGameServerStats = () => {
  let interval: number;

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

  onMounted(() => {
    // @ts-ignore
    interval = setInterval(loadGameServerStats, 10000);
  });

  onBeforeUnmount(() => {
    clearInterval(interval);
  });

  return {
    gameServerStats,
    loadGameServerStats,
  };
};
