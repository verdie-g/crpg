<template>
  <form>
    <b-field>
      <p class="control">
        <b-input
          placeholder="Search..."
          type="search"
          icon="search"
          size="is-medium"
          v-model.lazy="searchQuery"
        />
      </p>
    </b-field>
    <b-field label="Type">
      <b-dropdown v-model="type" aria-role="list">
        <template #trigger>
          <b-button type="is-primary" icon-right="caret-down">
            {{ typeString }}
          </b-button>
        </template>

        <b-dropdown-item :value="null" :key="0" aria-role="listitem">
          <span>Any</span>
        </b-dropdown-item>
        <b-dropdown-item
          v-for="([value, name], idx) in Object.entries(allTypes)"
          :value="value"
          :key="idx + 1"
          aria-role="listitem"
        >
          <span>{{ name }}</span>
        </b-dropdown-item>
      </b-dropdown>
    </b-field>

    <b-field label="Culture">
      <b-dropdown v-model="culture" aria-role="list">
        <template #trigger>
          <b-button type="is-primary" icon-right="caret-down">
            {{ cultureString }}
          </b-button>
        </template>

        <b-dropdown-item :value="null" :key="0" aria-role="listitem">
          <span>Any</span>
        </b-dropdown-item>
        <b-dropdown-item
          v-for="(culture, idx) in allCultures"
          :value="culture"
          :key="idx + 1"
          aria-role="listitem"
        >
          <span>{{ culture }}</span>
        </b-dropdown-item>
      </b-dropdown>
    </b-field>

    <b-field label="Sort By">
      <b-dropdown v-model="sortBy" aria-role="list">
        <template #trigger>
          <b-button type="is-primary" icon-right="caret-down">{{ sortBy }}</b-button>
        </template>
        <b-dropdown-item :value="null" :key="0" aria-role="listitem">
          <span>Price</span>
        </b-dropdown-item>
        <b-dropdown-item
          v-for="(sortableProperty, i) in sortableProperties"
          :value="sortableProperty"
          :key="sortableProperty + i"
          aria-role="listitem"
        >
          <span>{{ sortableProperty }}</span>
        </b-dropdown-item>
      </b-dropdown>
      <p class="control">
        <b-button
          type="is-primary"
          :icon-right="sortButtonIcon"
          @click="onSortDirectionButtonClicked"
        />
      </p>
    </b-field>

    <b-field>
      <b-checkbox v-model="showOwned">Show owned items</b-checkbox>
    </b-field>

    <b-field>
      <b-checkbox v-model="showAffordable">Show only affordable items</b-checkbox>
    </b-field>
  </form>
</template>

<script lang="ts">
import { Component, Model, Prop, Vue } from 'vue-property-decorator';
import { itemTypeToStr } from '@/services/item-service';
import { recordFilter } from '@/utils/record';
import ItemType from '@/models/item-type';
import ShopFilters from '@/models/shop-filters';
import Culture from '@/models/culture';

@Component
export default class ShopFiltersForm extends Vue {
  @Model('input', {
    type: Object,
    default: (): ShopFilters => ({
      type: null,
      culture: null,
      showOwned: true,
      showAffordable: false,
      sortBy: '',
      sortDesc: false,
      searchQuery: '',
    }),
  })
  readonly filter: ShopFilters;
  @Prop(Array)
  readonly sortableProperties: string[];

  hiddenItemTypes = [ItemType.Undefined, ItemType.Pistol, ItemType.Musket, ItemType.Bullets];
  allTypes = recordFilter(itemTypeToStr, t => !this.hiddenItemTypes.includes(t));
  allCultures = Object.values(Culture).filter(c => c !== Culture.Neutral);

  get typeString(): string {
    return this.type === null ? 'Any' : itemTypeToStr[this.type];
  }

  get type(): ItemType | null {
    return this.filter.type;
  }

  set type(type: ItemType | null) {
    if (type === this.type) {
      return;
    }

    this.emitInput({ type, sortBy: 'Price' });
  }

  get cultureString(): string {
    return this.culture === null ? 'Any' : this.culture;
  }

  get culture(): Culture | null {
    return this.filter.culture;
  }

  set culture(culture: Culture | null) {
    if (culture === this.culture) {
      return;
    }

    this.emitInput({ culture });
  }

  get showOwned(): boolean {
    return this.filter.showOwned;
  }

  set showOwned(showOwned: boolean) {
    this.emitInput({ showOwned });
  }

  get showAffordable(): boolean {
    return this.filter.showAffordable;
  }

  set showAffordable(showAffordable: boolean) {
    this.emitInput({ showAffordable });
  }

  get sortBy(): string {
    if (!this.sortableProperties.some(sortableProp => this.filter.sortBy === sortableProp))
      return 'Price';
    return this.filter.sortBy;
  }
  set sortBy(sortBy: string) {
    this.emitInput({ sortBy });
  }
  get sortDesc(): boolean {
    return this.filter.sortDesc;
  }
  set sortDesc(sortDesc: boolean) {
    this.emitInput({ sortDesc });
  }
  get sortButtonIcon() {
    if (this.sortDesc) return 'arrow-down';
    return 'arrow-up';
  }
  get searchQuery(): string {
    return this.filter.searchQuery;
  }
  set searchQuery(searchQuery: string) {
    this.emitInput({ searchQuery });
  }

  emitInput(shopFilters: Partial<ShopFilters>) {
    this.$emit('input', {
      type: this.type,
      culture: this.culture,
      showOwned: this.showOwned,
      showAffordable: this.showAffordable,
      sortBy: this.sortBy,
      sortDesc: this.sortDesc,
      searchQuery: this.searchQuery,
      ...shopFilters,
    });
  }

  onSortDirectionButtonClicked(): void {
    this.sortDesc = !this.sortDesc;
  }
}
</script>

<style scoped lang="scss"></style>
