<script setup lang="ts">
import { ItemFieldFormat, type ItemFlat } from '@/models/item';
import { aggregationsConfig } from '@/services/item-search-service/aggregations';
import { getItemFieldDiffStr, humanizeBucket } from '@/services/item-service';

const props = withDefaults(
  defineProps<{
    item: ItemFlat;
    field: keyof ItemFlat;
    bestValue?: number;
    compareMode: boolean;
  }>(),
  { compareMode: false }
);

const rawBuckets = computed(() => props.item[props.field]);
const compareRule = computed(() => aggregationsConfig[props.field]?.compareRule);
const isBest = computed(() =>
  props.bestValue !== undefined ? rawBuckets.value === props.bestValue : false
);

// TODO:
const diffStr = computed(() => {
  if (
    typeof rawBuckets.value === 'number' &&
    props.compareMode &&
    !isBest.value &&
    compareRule.value !== undefined &&
    props.bestValue !== undefined
  ) {
    return getItemFieldDiffStr(compareRule.value, rawBuckets.value, props.bestValue);
  }

  return null;
});

// TODO:
const formattedBuckets = computed(() => {
  if (Array.isArray(rawBuckets.value)) {
    return rawBuckets.value.map(v => humanizeBucket(props.field, v, props.item));
  }

  return [humanizeBucket(props.field, rawBuckets.value, props.item)];
});

// TODO: to class
const fieldStyle = computed(() => {
  if (!props.compareMode || compareRule.value === undefined) return '';
  if (isBest.value) return 'color: rgb(52 211 153)';
  else return 'color:rgb(208 79 44)';
});
</script>

<template>
  <!-- TODO: badge for array without icon, custom style for price-->
  <div :style="fieldStyle" class="flex flex-wrap items-center gap-1">
    <template v-for="formattedValue in formattedBuckets">
      <slot v-if="$slots.default" v-bind="{ rawBuckets, formattedValue, diffStr }" />

      <template v-else>
        <ItemFieldIcon
          v-if="formattedValue.icon !== null"
          :icon="formattedValue.icon"
          :label="(formattedValue.label as string)"
          size="2xl"
        />

        <Tag
          v-else-if="aggregationsConfig[field]?.format === ItemFieldFormat.List"
          :label="formattedValue.label"
          size="sm"
        />

        <div v-else class="text-xs font-bold">
          {{ formattedValue.label }}
        </div>
      </template>

      <div v-if="diffStr !== null" class="text-2xs font-bold">
        {{ diffStr }}
      </div>
    </template>
  </div>
</template>
