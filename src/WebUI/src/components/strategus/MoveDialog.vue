<template>
  <l-layer-group ref="layerGroup">
    <l-popup :lat-lng="latLng" :options="{ className: 'move-popup' }" @remove="$emit('cancel')">
      <div class="is-flex-direction-column">
        <button
          v-for="movementType in movementTypes"
          :key="movementType"
          @click="$emit('confirm', movementType)"
          class="button is-fullwidth"
        >
          {{ movementTypesStr[movementType] }}
        </button>
      </div>
    </l-popup>
  </l-layer-group>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { LatLng } from 'leaflet';
import { LLayerGroup, LPopup } from 'vue2-leaflet';
import MovementType from '@/models/movement-type';

@Component({
  components: { LLayerGroup, LPopup },
})
export default class MoveDialogComponent extends Vue {
  @Prop(Object) readonly latLng: LatLng;
  @Prop(Array) readonly movementTypes: MovementType[];

  movementTypesStr: Record<MovementType, string> = {
    [MovementType.Move]: this.$t('strategusMoveDialogMove').toString(),
    [MovementType.Follow]: this.$t('strategusMoveDialogFollow').toString(),
    [MovementType.Attack]: this.$t('strategusMoveDialogAttack').toString(),
  };

  mounted() {
    this.$nextTick(() => (this.$refs.layerGroup as any).mapObject.openPopup(this.latLng));
    (this.$refs.layerGroup as any).mapObject.on('popupclose', () => this.$emit('cancel'));
  }
}

export function promptMovementType(
  parent: Vue,
  latLng: LatLng,
  movementTypes: MovementType[]
): Promise<MovementType | null> {
  const DialogCtor = Vue.extend(MoveDialogComponent);
  const dialog = new DialogCtor({
    el: document.createElement('div'),
    propsData: {
      latLng,
      movementTypes,
    },
    parent,
  });
  parent.$el.appendChild(dialog.$el);

  const destroy = () => {
    dialog.$destroy();
    dialog.$el.parentNode!.removeChild(dialog.$el);
  };

  return new Promise(resolve => {
    let resolved = false;
    dialog.$on('confirm', (movement: MovementType) => {
      resolved = true;
      destroy();
      resolve(movement);
    });
    dialog.$on('cancel', () => {
      // 'cancel' is emitted when the popup is closed, which happens if the user clicked
      // on the cross but also if the user clicked on an action which emits a 'confirm'
      // event that already resolves the promise.
      if (resolved) {
        return;
      }

      resolved = true;
      destroy();
      resolve(null);
    });
  });
}
</script>

<style lang="scss">
.move-popup {
  .leaflet-popup-content {
    margin: 0;
  }

  .leaflet-popup-content-wrapper {
    border-radius: 4px; // Same as bulma buttons.
  }
}
</style>
