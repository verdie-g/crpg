<script setup lang="ts">
import { LMarker, LIcon } from '@vue-leaflet/vue-leaflet';
import { SettlementType, type SettlementPublic } from '@/models/strategus/settlement';
import { positionToLatLng } from '@/utils/geometry';

const { settlement } = defineProps<{ settlement: SettlementPublic }>();

const settlementMarkerStyle = computed(() => {
  switch (settlement.type) {
    case SettlementType.Town:
      return {
        baseClass: 'text-sm px-2 py-1.5 gap-2',
        icon: 'town',
        title: 'Town',
        iconSize: 'lg',
      };
    case SettlementType.Castle:
      return {
        baseClass: 'text-xs px-1.5 py-1 gap-1.5',
        icon: 'castle',
        title: 'Castle',
        iconSize: 'sm',
      };
    case SettlementType.Village:
    default:
      return {
        baseClass: 'text-2xs px-1 py-1 gap-1',
        icon: 'village',
        title: 'Village',
        iconSize: 'sm',
      };
  }
});
</script>

<template>
  <LMarker
    :latLng="positionToLatLng(settlement.position.coordinates)"
    :options="{ bubblingMouseEvents: false }"
  >
    <LIcon className="!flex justify-center items-center">
      <div
        class="flex items-center whitespace-nowrap rounded-md bg-base-100 bg-opacity-30 text-white hover:ring"
        :class="settlementMarkerStyle.baseClass"
        :title="settlementMarkerStyle.title"
      >
        <OIcon :icon="settlementMarkerStyle.icon" :size="settlementMarkerStyle.iconSize" />
        <span>{{ settlement.name }}</span>
      </div>
    </LIcon>
  </LMarker>
</template>
