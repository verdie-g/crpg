import { type HumanDuration } from '@/models/datetime';

const [expiredDate, nowDate, futureDate] = [
  '2022-11-27T20:00:00.0000000Z',
  '2022-11-27T21:00:00.0000000Z',
  '2022-11-27T22:00:00.0000000Z',
];

// mock Date
const DateReal = global.Date;
vi.spyOn(global, 'Date').mockImplementation((...args: any[]) => {
  if (args.length) {
    // @ts-ignore
    return new DateReal(...args);
  }
  return new Date(nowDate);
});

import {
  parseTimestamp,
  convertHumanDurationToMs,
  checkIsDateExpired,
  computeLeftMs,
  isBetween,
} from './date';

it.each<[[Date, Date, Date], boolean]>([
  [
    [
      new Date('2000-01-01T10:00:00.000Z'),
      new Date('2000-01-01T09:00:00.000Z'),
      new Date('2000-01-01T11:00:00.000Z'),
    ],
    true,
  ],
  [
    [
      new Date('2000-01-01T09:00:00.000Z'),
      new Date('2000-01-01T09:00:00.000Z'),
      new Date('2000-01-01T09:00:00.000Z'),
    ],
    true,
  ],
  [
    [
      new Date('2000-01-01T09:00:00.000Z'),
      new Date('2000-01-01T10:00:00.000Z'),
      new Date('2000-01-01T09:00:00.000Z'),
    ],
    false,
  ],
])('isBetween - dates: %j', (dates, expectation) => {
  expect(isBetween(...dates)).toEqual(expectation);
});

it('computeLeftMs', () => {
  expect(computeLeftMs(new Date(nowDate), 1000)).toEqual(1000);
});

it('checkIsDateExpired', () => {
  expect(checkIsDateExpired(new Date(expiredDate), 1000)).toEqual(true);
  expect(checkIsDateExpired(new Date(nowDate), 1000)).toEqual(false);
  expect(checkIsDateExpired(new Date(futureDate), 1000)).toEqual(false);
});

it.each<[HumanDuration, number]>([
  [{ days: 0, hours: 0, minutes: 1 }, 60000],
  [{ days: 0, hours: 0, minutes: 0 }, 0],
])('convertHumanDurationToMs - humanDateTime: %j', (humanDateTime, expectation) => {
  expect(convertHumanDurationToMs(humanDateTime)).toEqual(expectation);
});

it('parseTimestamp', () => {
  expect(parseTimestamp(60000)).toEqual({
    days: 0,
    hours: 0,
    minutes: 1,
  });
  expect(parseTimestamp(12000000)).toEqual({
    days: 0,
    hours: 3,
    minutes: 20,
  });
});
