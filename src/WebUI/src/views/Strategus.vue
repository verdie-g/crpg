<template>
  <section class="section">
    <l-map :zoom="zoom" :center="center" :options="mapOptions">
      <l-tile-layer :url="url" :attribution="attribution" />
    </l-map>
  </section>
</template>

<script lang="ts">
import { Vue, Component } from 'vue-property-decorator';
import { latLng } from 'leaflet';
import { CRS } from 'leaflet';
import { LMap, LTileLayer, LMarker, LPopup, LTooltip } from 'vue2-leaflet';
@Component({
  components: { LMap, LTileLayer, LMarker, LPopup, LTooltip },
})
export default class Strategus extends Vue {
  zoom = 8;
  center = latLng(-100, 100);
  url = 'http://pecores.fr/gigamap/{z}/{y}/{x}.png';
  attribution = 'Stategus';
  mapOptions = {
    zoomSnap: 0.5,
    maxZoom: 8,
    crs: CRS.Simple,
  };
}
</script>

<style scoped lang="scss">
.section {
  height: 1100px;
  width: 1300px;
}
</style>
