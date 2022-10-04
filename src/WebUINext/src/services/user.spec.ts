import MockAdapter from 'axios-mock-adapter';
import { api } from '~/boot/api';
import mockUserResponse from '~~/mocks/user.json';
import { getUser } from './user';

new MockAdapter(api).onGet('user').reply(200, mockUserResponse);

it('getUser', async () => {
  expect(await getUser()).toEqual(mockUserResponse);
});
