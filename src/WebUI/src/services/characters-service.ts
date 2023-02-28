import { applyPolynomialFunction, clamp } from '@/utils/math';
import Constants from '../../../../data/constants.json';
import type CharacterSpeedStats from '@/models/сharacter-speed-stats';

export function computeHealthPoints(ironFlesh: number, strength: number): number {
  return (
    Constants.defaultHealthPoints +
    applyPolynomialFunction(ironFlesh, Constants.healthPointsForIronFleshCoefs) +
    applyPolynomialFunction(strength, Constants.healthPointsForStrengthCoefs)
  );
}

export function getExperienceForLevel(level: number): number {
  if (level <= 30) {
    return Math.trunc(
      4420824 *
        Math.max(ComputeExperienceDistribution(level) / ComputeExperienceDistribution(30), 0)
    );
  } else {
    return getExperienceForLevel(30) * Math.pow(2, level - 30);
  }
}

function ComputeExperienceDistribution(level: number): number {
  const [a, b] = Constants.experienceForLevelCoefs;
  return Math.pow(level - 1, a) + Math.pow(b, a / 2.0) * (level - 1);
}

export function computeSpeedStats({
  strength,
  athletics,
  agility,
  totalEncumbrance,
  longestWeaponLength,
}: {
  strength: number;
  athletics: number;
  agility: number;
  totalEncumbrance: number;
  longestWeaponLength: number;
}): CharacterSpeedStats {
  const awfulScaler = 3231477.548;
  const weightReductionPolynomialFactor = [
    30 / awfulScaler,
    0.00005 / awfulScaler,
    1000000 / awfulScaler,
    0,
  ];
  const weightReductionFactor =
    1 / (1 + applyPolynomialFunction(strength - 3, weightReductionPolynomialFactor));
  const freeWeight = 2.5 * (1 + (strength - 3) / 30);
  const perceivedWeight = Math.max(totalEncumbrance - freeWeight, 0) * weightReductionFactor;
  const nakedSpeed = 0.68 + 0.00091 * (20 * athletics + 2 * agility);
  const сurrentSpeed = clamp(
    nakedSpeed * Math.pow(361 / (361 + Math.pow(perceivedWeight, 5)), 0.055),
    0.1,
    1.5
  );
  const timeToMaxSpeed =
    1.5 *
    (1 + perceivedWeight / 15) *
    (20 / (20 + Math.pow((20 * athletics + 3 * agility) / 120, 2)));
  const maxWeaponLength = 75 + (strength - 3) * 7;

  const movementSpeedPenaltyWhenAttacking =
    100 * (Math.min(0.8 + (0.2 * maxWeaponLength) / longestWeaponLength, 1) - 1);

  return {
    weightReductionFactor,
    freeWeight,
    perceivedWeight,
    nakedSpeed,
    сurrentSpeed,
    timeToMaxSpeed,
    maxWeaponLength,
    movementSpeedPenaltyWhenAttacking,
  };
}
