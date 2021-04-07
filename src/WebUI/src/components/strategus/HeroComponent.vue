<template>
  <l-circle-marker
    :lat-lng="[hero.position.coordinates[1], hero.position.coordinates[0]]"
    :radius="markerRadius"
    :color="markerColor"
    :fill="true"
    :fillColor="markerColor"
    :fillOpacity="1.0"
  >
    <l-tooltip :options="{ direction: 'top' }">{{ hero.name }} ({{ hero.troops }})</l-tooltip>
    <move-actions v-if="!self" :actions="[Action.Move, Action.Follow]" :map="map" :hero="hero" />
  </l-circle-marker>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { LMap, LCircleMarker, LTooltip, LPopup } from 'vue2-leaflet';
import Hero from '@/models/hero';
import ActionType from '@/models/action-type';
import MoveActions from '@/components/strategus/MoveActions.vue';
import Constants from '@/../../../data/constants.json';

const minRadius = 4;
const maxRadius = 10;

@Component({
  components: { LCircleMarker, LTooltip, LPopup, MoveActions },
})
export default class HeroComponent extends Vue {
  @Prop(Object) readonly hero: Hero;
  @Prop(Object) readonly map: LMap;
  @Prop(Boolean) readonly self: boolean;

  Action = ActionType;

  get markerRadius(): number {
    const troopsRange = Constants.strategusMaxHeroTroops - Constants.strategusMinHeroTroops;
    const sizeFactor = this.hero.troops / troopsRange;
    return minRadius + sizeFactor * (maxRadius - minRadius);
  }

  get markerColor(): string {
    return this.self ? '#0f0' : '#f00';
  }
}
</script>

<style scoped lang="scss"></style>
