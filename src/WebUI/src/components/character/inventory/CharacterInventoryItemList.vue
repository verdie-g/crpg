<script setup lang="ts">
import { UseElementBounding as ElementBounding } from '@vueuse/components';
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
const { openItemDetail, closeItemDetail } = useItemDetail();

const onSellItem = (item: UserItem) => {
  emit('sell', item.id);
  closeItemDetail(item.baseItem.id);
};
</script>

<template>
  <div class="grid grid-cols-3 gap-2 2xl:grid-cols-4" style="grid-area: items">
    <ElementBounding v-for="item in items" :key="item.id" v-slot="{ x, y, width }">
      <CharacterInventoryItemCard
        class="h-20"
        :item="item"
        :equipped="equippedItemsIds.includes(item.id)"
        :notMeetRequirement="
          validateItemNotMeetRequirement(item.baseItem, characterCharacteristics)
        "
        :isNew="!isGraceTimeExpired(getItemGraceTimeEnd(item))"
        @dragstart="onDragStart(item)"
        @dragend="onDragEnd"
        @sell="onSellItem(item)"
        @click="openItemDetail({ id: item.baseItem.id, userId: item.id, bound: { x, y, width } })"
      />
    </ElementBounding>
  </div>
</template>
