<template>
  <div>
    <div v-for="field in itemDescriptor.fields">{{ field[0] }}: {{ field[1] }}</div>
    <b-tabs v-if="itemDescriptor.modes.length" :value="weaponIdx" class="weapon-tabs">
      <b-tab-item v-for="mode in itemDescriptor.modes" :label="mode.name">
        <div v-for="field in mode.fields">
          {{ field[0] }}: {{ field[1] }}
          <br />
        </div>
        <b-taglist class="flags">
          <b-tag v-for="flag in mode.flags" type="is-info">{{ flag }}</b-tag>
        </b-taglist>
      </b-tab-item>
    </b-tabs>
  </div>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { getItemDescriptor } from '@/services/item-service';
import Item from '@/models/item';
import { ItemDescriptor } from '@/models/item-descriptor';

@Component
export default class ItemProperties extends Vue {
  @Prop(Object) readonly item: Item;
  @Prop(Number) readonly weaponIdx: number;

  get itemDescriptor(): ItemDescriptor {
    return getItemDescriptor(this.item);
  }
}
</script>

<style scoped lang="scss">
.flags {
  margin-top: 4px;
}
</style>
