<template>
  <l-circle-marker
    v-if="hero"
    :lat-lng="[hero.position.coordinates[1], hero.position.coordinates[0]]"
    :radius="5"
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

@Component({
  components: { LCircleMarker },
})
export default class HeroComponent extends Vue {
  @Prop(Object) readonly hero: Hero;
}
</script>

<style scoped lang="scss"></style>
