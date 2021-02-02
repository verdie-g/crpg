<template>
  <form>
    <b-field label="Type">
      <b-dropdown v-model="types" multiple aria-role="list">
        <template #trigger>
          <b-button type="is-primary" icon-right="caret-down">
            {{ typesString }}
          </b-button>
        </template>

        <b-dropdown-item v-for="([value, name], idx) in Object.entries(itemTypes)"
                         :value="value" :key="idx" aria-role="listitem">
          <span>{{ name }}</span>
        </b-dropdown-item>
      </b-dropdown>
    </b-field>

    <b-field>
      <b-checkbox v-model="showOwned">Show owned items</b-checkbox>
    </b-field>
  </form>
</template>

<script lang="ts">
import { Component, Model, Vue } from 'vue-property-decorator';
import { itemTypeToStr } from '@/services/item-service';
import { recordFilter } from '@/utils/record';
import ItemType from '@/models/item-type';
import ShopFilters from '@/models/ShopFilters';
import { stringTruncate } from '@/utils/string';

@Component
export default class ShopFiltersForm extends Vue {
  @Model('input', {
    type: Object,
    default: (): ShopFilters => ({ types: [], showOwned: true }),
  }) readonly filter: ShopFilters;

  hiddenItemTypes = [ItemType.Undefined, ItemType.Pistol, ItemType.Musket, ItemType.Bullets];
  itemTypes = recordFilter(itemTypeToStr, t => !this.hiddenItemTypes.includes(t));

  get typesString(): string {
    if (this.types.length === 0) {
      return '*';
    }

    const joinedTypes = this.types.map(t => itemTypeToStr[t]).join(', ');
    return stringTruncate(joinedTypes, 25);
  }

  get types(): ItemType[] {
    return this.filter.types;
  }

  set types(types: ItemType[]) {
    this.$emit('input', { types, showOwned: this.showOwned });
  }

  get showOwned(): boolean {
    return this.filter.showOwned;
  }

  set showOwned(showOwned: boolean) {
    this.$emit('input', { showOwned, types: this.types });
  }
}
</script>

<style scoped lang="scss">
</style>
