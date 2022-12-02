import type { DeepReadonly } from 'vue';
import type {
  Character,
  CharacterCharacteristics,
  EquippedItem,
  CharacterOverallItemsStats,
  EquippedItemsBySlot,
} from '@/models/character';

export const characterKey: InjectionKey<ComputedRef<Character>> = Symbol('Character');

export const characterCharacteristicsKey: InjectionKey<{
  characterCharacteristics: DeepReadonly<Ref<CharacterCharacteristics>>;
  setCharacterCharacteristics: (payload: CharacterCharacteristics) => void;
}> = Symbol('CharacterCharacteristics');

export const characterItemsKey: InjectionKey<{
  characterItems: DeepReadonly<Ref<EquippedItem[]>>;
  loadCharacterItems: (delay: number, payload: { id: number }) => Promise<EquippedItem[]>;
}> = Symbol('CharacterItems');

export const characterHealthPointsKey: InjectionKey<ComputedRef<number>> =
  Symbol('CharacterHealthPoints');

export const characterItemsStatsKey: InjectionKey<ComputedRef<CharacterOverallItemsStats>> =
  Symbol('CharacterItemsStats');

export const equippedItemsBySlotKey: InjectionKey<ComputedRef<EquippedItemsBySlot>> =
  Symbol('EquippedItemsBySlot');
