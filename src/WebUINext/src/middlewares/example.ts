import type { NavigationGuard } from 'vue-router';

const exampleRouterMiddleware: NavigationGuard = (to, from) => {
  // use return instead next()
  // return { name: 'index' }
  // return '/'
  return true;
};

export default exampleRouterMiddleware;
