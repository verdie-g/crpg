<template>
  <div class="main-strategus">
    <l-map
      ref="map"
      class="map"
      :zoom="zoom"
      :center="center"
      :options="mapOptions"
      :max-bounds="maxBounds"
      @moveend="displayedBounds = map.mapObject.getBounds()"
      @leaflet:load="displayedBounds = map.mapObject.getBounds()"
    >
      <l-control-mouse-position />
      <l-tile-layer :url="url" :attribution="attribution" />
      <settlement v-for="settlement in settlements" :key="settlement.id" :settlement="settlement" />
    </l-map>
  </div>
</template>

<script lang="ts">
import { Vue, Component } from 'vue-property-decorator';
import { LatLng, LatLngBounds, CRS } from 'leaflet';
import { LMap, LTileLayer } from 'vue2-leaflet';
import LControlMousePosition from '@/components/strategus/LControlMousePosition.vue';
import Settlement from '@/models/settlement';
import SettlementType from '@/models/settlement-type';
import strategusModule from '@/store/strategus-module';
import SettlementComponent from '@/components/SettlementComponent.vue';

@Component({
  components: { LMap, LTileLayer, LControlMousePosition, settlement: SettlementComponent },
})
export default class Strategus extends Vue {
  zoom = 6;
  center = new LatLng(-139, 122.75);
  url = 'http://pecores.fr/gigamap/{z}/{y}/{x}.png';
  attribution = '<a target="_blank" href="https://www.taleworlds.com">TaleWorlds Entertainment</a>';
  mapOptions = {
    zoomSnap: 0.5,
    minZoom: 3,
    maxZoom: 7,
    crs: CRS.Simple,
    maxBoundsViscosity: 0.8,
    inertiaDeceleration: 2000,
  };
  maxBounds = new LatLngBounds([
    [0, 0],
    [-214.88, 768],
  ]);
  displayedBounds: LatLngBounds | null = null;

  get settlements(): Settlement[] {
    if (
      this.displayedBounds === null ||
      this.map === undefined ||
      this.map.mapObject === undefined
    ) {
      return [];
    }
    // Keep settlement inside displayedBounds
    return strategusModule.settlements.filter(
      settlement =>
        this.displayedBounds!.contains(
          new LatLng(settlement.position.coordinates[1], settlement.position.coordinates[0])
        ) &&
        (this.map!.mapObject.getZoom() > 4 || settlement.type === SettlementType.Town)
    );
  }

  // get Map object
  get map(): LMap {
    return this.$refs.map as LMap;
  }

  created() {
    strategusModule.getSettlements();
  }
}
</script>

<style lang="scss">
// Hide vertical scrollbar
html {
  overflow-y: auto;
}
</style>

<style scoped lang="scss">
.main-strategus {
  .map {
    //calc(Screen height - navbar)
    height: calc(100vh - 4.25rem);
    background-color: #284745;
  }
}
</style>
