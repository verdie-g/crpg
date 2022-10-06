import { exampleRouterMiddleware } from './example';

describe('router middleware/guard example', () => {
  it('redirect to login', async () => {
    const from = {
      name: 'index',
      path: '/',
    };

    const to = {
      name: 'admin',
      path: '/admin',
    };

    // @ts-ignore need mock next fn :(
    expect(await exampleRouterMiddleware(to, from)).toEqual({ name: 'login' });
  });

  it('ok', async () => {
    const from = {
      name: 'index',
      path: '/',
    };

    const to = {
      name: 'user',
      path: '/user',
    };

    // @ts-ignore need mock next fn :(
    expect(await exampleRouterMiddleware(to, from, next)).toBeTruthy();
  });
});
