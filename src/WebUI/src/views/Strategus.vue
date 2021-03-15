<template>
  <div>
    <l-map
      class="map"
      :zoom="zoom"
      :center="center"
      :options="mapOptions"
      :max-bounds="maxBounds"
      @click="infoPos"
    >
      <l-tile-layer :url="url" :attribution="attribution" />
      <l-marker
        v-for="settlement in settlements"
        :lat-lng="[settlement.position.coordinates[0], settlement.position.coordinates[1]]"
        :key="settlement.id"
      ></l-marker>
    </l-map>
  </div>
</template>

<script lang="ts">
import { Vue, Component } from 'vue-property-decorator';
import { latLng, latLngBounds, CRS } from 'leaflet';
import { LMap, LTileLayer, LMarker, LPopup, LTooltip, LIcon } from 'vue2-leaflet';
import strategusModule from '@/store/strategus-module';
import Settlement from '@/models/settlement';

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
    minZoom: 2.5,
    maxZoom: 8,
    crs: CRS.Simple,
  };
  maxBounds = latLngBounds([
    [-40.6, 5.1],
    [-215.4, 250.8],
  ]);
  infoPOS = null;

  get settlements(): Settlement[] {
    return strategusModule.settlements;
  }

  created() {
    strategusModule.getSettlements();
  }

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
