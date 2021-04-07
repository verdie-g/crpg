<template>
  <div class="strategus-main">
    <l-map
      ref="map"
      class="map"
      :zoom.sync="zoom"
      :center="center"
      :options="mapOptions"
      :max-bounds="maxBounds"
      @ready="onMapBoundsChange"
      @moveend="onMapBoundsChange"
    >
      <l-control-mouse-position />
      <l-control class="column is-one-third" position="topleft">
        <component
          class="box"
          v-if="currentDialog"
          :is="currentDialog"
          v-on="dialogEventHandlers"
        />
      </l-control>
      <l-tile-layer :url="url" :attribution="attribution" />
      <settlement
        v-for="settlement in settlements"
        :key="'settlement-' + settlement.id"
        :settlement="settlement"
        :map="map"
      />
      <hero v-if="hero" :hero="hero" :self="true" />
      <hero
        v-for="vh in visibleHeroes"
        :key="'hero-' + vh.id"
        :hero="vh"
        :self="false"
        :map="map"
      />
    </l-map>
  </div>
</template>

<script lang="ts">
import { Vue, Component } from 'vue-property-decorator';
import { LatLng, LatLngBounds, CRS } from 'leaflet';
import { LMap, LTileLayer, LCircleMarker, LControl } from 'vue2-leaflet';
import LControlMousePosition from '@/components/strategus/LControlMousePosition.vue';
import Settlement from '@/models/settlement-public';
import SettlementType from '@/models/settlement-type';
import strategusModule from '@/store/strategus-module';
import SettlementComponent from '@/components/strategus/SettlementComponent.vue';
import RegistrationDialog from '@/components/strategus/RegistrationDialog.vue';
import Constants from '../../../../data/constants.json';
import Hero from '@/models/hero';
import HeroComponent from '@/components/strategus/HeroComponent.vue';
import { pointToLatLng } from '@/utils/geometry';
import HeroVisible from '@/models/hero-visible';

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
    LControl,
    ...dialogs,
    settlement: SettlementComponent,
    hero: HeroComponent,
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
  updateIntervalId = -1;

  // Register here handlers for all events that can emitted from a dialog.
  dialogEventHandlers = {
    heroSpawn: this.heroSpawn,
  };

  get settlements(): Settlement[] {
    if (this.mapBounds === null) {
      return [];
    }

    return strategusModule.settlements.filter(this.shouldDisplaySettlement);
  }

  get hero(): Hero | null {
    return strategusModule.hero;
  }

  get visibleHeroes(): HeroVisible[] {
    return strategusModule.visibleHeroes;
  }

  get map(): LMap {
    return this.$refs.map as LMap;
  }

  get currentDialog(): string | null {
    return strategusModule.currentDialog;
  }

  created() {
    strategusModule.getSettlements();
    strategusModule.getUpdate().then(res => {
      if (res.errors !== null) {
        // Not registered to strategus.
        strategusModule.pushDialog('RegistrationDialog');
      } else {
        this.heroSpawn();
      }
    });
  }

  beforeDestroy() {
    if (this.updateIntervalId !== -1) {
      clearInterval(this.updateIntervalId);
    }
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

  heroSpawn() {
    strategusModule.getUpdate();
    this.updateIntervalId = setInterval(() => strategusModule.getUpdate(), 60 * 1000);
    this.map.mapObject.flyTo(pointToLatLng(this.hero!.position), 5, { duration: 0.4 });
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
  .map {
    // calc(Screen height - navbar)
    height: calc(100vh - 4.25rem);
    background-color: #284745;
  }
}
</style>
