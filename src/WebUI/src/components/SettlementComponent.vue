<template>
  <l-marker :lat-lng="[settlement.position.coordinates[1], settlement.position.coordinates[0]]">
    <l-icon class-name="is-flex is-justify-content-center is-align-items-center">
      <div
        class="settlement-icon-txt has-text-light px-3"
        :class="getSettlementCssClass(settlement)"
      >
        {{ settlement.name }}
      </div>
    </l-icon>
  </l-marker>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { Icon } from 'leaflet';
import { LIcon, LMarker } from 'vue2-leaflet';
import Settlement from '@/models/settlement';
import SettlementType from '@/models/settlement-type';

//Default icons
delete (Icon.Default.prototype as any)._getIconUrl;
Icon.Default.mergeOptions({
  iconRetinaUrl: require('leaflet/dist/images/marker-icon-2x.png'),
  iconUrl: require('leaflet/dist/images/marker-icon.png'),
  shadowUrl: require('leaflet/dist/images/marker-shadow.png'),
});

@Component({
  components: { LIcon, LMarker },
})
export default class SettlementComponent extends Vue {
  @Prop(Object) readonly settlement: Settlement[];

  getSettlementCssClass(settlement: Settlement): string {
    switch (settlement.type) {
      case SettlementType.Village:
        return 'is-size-7';
      case SettlementType.Castle:
        return 'is-size-6';
      case SettlementType.Town:
        return 'is-size-5';
      default:
        return 'is-size-7';
    }
  }
}
</script>

<style scoped lang="scss">
.settlement-icon-txt {
  display: inline-block;
  border-radius: 2px;
  white-space: nowrap;
  background-color: rgba(0, 0, 0, 0.4);
}
</style>
