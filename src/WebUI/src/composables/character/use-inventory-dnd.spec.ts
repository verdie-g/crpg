// TODO: FIXME: MOCK SERVICES!!
import { type UserItem, type UserItemsBySlot } from '@/models/user';
import { ItemSlot, ItemType } from '@/models/item';

const mockEmit = vi.fn();

vi.mock('vue', async () => ({
  ...(await vi.importActual<typeof import('vue')>('vue')),
  getCurrentInstance: vi.fn().mockImplementation(() => ({
    emit: mockEmit,
  })),
}));

import { useInventoryDnD } from './use-inventory-dnd';

const userItemsBySlot = {
  [ItemSlot.Head]: {
    id: 4,
    baseItem: {
      type: 'HeadArmor',
    },
  },
  [ItemSlot.Weapon0]: {
    id: 3,
    baseItem: {
      type: 'OneHandedWeapon',
    },
  },
};

describe('useInventoryDnD', () => {
  it.each<
    [
      // payload
      UserItem | null, // item
      ItemSlot | null, // slot
      // expectation
      [
        number | null, // activeItemID
        ItemSlot[], // availableSlots
        ItemSlot | null // fromSlot
      ]
    ]
  >(
    // prettier-ignore
    [
      [
        null,
        null,
        [null, [], null]
      ],
      [
        { id: 1, baseItem: { type: ItemType.TwoHandedWeapon } } as UserItem,
        null,
        [1, [ItemSlot.Weapon0, ItemSlot.Weapon1, ItemSlot.Weapon2, ItemSlot.Weapon3], null],
      ],
      [
        { id: 1, baseItem: { type: ItemType.TwoHandedWeapon } } as UserItem,
        ItemSlot.Weapon1,
        [
          1,
          [ItemSlot.Weapon0, ItemSlot.Weapon1, ItemSlot.Weapon2, ItemSlot.Weapon3],
          ItemSlot.Weapon1,
        ],
      ],
      [
        { id: 2, baseItem: { type: ItemType.BodyArmor } } as UserItem,
        ItemSlot.Body,
        [2, [ItemSlot.Body], ItemSlot.Body],
      ],
      [
        { id: 42, baseItem: { type: ItemType.Mount } } as UserItem,
        null,
        [42, [ItemSlot.Mount], null],
      ],
  ]
  )('onDragStart - ', (userItem, slot, expectation) => {
    const { focusedItemId, availableSlots, fromSlot, toSlot, onDragStart } = useInventoryDnD(
      ref(userItemsBySlot as UserItemsBySlot)
    );

    expect(focusedItemId.value).toBeNull();
    expect(availableSlots.value).toEqual([]);
    expect(fromSlot.value).toBeNull();
    expect(toSlot.value).toBeNull();

    onDragStart(userItem, slot);

    const [expFocusedItemId, expAvailableSlots, expFromSlot] = expectation;

    expect(focusedItemId.value).toEqual(expFocusedItemId);
    expect(availableSlots.value).toEqual(expAvailableSlots);
    expect(fromSlot.value).toEqual(expFromSlot);
  });

  it('onDragEnter', () => {
    const { toSlot, onDragEnter } = useInventoryDnD(ref(userItemsBySlot as UserItemsBySlot));

    expect(toSlot.value).toBeNull();

    onDragEnter(ItemSlot.Mount);

    expect(toSlot.value).toEqual(ItemSlot.Mount);
  });

  it('onDragLeave', () => {
    const { toSlot, onDragEnter, onDragLeave } = useInventoryDnD(
      ref(userItemsBySlot as UserItemsBySlot)
    );

    onDragEnter(ItemSlot.Mount);

    expect(toSlot.value).toEqual(ItemSlot.Mount);

    onDragLeave();

    expect(toSlot.value).toBeNull();
  });

  describe('onDragEnd', () => {
    it('empty slot', () => {
      const { toSlot, onDragEnter, onDragEnd } = useInventoryDnD(
        ref(userItemsBySlot as UserItemsBySlot)
      );

      onDragEnter(ItemSlot.Mount);

      expect(toSlot.value).toEqual(ItemSlot.Mount);

      onDragEnd();

      expect(mockEmit).not.toBeCalled();
      expect(toSlot.value).toBeNull();
    });

    it('empty slot, with toSlot', () => {
      const { onDragEnd } = useInventoryDnD(ref(userItemsBySlot as UserItemsBySlot));

      onDragEnd();

      expect(mockEmit).not.toBeCalled();
    });

    it('with slot, empty toSlot - drag item outside = unEquip item', () => {
      const { onDragEnd } = useInventoryDnD(ref(userItemsBySlot as UserItemsBySlot));

      onDragEnd(null, ItemSlot.Mount);
      expect(mockEmit).toBeCalledWith('change', [{ userItemId: null, slot: ItemSlot.Mount }]);
    });
  });

  describe('onDrop', () => {
    it('to empty, not available slots', () => {
      const { onDragStart, onDrop } = useInventoryDnD(ref(userItemsBySlot as UserItemsBySlot));

      onDragStart({ id: 1, baseItem: { type: ItemType.Mount } } as UserItem);

      onDrop(ItemSlot.MountHarness);

      expect(mockEmit).not.toBeCalled();
    });

    it('to empty slot, available slot', () => {
      const { onDragStart, onDrop } = useInventoryDnD(ref(userItemsBySlot as UserItemsBySlot));

      onDragStart({ id: 1, baseItem: { type: ItemType.Mount } } as UserItem);

      onDrop(ItemSlot.Mount);

      expect(mockEmit).toBeCalledWith('change', [{ userItemId: 1, slot: ItemSlot.Mount }]);
    });

    it('to not empty, available slot', () => {
      const { onDragStart, onDrop } = useInventoryDnD(ref(userItemsBySlot as UserItemsBySlot));

      onDragStart({ id: 12, baseItem: { type: ItemType.HeadArmor } } as UserItem);

      onDrop(ItemSlot.Head);

      expect(mockEmit).toBeCalledWith('change', [{ userItemId: 12, slot: ItemSlot.Head }]);
    });

    it('swap items - drop item from ItemSlot.Weapon1 to ItemSlot.Weapon0', () => {
      const { onDragStart, onDrop } = useInventoryDnD(
        ref({
          [ItemSlot.Weapon0]: {
            id: 1,
            baseItem: {
              type: 'OneHandedWeapon',
            },
          },
          [ItemSlot.Weapon1]: {
            id: 2,
            baseItem: {
              type: 'TwoHandedWeapon',
            },
          },
        } as UserItemsBySlot)
      );

      onDragStart(
        { id: 2, baseItem: { type: ItemType.TwoHandedWeapon } } as UserItem,
        ItemSlot.Weapon1
      );

      onDrop(ItemSlot.Weapon0);

      expect(mockEmit).toBeCalledWith('change', [
        { userItemId: 2, slot: ItemSlot.Weapon0 },
        { userItemId: 1, slot: ItemSlot.Weapon1 },
      ]);
    });
  });
});
