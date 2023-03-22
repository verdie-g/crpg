import { Region } from '@/models/region';
import { getHHScheduleConfig, getHHEventByRegion, getHHEventRemaining } from './hh-service';

// see: .env.test
it('getHHScheduleConfig', () => {
  expect(getHHScheduleConfig()).toEqual({
    Eu: {
      start: { hours: 19, minutes: 30 },
      end: { hours: 21, minutes: 30 },
      tz: 'Europe/Paris',
    },
    Na: {
      start: { hours: 20, minutes: 0 },
      end: { hours: 22, minutes: 0 },
      tz: 'America/Chicago',
    },
  });
});

it.each<
  [
    Region,
    {
      start: Date;
      end: Date;
    }
  ]
>([
  [
    Region.Eu,
    {
      start: new Date('2000-02-01T18:30:00.000Z'),
      end: new Date('2000-02-01T20:30:00.000Z'),
    },
  ],
  [
    Region.Na,
    {
      start: new Date('2000-02-02T02:00:00.000Z'),
      end: new Date('2000-02-02T04:00:00.000Z'),
    },
  ],
])('getHHEventByRegion - region: %s', (region, expectation) => {
  vi.setSystemTime(new Date(2000, 1, 1, 13));
  expect(getHHEventByRegion(region)).toEqual(expectation);
});

describe('getHHEventRemaining', () => {
  const event = {
    start: new Date('2000-02-01T18:30:00.000Z'),
    end: new Date('2000-02-01T20:30:00.000Z'),
  };

  it.each<[Date, number]>([
    [new Date('2000-02-01T20:29:59.999Z'), 1],
    [new Date('2000-02-01T20:30:00.000Z'), 0],
    [new Date('2000-02-01T20:31:00.000Z'), 0],
    [new Date('2000-02-01T18:30:00.000Z'), 7200000],
    [new Date('2000-02-01T18:29:00.000Z'), 0],
  ])('getHHEventRemaining - now: %s', (now, expectation) => {
    vi.setSystemTime(now);
    expect(getHHEventRemaining(event)).toEqual(expectation);
  });
});
