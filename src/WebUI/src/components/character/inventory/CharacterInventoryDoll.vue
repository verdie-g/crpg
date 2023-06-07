<script setup lang="ts">
import type { EquippedItemId } from '@/models/character';
import { ItemSlot } from '@/models/item';
import {
  getCharacterSLotsSchema,
  getOverallArmorValueBySlot,
  validateItemNotMeetRequirement,
} from '@/services/characters-service';
import { useInventoryDnD } from '@/composables/character/use-inventory-dnd';
import { useItemDetail } from '@/composables/character/use-item-detail';
import {
  equippedItemsBySlotKey,
  characterItemsStatsKey,
  characterCharacteristicsKey,
} from '@/symbols/character';

const equippedItemsBySlot = injectStrict(equippedItemsBySlotKey);
const itemsStats = injectStrict(characterItemsStatsKey);
const { characterCharacteristics } = injectStrict(characterCharacteristicsKey);

const slotsSchema = getCharacterSLotsSchema();

const emit = defineEmits<{
  (e: 'change', items: EquippedItemId[]): void;
  (e: 'sell', itemId: number): void;
}>();

const onUnEquipItem = (slot: ItemSlot) => {
  emit('change', [{ userItemId: null, slot }]);
};

const {
  availableSlots,
  fromSlot,
  toSlot,
  onDragStart,
  onDragEnd,
  onDragEnter,
  onDragLeave,
  onDrop,
} = useInventoryDnD(equippedItemsBySlot);

const { openItemDetail, getElementBounds } = useItemDetail();
</script>

<template>
  <div class="relative grid grid-cols-3 gap-4">
    <div class="absolute inset-0 -z-10 flex items-end justify-center">
      <!-- TODO: new img -->
      <SvgSpriteImg name="body-silhouette" viewBox="0 0 970 2200" class="w-52 2xl:w-64" />
    </div>
    <div
      v-for="(slotGroup, idx) in slotsSchema"
      class="flex flex-col gap-3"
      :class="[{ 'z-20': idx === 0 }, { 'z-10 justify-end': idx === 1 }]"
    >
      <CharacterInventoryDollSlot
        v-for="slot in slotGroup"
        :key="slot.key"
        :slot="slot.key"
        :placeholder="slot.placeholderIcon"
        :userItem="equippedItemsBySlot[slot.key]"
        :notMeetRequirement="
          slot.key in equippedItemsBySlot &&
          validateItemNotMeetRequirement(
            equippedItemsBySlot[slot.key].item,
            characterCharacteristics
          )
        "
        :available="Boolean(availableSlots.length && availableSlots.includes(slot.key))"
        :focused="toSlot === slot.key && availableSlots.includes(slot.key)"
        :armorOverall="getOverallArmorValueBySlot(slot.key, itemsStats)"
        :invalid="
          Boolean(
            availableSlots.length && toSlot === slot.key && !availableSlots.includes(slot.key)
          )
        "
        :remove="fromSlot === slot.key && !toSlot"
        @dragend="(_e:DragEvent) => onDragEnd(_e, slot.key)"
        @drop="onDrop(slot.key)"
        @dragover.prevent="onDragEnter(slot.key)"
        @dragleave.prevent="onDragLeave"
        @dragenter.prevent
        @dragstart="onDragStart(equippedItemsBySlot[slot.key], slot.key)"
        @unEquip="onUnEquipItem(slot.key)"
        @click="e=>
            equippedItemsBySlot[slot.key] !== undefined &&
              openItemDetail({
                id: equippedItemsBySlot[slot.key].item.id,
                userId: equippedItemsBySlot[slot.key].id,
                bound: getElementBounds(e.target as HTMLElement),
              })
          "
      />
    </div>
  </div>
</template>
