import qs from 'qs';
import {
  type Rank,
  type CharacterCompetitive,
  type CharacterCompetitiveNumbered,
} from '@/models/competitive';
import { Region } from '@/models/region';
import { get } from '@/services/crpg-client';
import { inRange } from '@/utils/math';

// TODO: spec
export const getLeaderBoard = async (region?: Region): Promise<CharacterCompetitiveNumbered[]> => {
  // TODO: realize GET params in crpg-client
  const params = qs.stringify(
    { region },
    {
      strictNullHandling: true,
      arrayFormat: 'brackets',
      skipNulls: true,
    }
  );

  return (await get<CharacterCompetitive[]>(`/leaderboard/leaderboard?${params}`)).map(
    (cr, idx) => ({
      position: idx + 1,
      ...cr,
    })
  );
};

const rankGroups: [string, string][] = [
  ['Iron', '#555756'],
  ['Copper', '#B87333'],
  ['Bronze', '#CD7F32'],
  ['Silver', '#C7CCCA'],
  ['Gold', '#EABC40'],
  ['Platinum', '#40A7B9'],
  ['Diamond', '#C289F5'],
  ['Champion', '#B73E6C'],
];

const step = 50;
const rankSubGroupCount = 5;

const createRankGroup = (baseRank: [string, string]) =>
  [...Array(rankSubGroupCount).keys()]
    .reverse()
    .map(subRank => ({ title: `${baseRank[0]} ${subRank + 1}`, color: baseRank[1] }));

export const createRankTable = (): Rank[] =>
  rankGroups
    .flatMap(createRankGroup)
    .map((baseRank, idx) => ({ ...baseRank, min: idx * step, max: idx * step + step }));

export const getRankByCompetitiveValue = (rankTable: Rank[], competitiveValue: number) => {
  if (competitiveValue < rankTable[0].min) {
    return rankTable[0];
  }

  if (competitiveValue > rankTable[rankTable.length - 1].max) {
    return rankTable[rankTable.length - 1];
  }

  return createRankTable().find(row => inRange(competitiveValue, row.min, row.max))!;
};
