import { mockGet, mockPost } from 'vi-fetch';
import { response } from '@/__mocks__/crpg-client';
import { type RestrictionCreation, type RestrictionWithActive } from '@/models/restriction';

const mockCheckIsDateExpired = vi.fn();
vi.mock('@/utils/date', () => ({
  checkIsDateExpired: mockCheckIsDateExpired,
}));

import {
  mapRestrictions,
  getRestrictions,
  restrictUser,
  getActiveJoinRestriction,
} from '@/services/restriction-service';

const duration = 180000; // 3 min

describe('mapRestrictions', () => {
  describe('single', () => {
    const payload = [
      {
        id: 1,
        restrictedUser: {
          id: 1,
        },
        type: 'Join',
        createdAt: '2022-11-27T22:00:00.000Z',
      },
    ];

    it('non-expired', () => {
      mockCheckIsDateExpired.mockReturnValue(false);

      // @ts-ignore
      expect(mapRestrictions(payload).at(0).active).toBeTruthy();
    });

    it('expired', () => {
      mockCheckIsDateExpired.mockReturnValue(true);

      // @ts-ignore
      expect(mapRestrictions(payload).at(0).active).toBeFalsy();
    });
  });

  describe('several', () => {
    describe('same type', () => {
      const payload = [
        {
          id: 1,
          restrictedUser: {
            id: 1,
          },
          type: 'Join',
          createdAt: '2022-11-28T22:00:00.000Z',
        },
        {
          id: 2,
          restrictedUser: {
            id: 1,
          },
          type: 'Join',
          createdAt: '2022-11-27T22:00:00.000Z',
        },
      ];

      it('non-expired', () => {
        mockCheckIsDateExpired.mockReturnValue(false);

        // @ts-ignore
        const result = mapRestrictions(payload);

        expect(result.at(0)!.active).toBeTruthy();
        expect(result.at(1)!.active).toBeFalsy();
      });

      it('expired', () => {
        mockCheckIsDateExpired.mockReturnValue(true);

        // @ts-ignore
        const result = mapRestrictions(payload);

        expect(result.at(0)!.active).toBeFalsy();
        expect(result.at(1)!.active).toBeFalsy();
      });
    });

    describe('different type', () => {
      const payload = [
        {
          id: 1,
          restrictedUser: {
            id: 1,
          },
          duration,
          type: 'Chat',
          createdAt: '2022-11-27T22:00:00.000Z',
        },
        {
          id: 2,
          restrictedUser: {
            id: 1,
          },
          duration,
          type: 'Join',
          createdAt: '2022-11-27T22:00:00.000Z',
        },
      ];

      it('non-expired', () => {
        mockCheckIsDateExpired.mockReturnValue(false);

        // @ts-ignore
        const result = mapRestrictions(payload);

        expect(result.at(0)!.active).toBeTruthy();
        expect(result.at(1)!.active).toBeTruthy();
      });

      it('expired', () => {
        mockCheckIsDateExpired.mockReturnValue(true);

        // @ts-ignore
        const result = mapRestrictions(payload);

        expect(result.at(0)!.active).toBeFalsy();
        expect(result.at(1)!.active).toBeFalsy();
      });
    });

    describe('different user', () => {
      const payload = [
        {
          id: 1,
          restrictedUser: {
            id: 1,
          },
          duration,
          type: 'Join',
        },
        {
          id: 2,
          restrictedUser: {
            id: 2,
          },
          duration,
          type: 'Join',
        },
      ];

      it('non-expired', () => {
        mockCheckIsDateExpired.mockReturnValue(false);

        // @ts-ignore
        const result = mapRestrictions(payload);

        expect(result.at(0)!.active).toBeTruthy();
        expect(result.at(1)!.active).toBeTruthy();
      });

      it('expired', () => {
        mockCheckIsDateExpired.mockReturnValue(true);

        // @ts-ignore
        const result = mapRestrictions(payload);

        expect(result.at(0)!.active).toBeFalsy();
        expect(result.at(1)!.active).toBeFalsy();
      });
    });
  });
});

it('getRestrictions', async () => {
  mockCheckIsDateExpired.mockReturnValue(false);
  const restrictions = {
    id: 1,
    restrictedUser: { id: 1 },
    duration,
    type: 'Join',
    reason: '',
    restrictedByUser: { id: 1 },
  };

  mockGet('/restrictions').willResolve(response([restrictions]));
  expect(await getRestrictions()).toEqual([{ ...restrictions, active: true }]);

  expect(mockCheckIsDateExpired).toBeCalled();
});

it('getActiveJoinRestriction', () => {
  const restrictions = [
    {
      id: 1,
      duration: 1,
      type: 'Chat',
      reason: '',
      active: true,
    },
    {
      id: 2,
      duration: 11,
      type: 'Join',
      reason: '',
      active: true,
    },
  ];

  expect(getActiveJoinRestriction(restrictions as RestrictionWithActive[])!.id).toEqual(2);
});

it('restrictUser', async () => {
  const payload = {
    restrictedUserId: 1,
    type: 'Chat',
    reason: '',
    duration: 100,
  } as RestrictionCreation;

  const restriction = {
    id: 1,
    restrictedUser: { id: 1 },
    duration: 1,
    type: 'Join',
    reason: '',
    restrictedByUser: { id: 1 },
  };

  const mock = mockPost('/restrictions').willResolve(response(restriction));
  expect(await restrictUser(payload)).toEqual(restriction);
  expect(mock).toHaveFetchedWithBody(payload);
});
