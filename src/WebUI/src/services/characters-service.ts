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
  const [a, b, c] = Constants.experienceForLevelCoefs;
  if (level <= 30) {
    return Math.trunc(Math.max(a * Math.pow(1.26, level) + b * level + c, 0));
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
  const weightReductionFactor = 1 / (1 + (strength - 3) / 10);

  const freeWeight = 3 * (1 + (strength - 3) / 30);
  const perceivedWeight = Math.max(totalEncumbrance - freeWeight, 0) * weightReductionFactor;

  const nakedSpeed = 0.7 + 0.0009 * (20 * athletics + 3 * agility);
  const сurrentSpeed = clamp(nakedSpeed * (1 - perceivedWeight / 70), 0.1, 1.5);
  const timeToMaxSpeed = 0.8 * (1 + perceivedWeight / 40);

  return {
    freeWeight,
    perceivedWeight,
    nakedSpeed,
    сurrentSpeed,
    timeToMaxSpeed,
  };
}
