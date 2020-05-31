import ItemSlot from '@/models/item-slot';
import Item from '@/models/item';
import CharacterItems from '@/models/character-items';

class CharacterItemSlot {
  public getItem: (characterItems: CharacterItems) => Item | null;
  public setItem: (characterItems: CharacterItems, item: Item | null) => void;
}

const characterItemSlots: Record<ItemSlot, CharacterItemSlot> = {
  [ItemSlot.Head]: {
    getItem: characterItems => characterItems.headItem,
    setItem: (characterItems, item) => characterItems.headItem = item,
  },
  [ItemSlot.Cape]: {
    getItem: characterItems => characterItems.capeItem,
    setItem: (characterItems, item) => characterItems.capeItem = item,
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
  [ItemSlot.HorseHarness]: {
    getItem: characterItems => characterItems.horseHarnessItem,
    setItem: (characterItems, item) => characterItems.horseHarnessItem = item,
  },
  [ItemSlot.Horse]: {
    getItem: characterItems => characterItems.horseItem,
    setItem: (characterItems, item) => characterItems.horseItem = item,
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

export function getCharacterItemFromSlot(characterItems: CharacterItems, slot: ItemSlot): Item | null {
  return characterItemSlots[slot].getItem(characterItems);
}

export function setCharacterItem(characterItems: CharacterItems, slot: ItemSlot, item: Item | null): void {
  return characterItemSlots[slot].setItem(characterItems, item);
}
