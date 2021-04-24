<template>
  <l-circle-marker
    :lat-lng="[hero.position.coordinates[1], hero.position.coordinates[0]]"
    :radius="markerRadius"
    :color="markerColor"
    :fill="true"
    :fillColor="markerColor"
    :fillOpacity="1.0"
    @click="onClick"
  >
    <l-tooltip :options="{ direction: 'top' }">{{ hero.name }} ({{ hero.troops }})</l-tooltip>
  </l-circle-marker>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { LeafletMouseEvent, DomEvent } from 'leaflet';
import { LCircleMarker, LTooltip } from 'vue2-leaflet';
import Hero from '@/models/hero';
import Constants from '@/../../../data/constants.json';

const minRadius = 4;
const maxRadius = 10;

@Component({
  components: { LCircleMarker, LTooltip },
})
export default class HeroComponent extends Vue {
  @Prop(Object) readonly hero: Hero;
  @Prop(Boolean) readonly self: boolean;

  get markerRadius(): number {
    const troopsRange = Constants.strategusMaxHeroTroops - Constants.strategusMinHeroTroops;
    const sizeFactor = this.hero.troops / troopsRange;
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
