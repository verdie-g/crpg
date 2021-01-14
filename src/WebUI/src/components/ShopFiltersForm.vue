<template>
  <form>
    <b-field label="Type">
      <b-select v-model="types" multiple :native-size="8">
        <option v-for="([value, name], idx) in Object.entries(itemTypes)" :value="value" :key="idx">{{name}}</option>
      </b-select>
    </b-field>

    <b-field>
      <b-checkbox v-model="showOwned" :true-value="1" :false-value="0">Show owned items</b-checkbox>
    </b-field>
  </form>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { itemTypeToStr } from '@/services/item-service';
import { recordFilter } from '@/utils/record';
import ItemType from '@/models/item-type';
import ShopFilters from '@/models/ShopFilters';

@Component
export default class ShopFiltersForm extends Vue {
  @Prop({
    type: Object,
    default: (): ShopFilters => ({ types: [], showOwned: 1 }),
  }) readonly modelValue: ShopFilters;

  hiddenItemTypes = [ItemType.Undefined, ItemType.Pistol, ItemType.Musket, ItemType.Bullets];
  itemTypes = recordFilter(itemTypeToStr, t => !this.hiddenItemTypes.includes(t));

  get types(): ItemType[] {
    return this.modelValue.types;
  }

  set types(types: ItemType[]) {
    this.$emit('input', { types, showOwned: this.showOwned });
  }

  get showOwned(): number {
    return this.modelValue.showOwned;
  }

  set showOwned(showOwned: number) {
    this.$emit('input', { showOwned, types: this.types });
  }
}
</script>

<style scoped lang="scss">
</style>
