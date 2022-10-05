import { mockGet } from 'vi-fetch';
import mockUserResponse from '@@/mocks/user.json';
import type { Result } from '@/models/result';
import type { User } from '@/models/example';

import { getUser } from './example';

const userResponse: Result<User> = {
  data: mockUserResponse,
  errors: null,
};

it('getUser', async () => {
  mockGet('/user').willResolve(userResponse);

  expect(await getUser()).toEqual(mockUserResponse);
});
