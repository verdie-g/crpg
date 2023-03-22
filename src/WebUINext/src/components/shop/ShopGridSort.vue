<script setup lang="ts">
import { type SortingConfig } from '@/models/item-search';

const props = defineProps<{
  sortingConfig: SortingConfig;
  modelValue: string;
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void;
}>();

const sortingKeys = computed(() => Object.keys(props.sortingConfig));
const selfSortingIsActive = computed(() => sortingKeys.value.includes(props.modelValue));
const isSortingAsc = computed(
  () => selfSortingIsActive.value && props.sortingConfig[props.modelValue].order === 'asc'
);
const toggleSort = () => {
  const [sortAsc, sortDesc] = sortingKeys.value;
  emit('update:modelValue', props.modelValue === sortAsc ? sortDesc : sortAsc);
};
</script>

<template>
  <div
    class="flex cursor-pointer flex-col hover:text-content-100"
    @click="toggleSort"
    v-tooltip="$t(isSortingAsc ? 'shop.sort.desc' : 'shop.sort.asc')"
  >
    <OIcon
      v-if="!selfSortingIsActive || isSortingAsc"
      class="-my-1"
      icon="chevron-up"
      :size="selfSortingIsActive ? 'sm' : 'xs'"
    />
    <OIcon
      v-if="!selfSortingIsActive || !isSortingAsc"
      class="-my-1"
      icon="chevron-down"
      :size="selfSortingIsActive ? 'sm' : 'xs'"
    />
  </div>
</template>
