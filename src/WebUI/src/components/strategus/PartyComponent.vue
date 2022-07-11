<template>
  <l-circle-marker
    :lat-lng="[party.position.coordinates[1], party.position.coordinates[0]]"
    :radius="markerRadius"
    :color="markerColor"
    :fill="true"
    :fillColor="markerColor"
    :fillOpacity="1.0"
    @click="onClick"
  >
    <l-tooltip :options="{ direction: 'top' }">{{ party.name }} ({{ party.troops }})</l-tooltip>
  </l-circle-marker>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { LeafletMouseEvent, DomEvent } from 'leaflet';
import { LCircleMarker, LTooltip } from 'vue2-leaflet';
import Party from '@/models/party';
import Constants from '@/../../../data/constants.json';

const minRadius = 4;
const maxRadius = 10;

@Component({
  components: { LCircleMarker, LTooltip },
})
export default class PartyComponent extends Vue {
  @Prop(Object) readonly party: Party;
  @Prop(Boolean) readonly self: boolean;

  get markerRadius(): number {
    const troopsRange = Constants.strategusMaxPartyTroops - Constants.strategusMinPartyTroops;
    const sizeFactor = this.party.troops / troopsRange;
    return minRadius + sizeFactor * (maxRadius - minRadius);
  }

  get markerColor(): string {
    return this.self ? '#0f0' : '#f00';
  }

  onClick(event: LeafletMouseEvent) {
    DomEvent.stopPropagation(event);
    this.$emit('click');
  }
}
</script>

<style scoped lang="scss"></style>
