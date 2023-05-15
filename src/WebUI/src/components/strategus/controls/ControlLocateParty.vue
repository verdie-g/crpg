<script setup lang="ts">
import { Map } from 'leaflet';
import { LControl } from '@vue-leaflet/vue-leaflet';
import { positionToLatLng } from '@/utils/geometry';
import { type Party } from '@/models/strategus/party';

const { party } = defineProps<{ party: Party }>();

const map = ref<Map | null>(null);

const onReady = (leafletObject: typeof LControl) => {
  map.value = leafletObject._map as Map;
};

const onClick = () => {
  map.value!.flyTo(positionToLatLng(party.position.coordinates));
};
</script>

<template>
  <LControl @ready="onReady">
    <div class="leaflet-bar">
      <a
        href="#"
        class="!flex items-center justify-center"
        :title="`Locate party TODO: i18n`"
        @click="onClick"
      >
        <OIcon icon="crosshair" size="lg" />
      </a>
    </div>
  </LControl>
</template>
