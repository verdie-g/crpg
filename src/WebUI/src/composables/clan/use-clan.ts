import { useAsyncState } from '@vueuse/core';
import { getClan } from '@/services/clan-service';

export const useClan = (id: string) => {
  const clanId = computed(() => Number(id));

  const { state: clan, execute: loadClan } = useAsyncState(
    ({ id }: { id: number }) => getClan(id),
    null,
    {
      immediate: false,
    }
  );

  return {
    clanId,
    clan,
    loadClan,
  };
};
