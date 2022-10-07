import type { RouteLocationNormalized, NavigationGuard } from 'vue-router';

const canUserAccess = (route: RouteLocationNormalized): Promise<boolean> =>
  new Promise(resolve => resolve(route.name === 'admin' ? false : true));

export const exampleRouterMiddleware: NavigationGuard = async to => {
  // use return instead next()
  // return { name: 'index' }
  // return '/'

  const canAccess = await canUserAccess(to);

  if (!canAccess) return { name: 'login' };

  return true;
};
