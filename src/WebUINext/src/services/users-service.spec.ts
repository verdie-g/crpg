import { mockGet } from 'vi-fetch';
import mockUserResponse from '@/__mocks__/user.json';
import type { Result } from '@/models/crpg-client-result';
import type User from '@/models/user';

import { getUser } from './users-service';

const userResponse: Result<User> = {
  // @ts-ignore TODO:
  data: mockUserResponse,
  errors: null,
};

it('getUser', async () => {
  mockGet('/users/self').willResolve(userResponse);

  expect(await getUser()).toEqual(mockUserResponse);
});
