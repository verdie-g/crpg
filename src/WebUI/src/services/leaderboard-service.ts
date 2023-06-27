import qs from 'qs';
import {
  RankGroup,
  type Rank,
  type CharacterCompetitive,
  type CharacterCompetitiveNumbered,
} from '@/models/competitive';
import { Region } from '@/models/region';
import { type ClanEdition } from '@/models/clan';
import { type UserPublic } from '@/models/user';
import { get } from '@/services/crpg-client';
import { inRange } from '@/utils/math';
import { getEntries } from '@/utils/object';
import { mapClanResponse } from '@/services/clan-service';

interface UserPublicRaw extends Omit<UserPublic, 'clan'> {
  clan: ClanEdition | null;
}

interface CharacterCompetitiveRaw extends Omit<CharacterCompetitive, 'user'> {
  user: UserPublicRaw;
}

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

  const res = await get<CharacterCompetitiveRaw[]>(`/leaderboard/leaderboard?${params}`);

  return res.map((cr, idx) => ({
    ...cr,
    user: {
      ...cr.user,
      clan: cr.user.clan === null ? null : mapClanResponse(cr.user.clan),
    },
    position: idx + 1,
  }));
};

const rankColors: Record<RankGroup, string> = {
  [RankGroup.Iron]: '#A19D94',
  [RankGroup.Copper]: '#B87333',
  [RankGroup.Bronze]: '#CC6633',
  [RankGroup.Silver]: '#C7CCCA',
  [RankGroup.Gold]: '#EABC40',
  [RankGroup.Platinum]: '#40A7B9',
  [RankGroup.Diamond]: '#C289F5',
  [RankGroup.Champion]: '#B73E6C',
};

const step = 50;
const rankSubGroupCount = 5;

const createRank = (baseRank: [RankGroup, string]) =>
  [...Array(rankSubGroupCount).keys()].reverse().map(subRank => ({
    groupTitle: baseRank[0],
    title: `${baseRank[0]} ${subRank + 1}`,
    color: baseRank[1],
  }));

export const createRankTable = (): Rank[] =>
  getEntries<Record<RankGroup, string>>(rankColors)
    .flatMap(createRank)
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
