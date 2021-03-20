<template>
  <div class="main-strategus">
    <l-map
      ref="map"
      class="map"
      :zoom="zoom"
      :center="center"
      :options="mapOptions"
      :max-bounds="maxBounds"
      @moveend="setDisplayedBounds(mapRef.mapObject.getBounds())"
      @leaflet:load="setDisplayedBounds(mapRef.mapObject.getBounds())"
    >
      <l-tile-layer :url="url" :attribution="attribution" />
      <l-marker
        v-for="settlement in settlements"
        :lat-lng="[settlement.position.coordinates[1], settlement.position.coordinates[0]]"
        :key="settlement.id"
      >
        <l-icon :icon-anchor="[0, 0]" class-name="settlement-icon">
          <div
            class="settlement-icon-txt"
            :style="`font-size:${getSettlementParams(settlement).fontSize}px`"
          >
            {{ settlement.name }}
          </div>
        </l-icon>
      </l-marker>
    </l-map>
  </div>
</template>

<script lang="ts">
import { Vue, Component } from 'vue-property-decorator';
import { LatLng, LatLngBounds, CRS, Icon } from 'leaflet';
import { LMap, LTileLayer, LMarker, LIcon } from 'vue2-leaflet';
import strategusModule from '@/store/strategus-module';
import Settlement from '@/models/settlement';
import SettlementType from '@/models/settlement-type';

//Default icons
delete (Icon.Default.prototype as any)._getIconUrl;
Icon.Default.mergeOptions({
  iconRetinaUrl: require('leaflet/dist/images/marker-icon-2x.png'),
  iconUrl: require('leaflet/dist/images/marker-icon.png'),
  shadowUrl: require('leaflet/dist/images/marker-shadow.png'),
});

@Component({
  components: { LMap, LTileLayer, LMarker, LIcon },
})
export default class Strategus extends Vue {
  zoom = 6;
  center = new LatLng(-139, 122.75);
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
  maxBounds = new LatLngBounds([
    [0, 0],
    [-214.88, 768],
  ]);
  displayedBounds: LatLngBounds | null = null;

  get settlements(): Settlement[] {
    if (this.displayedBounds === null) {
      return [];
    }
    // Keep settlement displayed and filter by zoom
    return strategusModule.settlements.filter(
      settlement =>
        this.displayedBounds !== null &&
        this.displayedBounds.contains(
          new LatLng(settlement.position.coordinates[1], settlement.position.coordinates[0])
        ) &&
        ((this.mapRef.mapObject.getZoom() <= 3 && settlement.type === SettlementType.Town) ||
          this.mapRef.mapObject.getZoom() > 3)
    );
  }

  // get Map object
  get mapRef(): LMap {
    return this.$refs.map as LMap & { mapObject: () => any };
  }

  getSettlementParams(settlement: Settlement): { fontSize?: string } {
    switch (settlement.type) {
      case SettlementType.Village:
        return { fontSize: '10' };
      case SettlementType.Castle:
        return { fontSize: '14' };
      case SettlementType.Town:
        return { fontSize: '16' };
    }
    // default params
    return { fontSize: '14' };
  }

  setDisplayedBounds(bounds: LatLngBounds | null): void {
    this.displayedBounds = bounds;
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
// Center txt
.settlement-icon {
  display: flex;
  align-items: center;
  justify-content: center;
}
</style>

<style scoped lang="scss">
.main-strategus {
  .map {
    //calc(Screen height - navbar)
    height: calc(100vh - 4.25rem);
    background-color: #284745;
    .settlement-icon-txt {
      display: inline-block;
      color: rgb(254, 255, 236);
      border-radius: 2px;
      white-space: nowrap;
      padding: 1px 8px;
      background-color: rgba(0, 0, 0, 0.4);
    }
  }
}
</style>
