<template>
  <div class="has-text-white is-size-6 is-family-monospace">{{ mousePositionText }}</div>
</template>

<script lang="ts">
import { Component, Mixins } from 'vue-property-decorator';
import { LatLng, LeafletMouseEvent, Map } from 'leaflet';
import { LControl } from 'vue2-leaflet';

/* Leaflet control that display the coordinate of the mouse pointer */
@Component
export default class LControlMousePosition extends Mixins(LControl) {
  mousePosition: LatLng | null = null;
  map: Map | null = null;

  get mousePositionText(): string {
    if (this.mousePosition === null) {
      return '';
    }

    return (
      this.formatNumber(this.mousePosition.lat) + ' ' + this.formatNumber(this.mousePosition.lng)
    );
  }

  mounted() {
    this.map = (this.mapObject as any)._map as Map;
    this.map.on('mousemove', this.onMouseMove);
  }

  beforeDestroy() {
    this.map!.off('mousemove', this.onMouseMove);
  }

  onMouseMove(event: LeafletMouseEvent) {
    this.mousePosition = event.latlng;
  }

  formatNumber(n: number): string {
    const whole = Math.trunc(n);
    const decimal = Math.trunc(Math.abs(n % 1) * 1000);

    const wholeStr = (whole < 0 ? '-' : '+') + Math.abs(whole).toString().padStart(3, '0');
    const decimalStr = decimal.toString().padStart(3, '0');
    return wholeStr + '.' + decimalStr;
  }
}
</script>

<style scoped lang="scss"></style>
