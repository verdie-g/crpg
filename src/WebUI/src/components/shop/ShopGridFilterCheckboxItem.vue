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
      disabled: bucket.description === null,
      placement: 'top',
      title: bucket.label,
      ...(bucket.description !== null && {
        description: bucket.description,
      }),
    }"
  >
    <OCheckbox v-model="modelValue" :nativeValue="bucketValue" class="items-center">
      <div class="flex items-center gap-2">
        <ItemFieldIcon
          v-if="bucket.icon !== null"
          :icon="bucket.icon"
          :label="bucket.label"
          :showTooltip="false"
        />

        {{ bucket.label }}
        <span class="inline text-content-400">({{ docCount }})</span>
      </div>
    </OCheckbox>
  </Tooltip>
</template>
