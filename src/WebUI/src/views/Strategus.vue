<template>
  <div class="main_strategus height100 has-navbar-fixed-top">
    <transition name="fade">
      <div v-if="showLoader" class="height100" id="loader">
        <img src="@/assets/loader.gif" />
      </div>
    </transition>

    <l-map
      class="height100 has-navbar-fixed-top"
      id="map"
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
  zoom = 6;
  center = latLng(-137, 131);
  url = 'http://pecores.fr/gigamap/{z}/{y}/{x}.png';
  attribution = 'TaleWorlds Entertainment';
  mapOptions = {
    zoomSnap: 0.5,
    minZoom: 3,
    maxZoom: 7,
    crs: CRS.Simple,
    maxBoundsViscosity: 0.8,
  };
  maxBounds = latLngBounds([
    [-40.6, 5.3],
    [-215.4, 250.8],
  ]);
  showLoader = true;
  infoPos(event: any) {
    console.log(event.latlng);
  }
  created() {
    this.showLoader = false;
  }
}
</script>

<style scoped lang="scss">
.main_strategus {
  padding-top: 14px;
  #map {
    width: 100%;
  }
  .fade-enter-active,
  .fade-leave-active {
    transition: opacity 0.5s;
  }
  .fade-enter,
  .fade-leave-to {
    opacity: 0;
  }
  #loader {
    display: flex;
    align-items: center;
    justify-content: center;
    background-color: #131313;
    > img {
      height: 60px;
    }
  }
}
</style>
