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
  attribution = '<a target="_blank" href="https://www.taleworlds.com">TaleWorlds Entertainment</a>';
  mapOptions = {
    zoomSnap: 0.5,
    minZoom: 2,
    maxZoom: 7,
    crs: CRS.Simple,
    maxBoundsViscosity: 0.8,
    inertiaDeceleration: 2000,
  };
  maxBounds = latLngBounds([
    [0, 0],
    [-214.88, 768],
  ]);
}
</script>

<style lang="scss">
// Hide vertical scrollbar
html {
  overflow-y: auto;
}
</style>

<style scoped lang="scss">
.mainStrategus {
  .map {
    //calc(Screen height - navbar)
    height: calc(100vh - 4.25rem);
    background-color: #284745;
  }
}
</style>
