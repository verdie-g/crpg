// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-nocheck
import { mapRestrictions } from '@/services/restriction-service';

const [expiredDate, nowDate, futureDate] = [
  '2022-11-27T20:00:00.0000000Z',
  '2022-11-27T21:00:00.0000000Z',
  '2022-11-27T22:00:00.0000000Z',
];
const duration = 180000; // 3 min

// mock Date
const DateReal = global.Date;
const spy = jest.spyOn(global, 'Date').mockImplementation((...args) => {
  if (args.length) {
    return new DateReal(...args);
  }
  return new Date(nowDate);
});

describe('mapRestrictions', () => {
  afterAll(() => {
    spy.mockRestore();
  });

  it('single non-expired restriction', () => {
    const payload = [
      {
        id: 1,
        restrictedUser: {
          id: 1,
        },
        duration,
        type: 'Join',
        createdAt: futureDate,
      },
    ];

    expect(mapRestrictions(payload).at(0)!.active).toBeTruthy();
  });

  it('single expired restriction', () => {
    const payload = [
      {
        id: 1,
        restrictedUser: {
          id: 1,
        },
        duration,
        type: 'Join',
        createdAt: expiredDate,
      },
    ];

    expect(mapRestrictions(payload).at(0)!.active).toBeFalsy();
  });

  it('several restrictions of the same type', () => {
    const payload = [
      {
        id: 1,
        restrictedUser: {
          id: 1,
        },
        duration,
        type: 'Join',
        createdAt: expiredDate,
      },
      {
        id: 2,
        restrictedUser: {
          id: 1,
        },
        duration,
        type: 'Join',
        createdAt: futureDate,
      },
    ];

    const result = mapRestrictions(payload);

    expect(result.at(0)!.active).toBeFalsy();
    expect(result.at(1)!.active).toBeTruthy();
  });

  it('several restrictions of the different type', () => {
    const payload = [
      {
        id: 1,
        restrictedUser: {
          id: 1,
        },
        duration,
        type: 'Chat',
        createdAt: futureDate,
      },
      {
        id: 2,
        restrictedUser: {
          id: 1,
        },
        duration,
        type: 'Join',
        createdAt: futureDate,
      },
    ];

    const result = mapRestrictions(payload);

    expect(result.at(0)!.active).toBeTruthy();
    expect(result.at(1)!.active).toBeTruthy();
  });

  it('several restrictions of the different user', () => {
    const payload = [
      {
        id: 1,
        restrictedUser: {
          id: 1,
        },
        duration,
        type: 'Join',
        createdAt: futureDate,
      },
      {
        id: 2,
        restrictedUser: {
          id: 2,
        },
        duration,
        type: 'Join',
        createdAt: futureDate,
      },
    ];

    const result = mapRestrictions(payload);

    expect(result.at(0)!.active).toBeTruthy();
    expect(result.at(1)!.active).toBeTruthy();
  });
});
