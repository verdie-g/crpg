<template>
  <div class="leaflet-bar">
    <a href="#" @click="onClick" title="Locate party">
      <b-icon icon="crosshairs" size="is-medium" />
    </a>
  </div>
</template>

<script lang="ts">
import { Component, Mixins } from 'vue-property-decorator';
import { Map } from 'leaflet';
import { LControl } from 'vue2-leaflet';
import strategusModule from '@/store/strategus-module';
import { positionToLatLng } from '@/utils/geometry';

/* Leaflet control that display the coordinate of the mouse pointer */
@Component
export default class LocatePartyControl extends Mixins(LControl) {
  map: Map | null = null;

  mounted() {
    this.map = (this.mapObject as any)._map as Map;
  }

  onClick() {
    const party = strategusModule.party;
    if (party === null) {
      return;
    }

    const latLng = positionToLatLng(party.position.coordinates);
    this.map?.flyTo(latLng);
  }
}
</script>

<style scoped lang="scss"></style>
