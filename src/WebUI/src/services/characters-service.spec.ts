import { computeSpeedStats } from '@/services/characters-service';

const CASES = [
  [
    {
      strength: 3,
      agility: 3,
      athletics: 0,
      totalEncumbrance: 0,
    },
    {
      freeWeight: 3,
      nakedSpeed: 0.7081,
      perceivedWeight: 0,
      timeToMaxSpeed: 0.8,
      сurrentSpeed: 0.7081,
    },
  ],
  [
    {
      strength: 21,
      agility: 21,
      athletics: 7,
      totalEncumbrance: 20,
    },
    {
      freeWeight: 4.800000000000001,
      nakedSpeed: 0.8826999999999999,
      perceivedWeight: 5.428571428571429,
      timeToMaxSpeed: 0.9085714285714286,
      сurrentSpeed: 0.8142457142857141,
    },
  ],
  [
    {
      strength: 3,
      agility: 36,
      athletics: 12,
      totalEncumbrance: 8,
    },
    {
      freeWeight: 3,
      nakedSpeed: 1.0131999999999999,
      perceivedWeight: 5,
      timeToMaxSpeed: 0.9,
      сurrentSpeed: 0.9408285714285713,
    },
  ],
];

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
