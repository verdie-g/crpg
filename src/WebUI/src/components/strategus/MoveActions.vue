<template>
  <l-popup ref="popup" :options="{ closeButton: false, className: 'popup' }">
    <div class="buttons is-flex-direction-column is-align-items-center">
      <button
        v-for="action in actions"
        @click="onClickAction(action)"
        :key="'action-' + action"
        class="button is-fullwidth"
      >
        {{ action.charAt(0).toUpperCase() + action.slice(1) }}
      </button>
    </div>
  </l-popup>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { LMap, LPopup } from 'vue2-leaflet';
import ActionType from '@/models/action-type';
import Hero from '@/models/hero';
import Settlement from '@/models/settlement-public';

@Component({
  components: { LPopup },
})
export default class HeroComponent extends Vue {
  @Prop(Array) readonly actions: Array<ActionType>;
  @Prop(Object) readonly map: LMap;
  @Prop(Object) readonly hero: Hero;
  @Prop(Object) readonly settlement: Settlement;

  get popup(): LPopup {
    return this.$refs.popup as LPopup;
  }

  onClickAction(action: ActionType) {
    console.log(action);
    this.popup.mapObject.removeFrom(this.map.mapObject);
  }
}
</script>

<style lang="scss">
.leaflet-popup-content {
  margin: 5px;
}
.leaflet-popup-content-wrapper {
  border-radius: 4px;
}
</style>
