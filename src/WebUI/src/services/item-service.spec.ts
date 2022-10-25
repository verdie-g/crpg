import ItemsData from '@/__mocks__/items.json';
import {
  computeArmorSetPieceStrengthRequirement,
  computeOverallPrice,
  computeOverallWeight,
  computeOverallArmor,
} from './item-service';
import type Item from '@/models/item';

describe('Overall stats', () => {
  it('computeArmorSetPieceStrengthRequirement', () => {
    expect(computeArmorSetPieceStrengthRequirement(ItemsData as Item[])).toEqual(2);
  });

  it('computeArmorSetPieceStrengthRequirement - no items', () => {
    expect(computeArmorSetPieceStrengthRequirement([])).toEqual(0);
  });

  it('computeOverallPrice', () => {
    expect(computeOverallPrice(ItemsData as Item[])).toEqual(1430);
  });

  it('computeOverallPrice - no items', () => {
    expect(computeOverallPrice([])).toEqual(0);
  });

  it('computeOverallWeight', () => {
    expect(computeOverallWeight(ItemsData as Item[])).toEqual(3.8);
  });

  it('computeOverallWeight - no items', () => {
    expect(computeOverallWeight([])).toEqual(0);
  });

  it('computeOverallArmor', () => {
    expect(computeOverallArmor(ItemsData as Item[])).toEqual({
      armArmor: 0,
      bodyArmor: 2,
      headArmor: 0,
      legArmor: 4,
      mountArmor: 4,
    });
  });

  it('computeOverallArmor - no items', () => {
    expect(computeOverallArmor([])).toEqual({
      armArmor: 0,
      bodyArmor: 0,
      headArmor: 0,
      legArmor: 0,
      mountArmor: 0,
    });
  });
});
