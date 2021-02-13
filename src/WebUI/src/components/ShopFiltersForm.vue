<template>
  <form>
    <b-field label="Type">
      <b-dropdown v-model="types" multiple aria-role="list">
        <template #trigger>
          <b-button type="is-primary" icon-right="caret-down">
            {{ typesString }}
          </b-button>
        </template>

        <b-dropdown-item
          v-for="([value, name], idx) in Object.entries(itemTypes)"
          :value="value"
          :key="idx"
          aria-role="listitem"
        >
          <span>{{ name }}</span>
        </b-dropdown-item>
      </b-dropdown>
    </b-field>

    <b-field label="Culture">
      <b-dropdown v-model="cultures" multiple aria-role="list">
        <template #trigger>
          <b-button type="is-primary" icon-right="caret-down">
            {{ culturesString }}
          </b-button>
        </template>

        <b-dropdown-item
          v-for="(culture, idx) in allCultures"
          :value="culture"
          :key="idx"
          aria-role="listitem"
        >
          <span>{{ culture }}</span>
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
import Culture from '@/models/culture';

@Component
export default class ShopFiltersForm extends Vue {
  @Model('input', {
    type: Object,
    default: (): ShopFilters => ({ types: [], cultures: [], showOwned: true }),
  })
  readonly filter: ShopFilters;

  hiddenItemTypes = [ItemType.Undefined, ItemType.Pistol, ItemType.Musket, ItemType.Bullets];
  itemTypes = recordFilter(itemTypeToStr, t => !this.hiddenItemTypes.includes(t));
  allCultures = Object.values(Culture);

  get typesString(): string {
    if (this.types.length === 0) {
      return '*';
    }

    const joinedTypes = this.types.map(t => itemTypeToStr[t]).join(', ');
    return stringTruncate(joinedTypes, 20);
  }

  get types(): ItemType[] {
    return this.filter.types;
  }

  set types(types: ItemType[]) {
    this.emitInput({ types });
  }

  get culturesString(): string {
    return this.cultures.length === 0 ? '*' : stringTruncate(this.cultures.join(', '), 20);
  }

  get cultures(): Culture[] {
    return this.filter.cultures;
  }

  set cultures(cultures: Culture[]) {
    this.emitInput({ cultures });
  }

  get showOwned(): boolean {
    return this.filter.showOwned;
  }

  set showOwned(showOwned: boolean) {
    this.emitInput({ showOwned });
  }

  emitInput(shopFilters: Partial<ShopFilters>) {
    this.$emit('input', {
      types: this.types,
      cultures: this.cultures,
      showOwned: this.showOwned,
      ...shopFilters,
    });
  }
}
</script>

<style scoped lang="scss">
</style>
