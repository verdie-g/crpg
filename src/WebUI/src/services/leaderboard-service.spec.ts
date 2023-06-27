import { type FetchSpyInstance, mockGet } from 'vi-fetch';
import { response } from '@/__mocks__/crpg-client';
import { Region } from '@/models/region';
import { createRankTable, getRankByCompetitiveValue, getLeaderBoard } from './leaderboard-service';

const { mockedMapClanResponse } = vi.hoisted(() => ({
  mockedMapClanResponse: vi.fn(),
}));
vi.mock('@/services/clan-service', () => ({ mapClanResponse: mockedMapClanResponse }));

/*
  Champion 1	1950	9999
  Champion 2	1900	1950
  Champion 3	1850	1900
  Champion 4	1800	1850
  Champion 5	1750	1800

  Diamond 1	1700	1750
  Diamond 2	1650	1700
  Diamond 3	1600	1650
  Diamond 4	1550	1600
  Diamond 5	1500	1550

  Platinum 1	1450	1500
  Platinum 2	1400	1450
  Platinum 3	1350	1400
  Platinum 4	1300	1350
  Platinum 5	1250	1300

  Gold 1	1200	1250
  Gold 2	1150	1200
  Gold 3	1100	1150
  Gold 4	1050	1100
  Gold 5	1000	1050

  Silver 1	950	1000
  Silver 2	900	950
  Silver 3	850	900
  Silver 4	800	850
  Silver 5	750	800

  Bronze 1	700	750
  Bronze 2	650	700
  Bronze 3	600	650
  Bronze 4	550	600
  Bronze 5	500	550

  Copper 1	450	500
  Copper 2	400	450
  Copper 3	350	400
  Copper 4	300	350
  Copper 5	250	300

  Iron 1	200	250
  Iron 2	150	200
  Iron 3	100	150
  Iron 4	50	100
  Iron 5	-9999	50

*/
it.each([
  [-1, 'Iron 5'],
  [0, 'Iron 5'],
  [49, 'Iron 5'],
  [50, 'Iron 4'],
  [51, 'Iron 4'],
  [9999, 'Champion 1'],
])('getRankByCompetitiveValue - competitiveValue: %s', (competitiveValue, expectation) => {
  expect(getRankByCompetitiveValue(createRankTable(), competitiveValue).title).toEqual(expectation);
});

describe.only('getLeaderBoard', () => {
  let mock: FetchSpyInstance;

  beforeEach(() => {
    mock = mockGet(/\/leaderboard\/leaderboard/).willDo(_url => {
      return {
        body: response([
          {
            id: 5,
            user: {
              name: 'orle',
              clan: {
                id: 1,
                primaryColor: '4278190318',
                secondaryColor: '4278190318',
              },
            },
          },
          {
            id: 12,
            user: {
              name: 'Kadse',
              clan: null,
            },
          },
        ]),
      };
    });
  });

  it('set position', async () => {
    const res = await getLeaderBoard(Region.Eu);

    expect(mock).toHaveFetchedWithQuery('region=Eu');

    expect(res[0].position).toEqual(1);
    expect(res[1].position).toEqual(2);
  });

  it('normalize clan color', async () => {
    await getLeaderBoard(Region.Eu);

    expect(mockedMapClanResponse).toBeCalledTimes(1);
    expect(mockedMapClanResponse).toBeCalledWith({
      id: 1,
      primaryColor: '4278190318',
      secondaryColor: '4278190318',
    });
  });
});
