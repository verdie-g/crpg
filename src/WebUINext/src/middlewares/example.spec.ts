import { getRoute, next } from '@/__mocks__/router';
import { exampleRouterMiddleware } from './example';

const from = getRoute();

describe('router middleware/guard example', () => {
  it('redirect to login', async () => {
    const to = getRoute({
      name: 'admin',
      path: '/admin',
    });

    expect(await exampleRouterMiddleware(to, from, next)).toEqual({ name: 'login' });
  });

  it('ok', async () => {
    const to = getRoute({
      name: 'user',
      path: '/user',
    });

    expect(await exampleRouterMiddleware(to, from, next)).toEqual(true);
  });
});
