<template>
  <div>
    <l-map
      class="map"
      ref="map"
      :zoom="zoom"
      :center="center"
      :options="mapOptions"
      :max-bounds="maxBounds"
      @click="infoPos"
      @moveend="setVisibleBounds($refs.map.mapObject.getBounds())"
    >
      <l-tile-layer :url="url" :attribution="attribution" />
      <l-marker
        v-for="settlement in settlements"
        :lat-lng="[settlement.position.coordinates[1], settlement.position.coordinates[0]]"
        :key="settlement.id"
        :icon-url="IconeImg"
        :icon-size="[16, 16]"
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
import IconeImg from '@/assets/map-marker-icon.png';
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
  IconeImg = IconeImg;
  maxBounds = latLngBounds([
    [-40.6, 5.1],
    [-215.4, 250.8],
  ]);
  displayedBound = null;

  get settlements(): Settlement[] {
    if (this.displayedBound) {
      return strategusModule.settlements.filter(
        settlement =>
          this.displayedBound.contains(
            latLng(settlement.position.coordinates[1], settlement.position.coordinates[0])
          ) &&
          ((this.zoom > 5 && ['Village', 'Town', 'Castle'].includes(settlement.type)) ||
            (this.zoom <= 5 && ['Castle'].includes(settlement.type)))
      );
    }
    return [];
  }

  setVisibleBounds(bounds: any) {
    this.displayedBound = bounds;
  }

  created() {
    strategusModule.getSettlements();
  }
  mounted() {
    this.setVisibleBounds(this.$refs.map.mapObject.getBounds());
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
