<template>
  <div class="strategus-main">
    <div class="strategus-html-layer columns p-6">
      <div class="column is-one-third box strategus-dialog" v-if="currentDialog">
        <component :is="currentDialog" />
      </div>
    </div>

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
      <l-circle-marker
        v-if="hero"
        :lat-lng="[hero.position.coordinates[1], hero.position.coordinates[0]]"
        :radius="10"
        color="#f00"
      />
    </l-map>
  </div>
</template>

<script lang="ts">
import { Vue, Component } from 'vue-property-decorator';
import { LatLng, LatLngBounds, CRS } from 'leaflet';
import { LMap, LTileLayer, LCircleMarker } from 'vue2-leaflet';
import LControlMousePosition from '@/components/strategus/LControlMousePosition.vue';
import Settlement from '@/models/settlement-public';
import SettlementType from '@/models/settlement-type';
import strategusModule from '@/store/strategus-module';
import SettlementComponent from '@/components/strategus/SettlementComponent.vue';
import RegistrationDialog from '@/components/strategus/RegistrationDialog.vue';
import Constants from '../../../../data/constants.json';
import Hero from '@/models/hero';

// Register here all dialogs that can be used by the dynamic dialog component.
const dialogs = {
  RegistrationDialog,
};

@Component({
  components: {
    LMap,
    LTileLayer,
    LCircleMarker,
    LControlMousePosition,
    ...dialogs,
    settlement: SettlementComponent,
  },
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
  updateIntervalId: number;

  get settlements(): Settlement[] {
    if (this.mapBounds === null) {
      return [];
    }

    return strategusModule.settlements.filter(this.shouldDisplaySettlement);
  }

  get hero(): Hero | null {
    return strategusModule.hero;
  }

  get map(): LMap {
    return this.$refs.map as LMap;
  }

  get currentDialog(): string | null {
    return strategusModule.currentDialog;
  }

  created() {
    strategusModule.getSettlements();
    strategusModule.getUpdate();
    this.updateIntervalId = setInterval(() => strategusModule.getUpdate(), 60 * 1000);
  }

  beforeDestroy() {
    clearInterval(this.updateIntervalId);
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
.strategus-main {
  position: relative;

  .strategus-html-layer {
    position: absolute;
    z-index: 500; // To be over the map.
  }

  .strategus-dialog {
  }

  .map {
    // calc(Screen height - navbar)
    height: calc(100vh - 4.25rem);
    background-color: #284745;
  }
}
</style>
