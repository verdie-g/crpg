<script setup lang="ts">
import { UseElementBounding as ElementBounding } from '@vueuse/components';
import type { CharacterOverallItemsStats, EquippedItem, EquippedItemId } from '@/models/character';
import { ItemSlot } from '@/models/item';
import { getCharacterSLotsSchema, getOverallArmorValueBySlot } from '@/services/characters-service';
import { useInventoryDnD } from '@/composables/character/use-inventory-dnd';
import { useItemDetail } from '@/composables/character/use-item-detail';
import { equippedItemsBySlotKey, characterItemsStatsKey } from '@/symbols/character';

const equippedItemsBySlot = injectStrict(equippedItemsBySlotKey);
const itemsStats = injectStrict(characterItemsStatsKey);

const slotsSchema = getCharacterSLotsSchema();

const emit = defineEmits<{
  (e: 'change', items: EquippedItemId[]): void;
  (e: 'sell', itemId: number): void;
}>();

const onUnEquipItem = (slot: ItemSlot) => {
  console.log('onUnEquipItem');
  emit('change', [{ userItemId: null, slot }]);
};

const {
  focusedItemId,
  availableSlots,
  fromSlot,
  toSlot,
  onDragStart,
  onDragEnd,
  onDragEnter,
  onDragLeave,
  onDrop,
} = useInventoryDnD(equippedItemsBySlot);

const { openItemDetail } = useItemDetail();
</script>

<template>
  <div class="relative grid grid-cols-3 gap-4">
    <div class="absolute inset-0 -z-10 flex items-end justify-center">
      <!-- TODO: new img -->
      <img
        class="w-52 2xl:w-64"
        :src="getAssetUrl('themes/oruga-tailwind/img/body-silhouette.svg')"
      />
    </div>
    <div
      v-for="(slotGroup, idx) in slotsSchema"
      class="flex flex-col gap-3"
      :class="[{ 'z-20': idx === 0 }, { 'z-10 justify-end': idx === 1 }]"
    >
      <ElementBounding v-for="slot in slotGroup" :key="slot.key" v-slot="{ x, y, width }">
        <CharacterInventoryDollSlot
          :slot="slot.key"
          :placeholder="slot.placeholderIcon"
          :item="equippedItemsBySlot[slot.key]"
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
          @click="
            equippedItemsBySlot[slot.key] !== undefined &&
              openItemDetail({
                id: equippedItemsBySlot[slot.key].baseItem.id,
                userId: equippedItemsBySlot[slot.key].id,
                bound: { x, y, width },
              })
          "
        />
      </ElementBounding>
    </div>
  </div>
</template>
