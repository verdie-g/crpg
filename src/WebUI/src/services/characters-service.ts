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
    const [a, b] = Constants.experienceForLevelCoefs;
    const scaler = Math.pow(29, a) + b * 29;
    return Math.trunc((4420824 * Math.max(Math.pow(level - 1, a) + b * (level - 1), 0)) / scaler);
  } else {
    return getExperienceForLevel(30) * Math.pow(2, level - 30);
  }
}

export function computeSpeedStats({
  strength,
  athletics,
  agility,
  totalEncumbrance,
}: {
  strength: number;
  athletics: number;
  agility: number;
  totalEncumbrance: number;
}): CharacterSpeedStats {
  const weightReductionFactor = 1 / (1 + (strength * strength - 9) / 81);
  const freeWeight = 3 * (1 + (strength - 3) / 30);
  const perceivedWeight = Math.max(totalEncumbrance - freeWeight, 0) * weightReductionFactor;
  const nakedSpeed = 0.7 + 0.00085 * (20 * athletics + 2 * agility);
  const сurrentSpeed = clamp(
    nakedSpeed * Math.pow(361 / (361 + Math.pow(perceivedWeight, 5)), 0.05),
    0.1,
    1.5
  );
  const timeToMaxSpeed =
    1.4 *
    (1 + perceivedWeight / 40) *
    (20 / (20 + Math.pow((20 * athletics + 3 * agility) / 100, 2)));

  return {
    freeWeight,
    perceivedWeight,
    nakedSpeed,
    сurrentSpeed,
    timeToMaxSpeed,
  };
}
