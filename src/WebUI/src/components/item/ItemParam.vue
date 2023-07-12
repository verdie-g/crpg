<script setup lang="ts">
import {
  ItemFieldCompareRule,
  ItemFieldFormat,
  type ItemFlat,
  ItemCompareMode,
} from '@/models/item';
import { aggregationsConfig } from '@/services/item-search-service/aggregations';
import {
  getItemFieldAbsoluteDiffStr,
  getItemFieldRelativeDiffStr,
  humanizeBucket,
} from '@/services/item-service';

const {
  item,
  field,
  bestValue,
  relativeValue,
  compareMode = ItemCompareMode.Absolute,
  isCompare = false,
} = defineProps<{
  item: ItemFlat;
  field: keyof ItemFlat;
  isCompare?: boolean;
  compareMode?: ItemCompareMode;
  bestValue?: number;
  relativeValue?: number;
}>();

const rawBuckets = computed(() => item[field]);
const compareRule = computed(
  () => aggregationsConfig[field]?.compareRule || ItemFieldCompareRule.Bigger
);
const isBest = computed(() => (bestValue !== undefined ? rawBuckets.value === bestValue : false));

const diffStr = computed(() => {
  if (typeof rawBuckets.value !== 'number' || !isCompare) {
    return null;
  }

  if (compareMode === ItemCompareMode.Absolute) {
    if (!isBest.value && bestValue !== undefined) {
      return getItemFieldAbsoluteDiffStr(compareRule.value, rawBuckets.value, bestValue);
    }

    return null;
  }

  if (relativeValue !== undefined) {
    return getItemFieldRelativeDiffStr(compareRule.value, rawBuckets.value, relativeValue);
  }

  return null;
});

const formattedBuckets = computed(() => {
  if (Array.isArray(rawBuckets.value)) {
    return rawBuckets.value.map(bucket => humanizeBucket(field, bucket, item));
  }

  return [humanizeBucket(field, rawBuckets.value, item)];
});

// TODO: spec, refactor: more readable
const fieldStyle = computed(() => {
  if (!isCompare || typeof rawBuckets.value !== 'number') return '';

  if (compareMode === ItemCompareMode.Absolute) {
    if (isBest.value) return 'color: rgb(52 211 153)';
    else return 'color:rgb(208 79 44)';
  }

  if (compareMode === ItemCompareMode.Relative) {
    if (relativeValue === undefined || rawBuckets.value === relativeValue) return '';

    if (compareRule.value === ItemFieldCompareRule.Less) {
      if (relativeValue > rawBuckets.value) return 'color: rgb(52 211 153)';
    } else {
      if (rawBuckets.value > relativeValue) return 'color: rgb(52 211 153)';
    }

    return 'color:rgb(208 79 44)';
  }
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

        <Tooltip
          v-else
          v-bind="{
            disabled: formattedValue.description === null,
            placement: 'top',
            title: formattedValue.label,
            ...(formattedValue.description !== null && { description: formattedValue.description }),
          }"
        >
          <Tag
            v-if="
              aggregationsConfig[field]?.format === ItemFieldFormat.List &&
              formattedValue.label !== ''
            "
            :label="formattedValue.label"
            size="sm"
          />

          <div v-else class="text-xs font-bold">
            {{ formattedValue.label }}
          </div>
        </Tooltip>
      </template>

      <div v-if="diffStr !== null" class="text-2xs font-bold">
        {{ diffStr }}
      </div>
    </template>
  </div>
</template>
