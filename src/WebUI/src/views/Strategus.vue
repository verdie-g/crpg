<template>
  <div class="main-strategus">
    <l-map
      ref="map"
      class="map"
      :zoom.sync="zoom"
      :center="center"
      :options="mapOptions"
      :max-bounds="maxBounds"
      @leaflet:load="onMapBoundsChange"
      @moveend="onMapBoundsChange"
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
import Constants from '../../../../data/constants.json';

@Component({
  components: { LMap, LTileLayer, LControlMousePosition, settlement: SettlementComponent },
})
export default class Strategus extends Vue {
  zoom = 3;
  center = new LatLng(-100, 125);
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
    [-Constants.strategusMapHeight, Constants.strategusMapWidth * 3],
  ]);
  mapBounds: LatLngBounds | null = null;

  get settlements(): Settlement[] {
    if (this.mapBounds === null) {
      return [];
    }

    return strategusModule.settlements.filter(this.shouldDisplaySettlement);
  }

  get map(): LMap {
    return this.$refs.map as LMap;
  }

  created() {
    strategusModule.getSettlements();
  }

  onMapBoundsChange() {
    this.mapBounds = this.map.mapObject.getBounds();
  }

  shouldDisplaySettlement(settlement: Settlement): boolean {
    const [x, y] = settlement.position.coordinates;
    if (!this.mapBounds!.contains(new LatLng(y, x))) {
      return false;
    }

    return (
      this.zoom > 4 ||
      (this.zoom > 3 && settlement.type == SettlementType.Castle) ||
      settlement.type === SettlementType.Town
    );
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
