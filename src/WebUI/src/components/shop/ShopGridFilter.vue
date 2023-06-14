<script setup lang="ts">
import { type ItemFlat } from '@/models/item';
import { type Aggregation, AggregationView } from '@/models/item-search';
import {
  getMinRange,
  getMaxRange,
  getStepRange,
  getBucketValues,
} from '@/services/item-search-service';
import { humanizeBucket } from '@/services/item-service';

const props = defineProps<{
  aggregation: itemsjs.SearchAggregation<ItemFlat, keyof ItemFlat>;
  scopeAggregation: itemsjs.SearchAggregation<ItemFlat, keyof ItemFlat>;
  aggregationConfig: Aggregation;
  modelValue: string[] | number[];
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', modelValue: string[] | number[]): void;
}>();

const innerModel = computed({
  get() {
    return props.modelValue;
  },

  set(val) {
    emit('update:modelValue', val);
  },
});
</script>

<template>
  <div class="relative flex items-center gap-1">
    <OIcon
      v-if="innerModel?.length"
      class="absolute -left-5 top-1/2 -translate-y-1/2 transform cursor-pointer hover:text-status-danger"
      v-tooltip.bottom="$t('action.reset')"
      icon="close"
      size="xs"
      @click="innerModel = []"
    />

    <VDropdown :triggers="['click']">
      <VTooltip :delay="{ show: 600 }">
        <div
          class="max-w-[90px] cursor-pointer overflow-x-hidden text-ellipsis whitespace-nowrap border-b-2 border-dashed border-border-300 pb-0.5 text-2xs hover:text-content-100 2xl:max-w-[120px]"
        >
          {{ $t(`item.aggregations.${aggregation.name}.title`) }}
        </div>

        <template #popper>
          <div class="prose prose-invert">
            <h5 class="text-content-100">
              {{ $t(`item.aggregations.${aggregation.name}.title`) }}
            </h5>
            <p v-if="$t(`item.aggregations.${aggregation.name}.description`)">
              {{ $t(`item.aggregations.${aggregation.name}.description`) }}
            </p>
          </div>
        </template>
      </VTooltip>

      <template #popper>
        <div class="max-w-md">
          <template v-if="aggregationConfig.view === AggregationView.Checkbox">
            <DropdownItem v-for="bucket in aggregation.buckets">
              <OCheckbox v-model="innerModel" :nativeValue="bucket.key" class="items-center">
                <div class="flex items-center gap-2">
                  <ItemFieldIcon
                    v-if="humanizeBucket(aggregation.name, bucket.key)?.icon"
                    :icon="humanizeBucket(aggregation.name, bucket.key).icon!"
                    :label="humanizeBucket(aggregation.name, bucket.key)!.label"
                    :showTooltip="false"
                  />
                  <div>
                    {{ humanizeBucket(aggregation.name, bucket.key).label }}
                    <span class="inline text-content-400">({{ bucket.doc_count }})</span>
                  </div>
                </div>
              </OCheckbox>
            </DropdownItem>
          </template>

          <template v-else-if="aggregationConfig.view === AggregationView.Range">
            <div class="px-8 py-3">
              <SliderInput
                v-model="innerModel"
                :min="getMinRange(getBucketValues(scopeAggregation.buckets))"
                :max="getMaxRange(getBucketValues(scopeAggregation.buckets))"
                :step="getStepRange(getBucketValues(scopeAggregation.buckets))"
              />
            </div>
          </template>
        </div>
      </template>
    </VDropdown>
  </div>
</template>
