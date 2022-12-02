<script setup lang="ts">
import { WeaponClass, type ItemFlat, ItemType } from '@/models/item';
import {
  humanizeBucket, // TODO: FIXME:
  itemTypeByWeaponClass,
  hasWeaponClassesByItemType,
} from '@/services/item-service';

const props = defineProps<{
  itemType: ItemType;
  weaponClass: WeaponClass | null;
  itemTypeBuckets: itemsjs.Buckets<ItemFlat>;
  weaponClassBuckets: itemsjs.Buckets<ItemFlat>;
}>();

const emit = defineEmits<{
  (e: 'update:itemType', val: ItemType): void;
  (e: 'update:weaponClass', val: WeaponClass | null): void;
}>();

const itemTypeModel = computed({
  set(val: ItemType) {
    emit('update:itemType', val);
  },

  get() {
    return props.itemType;
  },
});

const weaponClassModel = computed({
  set(val: WeaponClass | null) {
    emit('update:weaponClass', val);
  },

  get() {
    return props.weaponClass;
  },
});

const subLevelActive = computed(
  () => hasWeaponClassesByItemType(itemTypeModel.value) && weaponClassModel.value !== null
);
</script>

<template>
  <OTabs
    v-model="itemTypeModel"
    :type="subLevelActive ? 'fill-rounded-grouped' : 'fill-rounded'"
    :animated="false"
  >
    <OTabItem v-for="bucket in itemTypeBuckets" :value="bucket.key" tag="div">
      <template #header>
        <OIcon
          :icon="humanizeBucket('type', bucket.key)!.icon!.name"
          size="2xl"
          v-tooltip.bottom="humanizeBucket('type', bucket.key)!.label"
        />

        <template
          v-if="subLevelActive && weaponClassModel !== null && (bucket.key as ItemType) === itemTypeByWeaponClass[weaponClassModel]"
        >
          <OIcon icon="chevron-right" size="lg" class="text-content-400" />

          <OTabs
            v-model="weaponClassModel"
            type="flat-rounded"
            contentClass="hidden"
            :animated="false"
          >
            <OTabItem v-for="bucket in weaponClassBuckets" :value="bucket.key">
              <template #header>
                <OIcon
                  :icon="humanizeBucket('weaponClass', bucket.key)!.icon!.name"
                  size="2xl"
                  v-tooltip.bottom="humanizeBucket('weaponClass', bucket.key)!.label"
                />
              </template>
            </OTabItem>
          </OTabs>
        </template>
      </template>
    </OTabItem>
  </OTabs>
</template>
