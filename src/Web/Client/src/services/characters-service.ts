import Character from '@/models/character';
import ItemSlot from '@/models/item-slot';
import Item from '@/models/item';

export function getCharacterItemFromSlot(character: Character, slot: ItemSlot) : Item | null {
  switch (slot) {
    case ItemSlot.Head:
      return character.headItem;
    case ItemSlot.Cape:
      return character.capeItem;
    case ItemSlot.Body:
      return character.bodyItem;
    case ItemSlot.Hand:
      return character.handItem;
    case ItemSlot.Leg:
      return character.legItem;
    case ItemSlot.HorseHarness:
      return character.horseHarnessItem;
    case ItemSlot.Horse:
      return character.horseItem;
    case ItemSlot.Weapon1:
      return character.weapon1Item;
    case ItemSlot.Weapon2:
      return character.weapon2Item;
    case ItemSlot.Weapon3:
      return character.weapon3Item;
    case ItemSlot.Weapon4:
      return character.weapon4Item;
    default:
      return null;
  }
}

export function setCharacterItem(character: Character, item: Item, slot: ItemSlot) {
  switch (slot) {
    case ItemSlot.Head:
      character.headItem = item;
      break;
    case ItemSlot.Cape:
      character.capeItem = item;
      break;
    case ItemSlot.Body:
      character.bodyItem = item;
      break;
    case ItemSlot.Hand:
      character.handItem = item;
      break;
    case ItemSlot.Leg:
      character.legItem = item;
      break;
    case ItemSlot.HorseHarness:
      character.horseHarnessItem = item;
      break;
    case ItemSlot.Horse:
      character.horseItem = item;
      break;
    case ItemSlot.Weapon1:
      character.weapon1Item = item;
      break;
    case ItemSlot.Weapon2:
      character.weapon2Item = item;
      break;
    case ItemSlot.Weapon3:
      character.weapon3Item = item;
      break;
    case ItemSlot.Weapon4:
      character.weapon4Item = item;
      break;
    default:
      break;
  }
}
