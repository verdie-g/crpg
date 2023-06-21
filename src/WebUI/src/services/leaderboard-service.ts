import { type Character } from '@/models/character';
import { type Rank } from '@/models/competitive';
import { get } from '@/services/crpg-client';
import { inRange } from '@/utils/math';

// export const getLeaderBoard = () => get<Character[]>('/leaderboard/leaderboard');
const meow = {
  // TODO:
  //
  name: 'Nami Legodaklas',
  userId: 11,
  level: 35,
  user: {
    name: 'orle',
    avatar:
      'https://avatars.akamai.steamstatic.com/d51d5155b1a564421c0b3fd5fb7eed7c4474e73d_full.jpg',
    region: 'Eu',
    platformUserId: 11,
    platform: 'Steam',
  },
  class: 'Archer',
  rating: {
    value: 50,
    deviation: 100,
    volatility: 100,
    competitiveValue: 1800,
  },
};

export const getLeaderBoard = () =>
  new Promise(res => {
    res(
      [
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
        meow,
      ].map((d, idx) => ({ idx: idx + 1, ...d }))
    );
  });

const step = 50;
const rankSubGroupCount = 5;
const rankGroups: [string, string][] = [
  // ['Iron', '#555756'],
  // ['Copper', '#B87333'],
  // ['Bronze', '#CD7F32'],
  // ['Silver', '#C7CCCA'],
  // ['Gold', '#EABC40'],
  // ['Platinum', '#40A7B9'],
  // ['Diamond', '#C289F5'],
  // ['Champion', '#B73E6C'],
  ['Peasant', '#555756'],
  ['Squire', '#B87333'],
  ['Knight', '#CD7F32'],
  ['Viscount', '#C7CCCA'],
  ['Earl', '#EABC40'],
  ['Duke', '#40A7B9'],
  ['King', '#C289F5'],
  ['Emperor', '#B73E6C'],
];

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
