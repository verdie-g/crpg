import { LatLngBounds } from 'leaflet';
import { getSettlements } from '@/services/strategus-service';
import { shouldDisplaySettlement } from '@/services/strategus-service/map';

export const useSettlements = (mapBounds: Ref<LatLngBounds | null>, zoom: Ref<number>) => {
  const { state: settlements, execute: loadSettlements } = useAsyncState(
    () => getSettlements(),
    [],
    {
      immediate: false,
    }
  );

  const visibleSettlements = computed(() => {
    if (mapBounds.value === null) {
      return [];
    }

    // console.log('mapBounds.value', mapBounds.value);

    return settlements.value.filter(s => shouldDisplaySettlement(s, mapBounds.value!, zoom.value));
  });

  return {
    visibleSettlements,
    loadSettlements,
  };
};
