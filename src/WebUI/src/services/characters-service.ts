import EquippedItem from '@/models/equipped-item';
import { applyPolynomialFunction } from '@/utils/math';
import Constants from '../../../../data/constants.json';

// TODO: handle upgrade items.
export function computeMaxRepairCost(equippedItems: EquippedItem[]): number {
  return equippedItems.reduce(
    (cost, ei) =>
      cost + applyPolynomialFunction(ei.userItem.baseItem.price, Constants.itemRepairCostCoefs),
    0
  );
}
export function computeAverageRepairCost(equippedItems: EquippedItem[]): number {
  return (
    equippedItems.reduce(
      (cost, ei) =>
        cost + applyPolynomialFunction(ei.userItem.baseItem.price, Constants.itemRepairCostCoefs),
      0
    ) * Constants.itemBreakChance
  );
}
export function computeHealthPoints(ironFlesh: number, strength: number): number {
  return (
    Constants.defaultHealthPoints +
    applyPolynomialFunction(ironFlesh, Constants.healthPointsForIronFleshCoefs) +
    applyPolynomialFunction(strength, Constants.healthPointsForStrengthCoefs)
  );
}

export function getExperienceForLevel(level: number): number {
  const a = Constants.experienceForLevelCoefs[0];
  const b = Constants.experienceForLevelCoefs[1];
  const c = Constants.experienceForLevelCoefs[2];
  return Math.trunc(Math.max(a * Math.pow(1.26, level) + b * level + c, 0));
}