<script setup lang="ts">
import { type ItemFlat } from '@/models/item';
import { type Aggregation, AggregationView } from '@/models/item-search';
import {
  getMinRange,
  getMaxRange,
  getStepRange,
  getBucketValues,
} from '@/services/item-search-service';

const modelValue = defineModel<string[] | number[]>();

const props = defineProps<{
  aggregation: itemsjs.SearchAggregation<ItemFlat, keyof ItemFlat>;
  scopeAggregation: itemsjs.SearchAggregation<ItemFlat, keyof ItemFlat>;
  aggregationConfig: Aggregation;
}>();
</script>

<template>
  <div class="relative flex items-center gap-1">
    <OIcon
      v-if="modelValue?.length"
      class="absolute -left-5 top-1/2 -translate-y-1/2 transform cursor-pointer hover:text-status-danger"
      v-tooltip.bottom="$t('action.reset')"
      icon="close"
      size="xs"
      @click="modelValue = []"
    />

    <VDropdown :triggers="['click']">
      <Tooltip
        :title="$t(`item.aggregations.${aggregation.name}.title`)"
        :description="$t(`item.aggregations.${aggregation.name}.description`)"
        :delay="{ show: 300 }"
      >
        <div
          class="max-w-[90px] cursor-pointer overflow-x-hidden text-ellipsis whitespace-nowrap border-b-2 border-dashed border-border-300 pb-0.5 text-2xs hover:text-content-100 2xl:max-w-[110px]"
        >
          {{ $t(`item.aggregations.${aggregation.name}.title`) }}
        </div>
      </Tooltip>

      <template #popper="{ hide }">
        <div class="max-w-md">
          <template v-if="aggregationConfig.view === AggregationView.Checkbox">
            <DropdownItem v-for="bucket in aggregation.buckets">
              <ShopGridFilterCheckboxItem
                v-model="modelValue"
                :aggregation="aggregation.name"
                :bucketValue="bucket.key"
                :docCount="bucket.doc_count"
                @update:modelValue="hide"
              />
            </DropdownItem>
          </template>

          <template v-else-if="aggregationConfig.view === AggregationView.Range">
            <div class="px-8 py-3">
              <SliderInput
                v-model="modelValue as number[]"
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
