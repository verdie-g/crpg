import EquippedItem from '@/models/equipped-item';

// This const should be synced with the Web API.
const ITEM_REPAIR_COST = 0.07;
const ITEM_BREAK_CHANCE = 0.08;

export function computeAverageRepairCost(equippedItems: EquippedItem[]): number {
  return equippedItems.reduce((cost, ei) => cost + ITEM_REPAIR_COST * ITEM_BREAK_CHANCE * ei.item.value, 0);
}
