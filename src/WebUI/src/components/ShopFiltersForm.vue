<template>
  <form>
    <b-field :label="$t('shopFiltersFormItemType')">
      <b-dropdown v-model="type" aria-role="list">
        <template #trigger>
          <b-button type="is-primary" icon-right="caret-down">
            {{ typeString }}
          </b-button>
        </template>

        <b-dropdown-item :value="null" :key="0" aria-role="listitem">
          <span>{{ $t('shopFiltersFormAny') }}</span>
        </b-dropdown-item>
        <b-dropdown-item
          v-for="([value, name], idx) in allTypes.entries()"
          :value="value"
          :key="idx + 1"
          aria-role="listitem"
        >
          <span>{{ name }}</span>
        </b-dropdown-item>
      </b-dropdown>
    </b-field>

    <b-field :label="$t('shopFiltersFormCulture')">
      <b-dropdown v-model="culture" aria-role="list">
        <template #trigger>
          <b-button type="is-primary" icon-right="caret-down">
            {{ cultureString }}
          </b-button>
        </template>

        <b-dropdown-item :value="null" :key="0" aria-role="listitem">
          <span>{{ $t('shopFiltersFormAny') }}</span>
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

    <b-field>
      <b-checkbox v-model="showOwned">{{ $t('shopFiltersShowOwnedItems') }}</b-checkbox>
    </b-field>

    <b-field>
      <b-checkbox v-model="showAffordable">{{ $t('shopFiltersShowAffordableItems') }}</b-checkbox>
    </b-field>
  </form>
</template>

<script lang="ts">
import { Component, Model, Vue } from 'vue-property-decorator';
import { itemTypeToStr } from '@/services/item-service';
import { mapFilter } from '@/utils/map';
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
    }),
  })
  readonly filter: ShopFilters;

  hiddenItemTypes = [ItemType.Undefined, ItemType.Pistol, ItemType.Musket, ItemType.Bullets];
  allTypes = mapFilter(itemTypeToStr(), t => !this.hiddenItemTypes.includes(t));
  allCultures = Object.values(Culture).filter(c => c !== Culture.Neutral);

  get typeString(): string {
    if (this.type === null) return this.$t('shopFiltersFormAny').toString();
    const translatedItemType = itemTypeToStr().get(this.type);
    return !!translatedItemType ? translatedItemType : this.$t('shopFiltersFormAny').toString();
  }

  get type(): ItemType | null {
    return this.filter.type;
  }

  set type(type: ItemType | null) {
    if (type === this.type) {
      return;
    }

    this.emitInput({ type });
  }

  get cultureString(): string {
    return this.culture === null ? this.$t('shopFiltersFormAny').toString() : this.culture;
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

  emitInput(shopFilters: Partial<ShopFilters>) {
    this.$emit('input', {
      type: this.type,
      culture: this.culture,
      showOwned: this.showOwned,
      showAffordable: this.showAffordable,
      ...shopFilters,
    });
  }
}
</script>

<style scoped lang="scss"></style>
