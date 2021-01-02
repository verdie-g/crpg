<template>
  <form>
    <b-field label="Type">
      <b-select multiple native-size="8" v-model="shopFilters.types" @input="onFilterInput">
        <option v-for="[value, name] in Object.entries(itemTypes)" :value="value">{{name}}</option>
      </b-select>
    </b-field>

    <b-field>
      <b-checkbox v-model="shopFilters.showOwned" @input="onFilterInput">Show owned items</b-checkbox>
    </b-field>
  </form>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { itemTypeToStr } from '@/services/item-service';
import { recordFilter } from '@/utils/record';
import ItemType from '@/models/item-type';

@Component
export default class ShopFiltersForm extends Vue {
  hiddenItemTypes = [ItemType.Undefined, ItemType.Pistol, ItemType.Musket, ItemType.Bullets];
  itemTypes = recordFilter(itemTypeToStr, t => !this.hiddenItemTypes.includes(t));

  shopFilters = {
    types: [],
    showOwned: true,
  };

  onFilterInput(): void {
    this.$emit('input', {
      types: this.shopFilters.types,
      showOwned: this.shopFilters.showOwned,
    });
  }
}
</script>

<style scoped lang="scss">
</style>
