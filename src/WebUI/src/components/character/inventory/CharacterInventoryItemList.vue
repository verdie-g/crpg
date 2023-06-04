<script setup lang="ts">
import { type UserItem } from '@/models/user';
import { useInventoryDnD } from '@/composables/character/use-inventory-dnd';
import { useItemDetail } from '@/composables/character/use-item-detail';
import { validateItemNotMeetRequirement } from '@/services/characters-service';
import { getItemGraceTimeEnd, isGraceTimeExpired } from '@/services/item-service';
import { equippedItemsBySlotKey, characterCharacteristicsKey } from '@/symbols/character';

const props = defineProps<{
  items: UserItem[];
  equippedItemsIds: number[];
}>();

const equippedItemsBySlot = injectStrict(equippedItemsBySlotKey);
const { characterCharacteristics } = injectStrict(characterCharacteristicsKey);

const emit = defineEmits<{
  (e: 'sell', itemId: number): void;
}>();

const { onDragStart, onDragEnd } = useInventoryDnD(equippedItemsBySlot);
const { openItemDetail, closeItemDetail, getElementBounds } = useItemDetail();

const onSellItem = (item: UserItem) => {
  emit('sell', item.id);
  closeItemDetail(item.item.id);
};
</script>

<template>
  <div class="grid grid-cols-3 gap-2 2xl:grid-cols-4" style="grid-area: items">
    <CharacterInventoryItemCard
      v-for="userItem in items"
      :key="userItem.id"
      class="h-20"
      :userItem="userItem"
      :equipped="equippedItemsIds.includes(userItem.id)"
      :notMeetRequirement="validateItemNotMeetRequirement(userItem.item, characterCharacteristics)"
      :isNew="!isGraceTimeExpired(getItemGraceTimeEnd(userItem))"
      @dragstart="onDragStart(userItem)"
      @dragend="onDragEnd"
      @sell="onSellItem(userItem)"
      @click="
        e =>
          openItemDetail({
            id: userItem.item.id,
            userId: userItem.id,
            bound: getElementBounds(e.target as HTMLElement),
          })
      "
    />
  </div>
</template>
