import { createRankTable, getRankByCompetitiveValue } from './leaderboard-service';

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
