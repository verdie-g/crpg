<template>
  <l-circle-marker
    v-if="hero"
    :lat-lng="[hero.position.coordinates[1], hero.position.coordinates[0]]"
    :radius="markerRadius"
    color="#f00"
    :fill="true"
    fillColor="#f00"
    :fillOpacity="1.0"
  />
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { LCircleMarker } from 'vue2-leaflet';
import Hero from '@/models/hero';
import Constants from '@/../../../data/constants.json';

const minRadius = 4;
const maxRadius = 10;

@Component({
  components: { LCircleMarker },
})
export default class HeroComponent extends Vue {
  @Prop(Object) readonly hero: Hero | null;

  get markerRadius(): number {
    if (this.hero === null) {
      return 0;
    }

    const troopsRange = Constants.strategusMaxHeroTroops - Constants.strategusMinHeroTroops;
    const sizeFactor = this.hero.troops / troopsRange;
    return minRadius + sizeFactor * (maxRadius - minRadius);
  }
}
</script>

<style scoped lang="scss"></style>
