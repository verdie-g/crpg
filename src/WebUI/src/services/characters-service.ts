import ItemSlot from '@/models/item-slot';
import Item from '@/models/item';
import CharacterItems from '@/models/character-items';
import * as userService from '@/services/users-service';

// This const should be synced with the Web API.
const ITEM_REPAIR_COST = 0.07;
const ITEM_BREAK_CHANCE = 0.08;

class CharacterItemSlot {
  public getItem: (characterItems: CharacterItems) => Item | null;
  public setItem: (characterItems: CharacterItems, item: Item | null) => void;
}

const characterItemSlots: Record<ItemSlot, CharacterItemSlot> = {
  [ItemSlot.Head]: {
    getItem: characterItems => characterItems.headItem,
    setItem: (characterItems, item) => characterItems.headItem = item,
  },
  [ItemSlot.Shoulder]: {
    getItem: characterItems => characterItems.shoulderItem,
    setItem: (characterItems, item) => characterItems.shoulderItem = item,
  },
  [ItemSlot.Body]: {
    getItem: characterItems => characterItems.bodyItem,
    setItem: (characterItems, item) => characterItems.bodyItem = item,
  },
  [ItemSlot.Hand]: {
    getItem: characterItems => characterItems.handItem,
    setItem: (characterItems, item) => characterItems.handItem = item,
  },
  [ItemSlot.Leg]: {
    getItem: characterItems => characterItems.legItem,
    setItem: (characterItems, item) => characterItems.legItem = item,
  },
  [ItemSlot.MountHarness]: {
    getItem: characterItems => characterItems.mountHarnessItem,
    setItem: (characterItems, item) => characterItems.mountHarnessItem = item,
  },
  [ItemSlot.Mount]: {
    getItem: characterItems => characterItems.mountItem,
    setItem: (characterItems, item) => characterItems.mountItem = item,
  },
  [ItemSlot.Weapon1]: {
    getItem: characterItems => characterItems.weapon1Item,
    setItem: (characterItems, item) => characterItems.weapon1Item = item,
  },
  [ItemSlot.Weapon2]: {
    getItem: characterItems => characterItems.weapon2Item,
    setItem: (characterItems, item) => characterItems.weapon2Item = item,
  },
  [ItemSlot.Weapon3]: {
    getItem: characterItems => characterItems.weapon3Item,
    setItem: (characterItems, item) => characterItems.weapon3Item = item,
  },
  [ItemSlot.Weapon4]: {
    getItem: characterItems => characterItems.weapon4Item,
    setItem: (characterItems, item) => characterItems.weapon4Item = item,
  },
};

function listCharacterItems(items: CharacterItems): Item[] {
  return [
    items.headItem!,
    items.shoulderItem!,
    items.bodyItem!,
    items.handItem!,
    items.legItem!,
    items.mountHarnessItem!,
    items.mountItem!,
    items.weapon1Item!,
    items.weapon2Item!,
    items.weapon3Item!,
    items.weapon4Item!,
  ].filter(i => i !== null);
}

export function getCharacterItemFromSlot(characterItems: CharacterItems, slot: ItemSlot): Item | null {
  return characterItemSlots[slot].getItem(characterItems);
}

export function setCharacterItem(characterItems: CharacterItems, slot: ItemSlot, item: Item | null): void {
  return characterItemSlots[slot].setItem(characterItems, item);
}

export function updateCharacterItems(characterId: number, items: CharacterItems): Promise<CharacterItems> {
  return userService.updateCharacterItems(characterId, {
    headItemId: items.headItem?.id ?? null,
    shoulderItemId: items.shoulderItem?.id ?? null,
    bodyItemId: items.bodyItem?.id ?? null,
    handItemId: items.handItem?.id ?? null,
    legItemId: items.legItem?.id ?? null,
    mountHarnessItemId: items.mountHarnessItem?.id ?? null,
    mountItemId: items.mountItem?.id ?? null,
    weapon1ItemId: items.weapon1Item?.id ?? null,
    weapon2ItemId: items.weapon2Item?.id ?? null,
    weapon3ItemId: items.weapon3Item?.id ?? null,
    weapon4ItemId: items.weapon4Item?.id ?? null,
    autoRepair: items.autoRepair,
  });
}

export function computeAverageRepairCost(items: CharacterItems): number {
  return listCharacterItems(items).reduce((cost, item) => cost + ITEM_REPAIR_COST * ITEM_BREAK_CHANCE * item.value, 0);
}
