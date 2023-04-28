<script setup lang="ts">
import { type ItemFlat, ItemType } from '@/models/item';
import { humanizeBucket } from '@/services/item-service';

const props = defineProps<{
  modelValue: ItemType[];
  buckets: itemsjs.Buckets<ItemFlat[keyof ItemFlat]>;
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', type: ItemType[]): void;
}>();

const itemTypeModel = computed({
  set(value: ItemType) {
    if (value === ItemType.Undefined) {
      emit('update:modelValue', []);
      return;
    }

    emit('update:modelValue', [value]);
  },

  get() {
    if (props.modelValue.length === 0) {
      return ItemType.Undefined;
    }

    return props.modelValue[0];
  },
});
</script>

<template>
  <OTabs v-model="itemTypeModel" type="fill-rounded" vertical>
    <OTabItem :value="ItemType.Undefined">
      <template #header>
        <OIcon icon="grid" size="xl" v-tooltip.bottom="'All'" />
      </template>
    </OTabItem>
    <OTabItem v-for="bucket in props.buckets" :value="bucket.key">
      <template #header>
        <OIcon
          :icon="humanizeBucket('type', bucket.key)!.icon!.name"
          size="xl"
          v-tooltip.bottom="humanizeBucket('type', bucket.key)!.label"
        />
      </template>
    </OTabItem>
  </OTabs>
</template>
