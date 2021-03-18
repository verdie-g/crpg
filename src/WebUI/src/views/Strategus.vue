<template>
  <div class="mainStrategus">
    <l-map
      ref="map"
      class="map"
      :zoom="zoom"
      :center="center"
      :options="mapOptions"
      :max-bounds="maxBounds"
      @moveend="setDisplayedBounds($refs.map.mapObject.getBounds())"
      @click="test"
    >
      <l-tile-layer :url="url" :attribution="attribution" />
      <l-marker
        v-for="settlement in settlements"
        :lat-lng="[settlement.position.coordinates[1], settlement.position.coordinates[0]]"
        :key="settlement.id"
        :icon-url="IconeImg"
        :icon-size="[16, 16]"
        name="test"
      >
        test
      </l-marker>
    </l-map>
  </div>
</template>

<script lang="ts">
import { Vue, Component } from 'vue-property-decorator';
import { latLng, latLngBounds, CRS } from 'leaflet';
import { LMap, LTileLayer, LMarker, LPopup, LTooltip, LIcon } from 'vue2-leaflet';
import strategusModule from '@/store/strategus-module';
import Settlement from '@/models/settlement';
import IconeImg from '@/assets/map-marker-icon.png';
import SettlementType from '@/models/settlement-type';
import { Icon } from 'leaflet';

delete (Icon.Default.prototype as any)._getIconUrl;

Icon.Default.mergeOptions({
  iconRetinaUrl: require('leaflet/dist/images/marker-icon-2x.png'),
  iconUrl: require('leaflet/dist/images/marker-icon.png'),
  shadowUrl: require('leaflet/dist/images/marker-shadow.png'),
});

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
  displayedBounds = null;
  IconeImg = IconeImg;

  get settlements(): Settlement[] {
    return [strategusModule.settlements[0]];
    if (this.displayedBounds === null) {
      return [];
    }
    return strategusModule.settlements.filter(
      settlement =>
        this.displayedBounds.contains(
          latLng(settlement.position.coordinates[1], settlement.position.coordinates[0])
        ) &&
        (this.zoom > 6 ||
          (this.zoom > 4 && settlement.type === SettlementType.Castle) ||
          settlement.type === SettlementType.Town)
    );
  }

  setDisplayedBounds(bounds: LatLngBounds) {
    this.displayedBounds = bounds;
  }

  created() {
    strategusModule.getSettlements();
  }

  mounted() {
    console.log(this.$refs.map.mapObject);
    this.setDisplayedBounds(this.$refs.map.getBounds());
  }

  test(event: any) {
    console.log(event.latlng);
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
.mainStrategus {
  .map {
    //calc(Screen height - navbar)
    height: calc(100vh - 4.25rem);
    background-color: #284745;
  }
}
</style>
