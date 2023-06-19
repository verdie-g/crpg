<script setup lang="ts">
import { ItemCompareMode, type ItemFlat } from '@/models/item';
import { type AggregationConfig } from '@/models/item-search';
import { useItemUpgrades } from '@/composables/item/use-item-upgrades';

const { item, cols } = defineProps<{
  item: ItemFlat;
  cols: AggregationConfig;
}>();

const { itemUpgrades, relativeEntries } = useItemUpgrades(item, cols, true);
</script>

<template>
  <OTable :data="itemUpgrades" :showHeader="false">
    <!-- offset col -->
    <OTableColumn :width="78" #default>
      <template></template>
    </OTableColumn>

    <OTableColumn field="name" #default="{ row: item }: { row: ItemFlat }">
      <ShopGridItemName :item="item" showTier />
    </OTableColumn>

    <OTableColumn
      v-for="field in (Object.keys(cols) as Array<keyof ItemFlat>)"
      #default="{ row: rowItem }: { row: ItemFlat }"
      :field="field"
      :width="cols[field]?.width ?? 140"
    >
      <ItemParam
        :item="rowItem"
        :field="field"
        isCompare
        :compareMode="ItemCompareMode.Relative"
        :relativeValue="relativeEntries[field]"
      />
    </OTableColumn>

    <template #empty>
      <div class="relative min-h-[10rem]">
        <OLoading active iconSize="xl" :fullPage="false" />
      </div>
    </template>
  </OTable>
</template>
