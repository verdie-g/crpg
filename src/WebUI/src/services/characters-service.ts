import { applyPolynomialFunction } from '@/utils/math';
import Constants from '../../../../data/constants.json';
import type CharachterSpeedStats from '@/models/charachter-speed-stats';

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
  agility,
  athletics,
  totalEncumbrance,
}: {
  strength: number;
  agility: number;
  athletics: number;
  totalEncumbrance: number;
}): CharachterSpeedStats {
  const freeWeight = 3 * (1 + (strength - 3) / 30);
  const perceivedWeight = Math.max(totalEncumbrance - freeWeight, 0) / (1 + (strength - 3) / 5);
  const nakedSpeed = 0.7 + 0.001 * (20 * athletics + 3 * agility);
  const сurrentSpeed = nakedSpeed * (1 - perceivedWeight / 80);
  const timeToMaxSpeed = 0.5 * (1 + perceivedWeight / 50);

  return {
    freeWeight,
    perceivedWeight,
    nakedSpeed,
    сurrentSpeed,
    timeToMaxSpeed,
  };
}
