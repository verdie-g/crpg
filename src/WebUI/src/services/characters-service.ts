import EquippedItem from '@/models/equipped-item';
import { applyPolynomialFunction } from '@/utils/math';
import Constants from '../../../../data/constants.json';

export function computeAverageRepairCost(equippedItems: EquippedItem[]): number {
  return equippedItems.reduce(
    (cost, ei) =>
      cost +
      applyPolynomialFunction(ei.item.value, Constants.itemRepairCostCoefs) *
      Constants.itemBreakChance,
    0
  );
}
