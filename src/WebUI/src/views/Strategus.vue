<template>
  <div class="mainStrategus">
    <l-map class="map" :zoom="zoom" :center="center" :options="mapOptions" :max-bounds="maxBounds">
      <l-tile-layer :url="url" :attribution="attribution" />
    </l-map>
  </div>
</template>

<script lang="ts">
import { Vue, Component } from 'vue-property-decorator';
import { latLng, latLngBounds, CRS } from 'leaflet';
import { LMap, LTileLayer, LMarker, LPopup, LTooltip, LIcon } from 'vue2-leaflet';

@Component({
  components: { LMap, LTileLayer, LMarker, LPopup, LTooltip, LIcon },
})
export default class Strategus extends Vue {
  zoom = 6;
  center = latLng(-139, 122.75);
  url = 'http://pecores.fr/gigamap/{z}/{y}/{x}.png';
  attribution = 'TaleWorlds Entertainment';
  mapOptions = {
    zoomSnap: 0.5,
    minZoom: 3,
    maxZoom: 7,
    crs: CRS.Simple,
    maxBoundsViscosity: 0.8,
    inertiaDeceleration: 8000,
  };
  maxBounds = latLngBounds([
    [-40.6, 5.3],
    [-215.4, 250.8],
  ]);
}
</script>

<style lang="scss">
html {
  overflow-y: auto;
}
</style>

<style scoped lang="scss">
.mainStrategus {
  .map {
    height: calc(100vh - 3.25rem);
    background-color: #313131;
  }
}
</style>
