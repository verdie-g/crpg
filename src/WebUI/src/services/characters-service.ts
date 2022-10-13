import { applyPolynomialFunction } from '@/utils/math';
import Constants from '../../../../data/constants.json';

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
  if (level <= 30) {
    return Math.trunc(Math.max(a * Math.pow(1.26, level) + b * level + c, 0));
  } else {
    return getExperienceForLevel(30) * Math.pow(2, level - 30);
  }
}
