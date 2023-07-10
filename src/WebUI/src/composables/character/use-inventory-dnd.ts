import type { EquippedItemsBySlot, EquippedItemId } from '@/models/character';
import { ItemSlot } from '@/models/item';
import { type UserItem } from '@/models/user';
import { getAvailableSlotsByItem, getLinkedSlots } from '@/services/item-service';
import { notify, NotificationType } from '@/services/notification-service';
import { t } from '@/services/translate-service';

// Shared state
const focusedItemId = ref<number | null>(null);
const availableSlots = ref<ItemSlot[]>([]);
const fromSlot = ref<ItemSlot | null>(null);
const toSlot = ref<ItemSlot | null>(null);

export const useInventoryDnD = (equippedItemsBySlot: Ref<EquippedItemsBySlot>) => {
  const { emit } = getCurrentInstance() as NonNullable<ReturnType<typeof getCurrentInstance>>;

  const onDragStart = (item: UserItem | null = null, slot: ItemSlot | null = null) => {
    if (!item) return;

    // TODO: to service
    if (item.isBroken) {
      notify(t('character.inventory.item.broken.notify.warning'), NotificationType.Warning);
      return;
    }

    focusedItemId.value = item.id;
    availableSlots.value = getAvailableSlotsByItem(item.item, equippedItemsBySlot.value);

    if (slot) {
      fromSlot.value = slot;
    }
  };

  const onDragEnter = (slot: ItemSlot) => {
    toSlot.value = slot;
  };

  const onDragLeave = () => {
    toSlot.value = null;
  };

  const onDragEnd = (_e: DragEvent | null = null, slot: ItemSlot | null = null) => {
    if (slot && !toSlot.value) {
      const items: EquippedItemId[] = [
        { userItemId: null, slot },
        ...getLinkedSlots(slot, equippedItemsBySlot.value).map(ls => ({
          userItemId: null,
          slot: ls,
        })),
      ];

      emit('change', items); // drop outside
    }

    focusedItemId.value = null;
    availableSlots.value = [];
    fromSlot.value = null;
    toSlot.value = null;
  };

  const onDrop = (slot: ItemSlot) => {
    if (!availableSlots.value.includes(slot)) return;

    const items: EquippedItemId[] = [{ userItemId: focusedItemId.value, slot }];

    // switch items (weapon)
    if (fromSlot.value) {
      items.push({
        userItemId:
          equippedItemsBySlot.value[slot] !== undefined ? equippedItemsBySlot.value[slot].id : null,
        slot: fromSlot.value,
      });
    }

    emit('change', items);
  };

  return {
    focusedItemId: readonly(focusedItemId),
    availableSlots: readonly(availableSlots),
    fromSlot: readonly(fromSlot),
    toSlot: readonly(toSlot),
    onDragStart,
    onDragEnd,
    onDragEnter,
    onDragLeave,
    onDrop,
  };
};
