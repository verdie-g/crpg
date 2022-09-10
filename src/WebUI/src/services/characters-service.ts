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

export function computeSalePrice(item: UserItem): number {
  const salePrice = applyPolynomialFunction(item.baseItem.price, Constants.itemSellCostCoefs);
  // Floor salePrice to match behaviour of backend int typecast
  return Math.floor(salePrice);
}
