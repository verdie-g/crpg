<template>
  <div class="is-flex is-flex-direction-column is-flex-grow-1">
    <div class="columns is-flex-wrap-wrap">
      <div v-for="field in itemDescriptor.fields" :key="field[0]" class="column is-half is-flex is-flex-direction-column">
        <span class="is-size-7">{{ field[0] }}</span>
        <strong>{{ field[1] }}</strong>
      </div>
    </div>
    <b-tabs v-if="itemDescriptor.modes.length > 1" :value="weaponIdx" class="weapon-tabs">
      <b-tab-item v-for="mode in itemDescriptor.modes" :key="mode.name" :label="mode.name">
        <div v-for="field in mode.fields">
          <strong>{{ field[1] }}</strong> {{ field[0] }}
          <br />
        </div>
        <b-taglist class="flags px-1 pt-2">
          <b-tag v-for="flag in mode.flags" :key="flag" type="is-info">{{ flag }}</b-tag>
        </b-taglist>
      </b-tab-item>
    </b-tabs>
    <div v-else class="is-flex is-flex-direction-column is-flex-grow-1">
      <div v-for="mode in itemDescriptor.modes" :key="mode.name" class="is-flex is-flex-direction-column is-flex-grow-1">
        <div class="is-flex-grow-1">
          <div v-for="field in mode.fields">
            <strong>{{ field[1] }}</strong> {{ field[0] }}
            <br />
          </div>
        </div>
        <b-taglist class="flags px-1 pt-2">
          <b-tag v-for="flag in mode.flags" :key="flag" type="is-info">{{ flag }}</b-tag>
        </b-taglist>
      </div>
    </div>
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
  @Prop(Number) readonly rank: number;
  @Prop(Number) readonly weaponIdx: number;

  get itemDescriptor(): ItemDescriptor {
    return getItemDescriptor(this.item, this.rank);
  }
}
</script>

<style scoped lang="scss">
.flags {
  margin-top: 4px;
}
</style>
