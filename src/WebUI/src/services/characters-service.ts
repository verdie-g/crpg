import EquippedItem from '@/models/equipped-item';
import { applyPolynomialFunction } from '@/utils/math';
import Constants from '../../../../data/constants.json';

// TODO: handle upgrade items.
export function computeAverageRepairCost(equippedItems: EquippedItem[]): number {
  return equippedItems.reduce(
    (cost, ei) =>
      cost +
      applyPolynomialFunction(ei.userItem.baseItem.price, Constants.itemRepairCostCoefs) *
        Constants.itemBreakChance,
    0
  );
}
