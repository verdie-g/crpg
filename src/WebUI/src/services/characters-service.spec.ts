import { computeSpeedStats } from '@/services/characters-service';

const CASES = [
  [
    {
      strength: 0,
      agility: 0,
      athletics: 0,
      totalEncumbrance: 0,
    },
    {
      freeWeight: 2.7,
      nakedSpeed: 0.7,
      perceivedWeight: 0,
      timeToMaxSpeed: 0.5,
      сurrentSpeed: 0.7,
    },
  ],
  [
    {
      strength: 3,
      agility: 3,
      athletics: 0,
      totalEncumbrance: 0,
    },
    {
      freeWeight: 3,
      nakedSpeed: 0.709,
      perceivedWeight: 0,
      timeToMaxSpeed: 0.5,
      сurrentSpeed: 0.709,
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
      nakedSpeed: 0.903,
      perceivedWeight: 3.3043478260869565,
      timeToMaxSpeed: 0.5330434782608695,
      сurrentSpeed: 0.8657021739130435,
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
      nakedSpeed: 1.048,
      perceivedWeight: 5,
      timeToMaxSpeed: 0.55,
      сurrentSpeed: 0.9825,
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
