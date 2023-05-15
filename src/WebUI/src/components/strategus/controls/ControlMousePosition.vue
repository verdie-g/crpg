<script setup lang="ts">
import { type Map, type LatLng, type LeafletMouseEvent } from 'leaflet';
import { LControl } from '@vue-leaflet/vue-leaflet';

const map = ref<Map | null>(null);
const mousePosition = ref<LatLng | null>(null);

const onReady = (leafletObject: typeof LControl) => {
  map.value = leafletObject._map as Map;
  map.value.on('mousemove', onMouseMove);
};

onBeforeUnmount(() => {
  map.value!.off('mousemove', onMouseMove);
});

const onMouseMove = (e: LeafletMouseEvent) => {
  mousePosition.value = e.latlng;
};

const formatNumber = (n: number): string => {
  // TODO: SPEC
  const whole = Math.trunc(n);
  const decimal = Math.trunc(Math.abs(n % 1) * 1000);

  const wholeStr = (whole < 0 ? '-' : '+') + Math.abs(whole).toString().padStart(3, '0');
  const decimalStr = decimal.toString().padStart(3, '0');
  return wholeStr + '.' + decimalStr;
};

const mousePositionText = computed(() => {
  if (mousePosition.value === null) {
    return '';
  }

  return formatNumber(mousePosition.value.lat) + ' ' + formatNumber(mousePosition.value.lng);
});
</script>

<template>
  <LControl @ready="onReady">
    <div v-if="mousePosition !== null" class="rounded-xl bg-base-200 p-2 text-white">
      {{ mousePositionText }}
    </div>
  </LControl>
</template>
