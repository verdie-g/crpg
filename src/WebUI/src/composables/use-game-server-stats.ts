import { Region } from '@/models/region';
import { getGameServerStats } from '@/services/game-server-statistics-service';
import usePollInterval from '@/composables/use-poll-interval';

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

  const { subscribe, unsubscribe } = usePollInterval();
  const id = Symbol('loadGameServerStats');
  onMounted(() => {
    subscribe(id, loadGameServerStats);
  });
  onBeforeUnmount(() => {
    unsubscribe(id);
  });

  return {
    gameServerStats,
    loadGameServerStats,
  };
};
