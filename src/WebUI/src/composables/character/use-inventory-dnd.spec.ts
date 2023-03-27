import { type PartialDeep } from 'type-fest';
import { type UserItem, type UserItemsBySlot } from '@/models/user';
import { ItemSlot, ItemType } from '@/models/item';
import { type CharacterCharacteristics } from '@/models/character';

const mockEmit = vi.fn();
vi.mock('vue', async () => ({
  ...(await vi.importActual<typeof import('vue')>('vue')),
  getCurrentInstance: vi.fn().mockImplementation(() => ({
    emit: mockEmit,
  })),
}));

const mockGetAvailableSlotsByItem = vi.fn().mockReturnValue([]);
vi.mock('@/services/item-service', () => {
  return {
    getAvailableSlotsByItem: mockGetAvailableSlotsByItem,
  };
});

const mockValidateItemNotMeetRequirement = vi.fn().mockReturnValue(false);
vi.mock('@/services/characters-service', () => {
  return {
    validateItemNotMeetRequirement: mockValidateItemNotMeetRequirement,
  };
});

const mockNotify = vi.fn();
vi.mock('@/services/notification-service', async () => {
  return {
    ...(await vi.importActual<typeof import('@/services/notification-service')>(
      '@/services/notification-service'
    )),
    notify: mockNotify,
  };
});

import { useInventoryDnD } from './use-inventory-dnd';

const userItemsBySlot: PartialDeep<UserItemsBySlot> = {
  [ItemSlot.Head]: {
    id: 4,
    baseItem: {
      type: ItemType.HeadArmor,
    },
  },
  [ItemSlot.Weapon0]: {
    id: 3,
    baseItem: {
      type: ItemType.OneHandedWeapon,
    },
  },
};

const characterCharacteristics: PartialDeep<CharacterCharacteristics> = {
  attributes: {
    strength: 12,
  },
};

describe('useInventoryDnD', () => {
  describe('onDragStart', () => {
    it('no item', () => {
      const { focusedItemId, availableSlots, fromSlot, onDragStart, onDragEnd } = useInventoryDnD(
        ref(userItemsBySlot as UserItemsBySlot),
        ref(characterCharacteristics as CharacterCharacteristics)
      );

      onDragStart(null, null);

      expect(focusedItemId.value).toEqual(null);
      expect(availableSlots.value).toEqual([]);
      expect(fromSlot.value).toEqual(null);

      onDragEnd(); // reset shared state
    });

    it('weapon: from inventory, to doll', () => {
      const userItem: PartialDeep<UserItem> = {
        id: 1,
        baseItem: { type: ItemType.TwoHandedWeapon, flags: [] },
      };

      const AVAILABLE_SLOTS = [
        ItemSlot.Weapon0,
        ItemSlot.Weapon1,
        ItemSlot.Weapon2,
        ItemSlot.Weapon3,
      ];

      mockGetAvailableSlotsByItem.mockReturnValue(AVAILABLE_SLOTS);

      const { focusedItemId, availableSlots, fromSlot, onDragStart, onDragEnd } = useInventoryDnD(
        ref(userItemsBySlot as UserItemsBySlot),
        ref(characterCharacteristics as CharacterCharacteristics)
      );

      onDragStart(userItem as UserItem, ItemSlot.Weapon1);

      expect(focusedItemId.value).toEqual(1);
      expect(availableSlots.value).toEqual(AVAILABLE_SLOTS);
      expect(fromSlot.value).toEqual(ItemSlot.Weapon1);

      onDragEnd();
    });

    it('weapon: from doll, to doll (another slot)', () => {
      const userItem: PartialDeep<UserItem> = {
        id: 1,
        baseItem: { type: ItemType.TwoHandedWeapon, flags: [] },
      };

      const AVAILABLE_SLOTS = [
        ItemSlot.Weapon0,
        ItemSlot.Weapon1,
        ItemSlot.Weapon2,
        ItemSlot.Weapon3,
      ];

      mockGetAvailableSlotsByItem.mockReturnValue(AVAILABLE_SLOTS);

      const { focusedItemId, availableSlots, fromSlot, onDragStart, onDragEnd } = useInventoryDnD(
        ref(userItemsBySlot as UserItemsBySlot),
        ref(characterCharacteristics as CharacterCharacteristics)
      );

      onDragStart(userItem as UserItem, null);

      expect(focusedItemId.value).toEqual(1);
      expect(availableSlots.value).toEqual(AVAILABLE_SLOTS);
      expect(fromSlot.value).toEqual(null);

      onDragEnd();
    });

    it('broken item', () => {
      const userItem: PartialDeep<UserItem> = {
        id: 42,
        rank: -1,
        baseItem: { type: ItemType.HeadArmor, flags: [] },
      };

      const { focusedItemId, availableSlots, fromSlot, onDragStart, onDragEnd } = useInventoryDnD(
        ref(userItemsBySlot as UserItemsBySlot),
        ref(characterCharacteristics as CharacterCharacteristics)
      );

      onDragStart(userItem as UserItem, ItemSlot.Head);

      expect(mockNotify).toBeCalledWith(
        'character.inventory.item.broken.notify.warning',
        'warning'
      );

      expect(focusedItemId.value).toEqual(null);
      expect(availableSlots.value).toEqual([]);
      expect(fromSlot.value).toEqual(null);

      onDragEnd();
    });

    it('requirements', () => {
      mockValidateItemNotMeetRequirement.mockResolvedValueOnce(true);

      const userItem: PartialDeep<UserItem> = {
        id: 42,
        baseItem: { type: ItemType.Crossbow, flags: [], requirement: 21 },
      };

      const { focusedItemId, availableSlots, fromSlot, onDragStart, onDragEnd } = useInventoryDnD(
        ref(userItemsBySlot as UserItemsBySlot),
        ref(characterCharacteristics as CharacterCharacteristics)
      );

      onDragStart(userItem as UserItem, ItemSlot.Weapon0);

      expect(mockNotify).toBeCalledWith(
        'character.inventory.item.requirement.notify.warning',
        'warning'
      );

      expect(focusedItemId.value).toEqual(null);
      expect(availableSlots.value).toEqual([]);
      expect(fromSlot.value).toEqual(null);

      onDragEnd();
    });
  });

  it('onDragEnter', () => {
    const { toSlot, onDragEnter } = useInventoryDnD(
      ref(userItemsBySlot as UserItemsBySlot),
      ref(characterCharacteristics as CharacterCharacteristics)
    );

    expect(toSlot.value).toBeNull();

    onDragEnter(ItemSlot.Mount);

    expect(toSlot.value).toEqual(ItemSlot.Mount);
  });

  it('onDragLeave', () => {
    const { toSlot, onDragEnter, onDragLeave } = useInventoryDnD(
      ref(userItemsBySlot as UserItemsBySlot),
      ref(characterCharacteristics as CharacterCharacteristics)
    );

    onDragEnter(ItemSlot.Mount);

    expect(toSlot.value).toEqual(ItemSlot.Mount);

    onDragLeave();

    expect(toSlot.value).toBeNull();
  });

  describe('onDragEnd', () => {
    it('empty slot', () => {
      const { toSlot, onDragEnter, onDragEnd } = useInventoryDnD(
        ref(userItemsBySlot as UserItemsBySlot),
        ref(characterCharacteristics as CharacterCharacteristics)
      );

      onDragEnter(ItemSlot.Mount);

      expect(toSlot.value).toEqual(ItemSlot.Mount);

      onDragEnd();

      expect(mockEmit).not.toBeCalled();
      expect(toSlot.value).toBeNull();
    });

    it('empty slot, with toSlot', () => {
      const { onDragEnd } = useInventoryDnD(
        ref(userItemsBySlot as UserItemsBySlot),
        ref(characterCharacteristics as CharacterCharacteristics)
      );

      onDragEnd();

      expect(mockEmit).not.toBeCalled();
    });

    it('with slot, empty toSlot - drag item outside = unEquip item', () => {
      const { onDragEnd } = useInventoryDnD(
        ref(userItemsBySlot as UserItemsBySlot),
        ref(characterCharacteristics as CharacterCharacteristics)
      );

      onDragEnd(null, ItemSlot.Mount);
      expect(mockEmit).toBeCalledWith('change', [{ userItemId: null, slot: ItemSlot.Mount }]);
    });
  });

  describe('onDrop', () => {
    it('to empty, not available slots', () => {
      mockGetAvailableSlotsByItem.mockReturnValue([ItemSlot.Mount]);

      const userItem: PartialDeep<UserItem> = {
        id: 1,
        baseItem: { type: ItemType.Mount, flags: [] },
      };

      const { onDragStart, onDrop, onDragEnd } = useInventoryDnD(
        ref(userItemsBySlot as UserItemsBySlot),
        ref(characterCharacteristics as CharacterCharacteristics)
      );

      onDragStart(userItem as UserItem);

      onDrop(ItemSlot.MountHarness);

      expect(mockEmit).not.toBeCalled();

      onDragEnd();
    });

    it('to empty slot, available slot', () => {
      mockGetAvailableSlotsByItem.mockReturnValue([ItemSlot.Mount]);

      const userItem: PartialDeep<UserItem> = {
        id: 1,
        baseItem: { type: ItemType.Mount, flags: [] },
      };

      const { onDragStart, onDrop, onDragEnd } = useInventoryDnD(
        ref(userItemsBySlot as UserItemsBySlot),
        ref(characterCharacteristics as CharacterCharacteristics)
      );

      onDragStart(userItem as UserItem);

      onDrop(ItemSlot.Mount);

      expect(mockEmit).toBeCalledWith('change', [{ userItemId: 1, slot: ItemSlot.Mount }]);

      onDragEnd();
    });

    it('to full slot, available slot', () => {
      mockGetAvailableSlotsByItem.mockReturnValue([ItemSlot.Head]);

      const userItem: PartialDeep<UserItem> = {
        id: 1,
        baseItem: { type: ItemType.HeadArmor, flags: [] },
      };

      const { onDragStart, onDrop, onDragEnd } = useInventoryDnD(
        ref(userItemsBySlot as UserItemsBySlot),
        ref(characterCharacteristics as CharacterCharacteristics)
      );

      onDragStart(userItem as UserItem);

      onDrop(ItemSlot.Head);

      expect(mockEmit).toBeCalledWith('change', [{ userItemId: 1, slot: ItemSlot.Head }]);

      onDragEnd();
    });

    it('swap items - drop item from ItemSlot.Weapon1 to ItemSlot.Weapon0', () => {
      const AVAILABLE_SLOTS = [
        ItemSlot.Weapon0,
        ItemSlot.Weapon1,
        ItemSlot.Weapon2,
        ItemSlot.Weapon3,
      ];

      mockGetAvailableSlotsByItem.mockReturnValue(AVAILABLE_SLOTS);

      const userItem: PartialDeep<UserItem> = {
        id: 2,
        baseItem: { type: ItemType.TwoHandedWeapon, flags: [] },
      };

      const { onDragStart, onDrop, onDragEnd } = useInventoryDnD(
        ref({
          [ItemSlot.Weapon0]: {
            id: 1,
            baseItem: {
              type: ItemType.OneHandedWeapon,
            },
          },
          [ItemSlot.Weapon1]: {
            id: 2,
            baseItem: {
              type: ItemType.TwoHandedWeapon,
            },
          },
        } as UserItemsBySlot),
        ref(characterCharacteristics as CharacterCharacteristics)
      );

      onDragStart(userItem as UserItem, ItemSlot.Weapon1);

      onDrop(ItemSlot.Weapon0);

      expect(mockEmit).toBeCalledWith('change', [
        { userItemId: 2, slot: ItemSlot.Weapon0 },
        { userItemId: 1, slot: ItemSlot.Weapon1 },
      ]);

      onDragEnd();
    });
  });
});
