<script setup lang="ts">
import { type ItemFlat } from '@/models/item';
import { humanizeBucket } from '@/services/item-service';

const modelValue = defineModel<string[] | number[]>();

const { aggregation, bucketValue, docCount } = defineProps<{
  aggregation: keyof ItemFlat;
  bucketValue: any;
  docCount: number;
}>();

const bucket = computed(() => humanizeBucket(aggregation, bucketValue));
</script>

<template>
  <Tooltip
    v-bind="{
      disabled: bucket.tooltip === null,
      placement: 'top',
      title: bucket.label,
      ...(bucket.tooltip !== null &&
        bucket.tooltip?.title !== null && {
          title: bucket!.tooltip!.title,
        }),
      ...(bucket.tooltip !== null &&
        bucket.tooltip.description !== null && {
          description: bucket!.tooltip!.description,
        }),
    }"
  >
    <OCheckbox v-model="modelValue" :nativeValue="bucketValue" class="items-center">
      <div class="flex items-center gap-2">
        <ItemFieldIcon v-if="bucket.icon !== null" :icon="bucket.icon" :label="bucket.label" />
        {{ bucket.label }}
        <span class="inline text-content-400">({{ docCount }})</span>
      </div>
    </OCheckbox>
  </Tooltip>
</template>
