<template>
  <div class="container">
    <l-map
      class="map"
      :zoom="zoom"
      :center="center"
      :options="mapOptions"
      :max-bounds="maxBounds"
      @click="infoPos"
    >
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
  zoom = 8;
  center = latLng(-137, 131);
  url = 'http://pecores.fr/gigamap/{z}/{y}/{x}.png';
  attribution = 'TaleWorlds Entertainment';
  mapOptions = {
    zoomSnap: 0.5,
    minZoom: 3,
    maxZoom: 8,
    crs: CRS.Simple,
  };
  maxBounds = latLngBounds([
    [-40.6, 5.1],
    [-215.4, 250.8],
  ]);
  infoPOS = null;

  infoPos(event: any) {
    console.log(event.latlng);
  }
}
</script>

<style scoped lang="scss">
.map {
  height: 900px;
  width: 100%;
}
</style>
