import { computeSpeedStats } from '@/services/characters-service';

const CASES = [[]];

it.each(CASES)('computeSpeedStats', (a, b) => {
  expect(
    computeSpeedStats(
      a as {
        strength: number;
        agility: number;
        athletics: number;
        totalEncumbrance: number;
      }
    )
  ).toEqual(b);
});
