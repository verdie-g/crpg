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
  return Math.pow(level - 1, a) + b * (level - 1);
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
  const AwfulScaler = 3231477.548;
  const [a, b, c, d] = [10 / AwfulScaler, 0.00005 / AwfulScaler, 1000000 / AwfulScaler, 0];
  const weightReductionFactor = 1 / (1 + applyPolynomialFunction(strength - 3, [a, b, c, d]));
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

  return {
    weightReductionFactor,
    freeWeight,
    perceivedWeight,
    nakedSpeed,
    сurrentSpeed,
    timeToMaxSpeed,
  };
}
