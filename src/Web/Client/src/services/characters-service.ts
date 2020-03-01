import Character from '@/models/character';
import ItemSlot from '@/models/item-slot';
import Item from '@/models/item';

class CharacterItemSlot {
  public getItem: (character: Character) => Item | null;
  public setItem: (character: Character, item: Item) => void;
}

const characterItemSlots: Record<ItemSlot, CharacterItemSlot> = {
  [ItemSlot.Head]: {
    getItem: character => character.headItem,
    setItem: (character, item) => character.headItem = item,
  },
  [ItemSlot.Cape]: {
    getItem: character => character.capeItem,
    setItem: (character, item) => character.capeItem = item,
  },
  [ItemSlot.Body]: {
    getItem: character => character.bodyItem,
    setItem: (character, item) => character.bodyItem = item,
  },
  [ItemSlot.Hand]: {
    getItem: character => character.handItem,
    setItem: (character, item) => character.handItem = item,
  },
  [ItemSlot.Leg]: {
    getItem: character => character.legItem,
    setItem: (character, item) => character.legItem = item,
  },
  [ItemSlot.HorseHarness]: {
    getItem: character => character.horseHarnessItem,
    setItem: (character, item) => character.horseHarnessItem = item,
  },
  [ItemSlot.Horse]: {
    getItem: character => character.horseItem,
    setItem: (character, item) => character.horseItem = item,
  },
  [ItemSlot.Weapon1]: {
    getItem: character => character.weapon1Item,
    setItem: (character, item) => character.weapon1Item = item,
  },
  [ItemSlot.Weapon2]: {
    getItem: character => character.weapon2Item,
    setItem: (character, item) => character.weapon2Item = item,
  },
  [ItemSlot.Weapon3]: {
    getItem: character => character.weapon3Item,
    setItem: (character, item) => character.weapon3Item = item,
  },
  [ItemSlot.Weapon4]: {
    getItem: character => character.weapon4Item,
    setItem: (character, item) => character.weapon4Item = item,
  },
};

export function getCharacterItemFromSlot(character: Character, slot: ItemSlot) : Item | null {
  return characterItemSlots[slot].getItem(character);
}

export function setCharacterItem(character: Character, slot: ItemSlot, item: Item) {
  return characterItemSlots[slot].setItem(character, item);
}
