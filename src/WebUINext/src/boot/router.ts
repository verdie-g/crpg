import {
  createRouter,
  createWebHistory,
  type RouteRecordRaw,
  type RouterScrollBehavior,
} from 'vue-router';
import { setupLayouts } from 'virtual:generated-layouts';
import generatedRoutes from 'virtual:generated-pages';
import queryString from 'query-string';

import { type BootModule } from '@/types/boot-module';
import { authRouterMiddleware } from '@/middlewares/auth';

const scrollBehavior: RouterScrollBehavior = (to, _from, savedPosition) => {
  if (savedPosition) {
    return savedPosition;
  }

  // check if any matched route config has meta that requires scrolling to top
  if (to.matched.some(m => m.meta.scrollToTop)) {
    return window.scrollTo({ top: 0, behavior: 'smooth' });
  }
};

const parseQuery = (query: string) => {
  return queryString.parse(query, {
    arrayFormat: 'bracket',
    parseNumbers: true,
    parseBooleans: true,
  });
};

const stringifyQuery = (query: Record<string, any>) => {
  const result = queryString.stringify(query, {
    encode: true,
    arrayFormat: 'bracket',
    skipEmptyString: true,
    skipNull: true,
  });

  return result ? `?${result}` : '';
};

export const install: BootModule = app => {
  const routes: RouteRecordRaw[] = setupLayouts(generatedRoutes);

  const router = createRouter({
    history: createWebHistory(),
    routes,
    scrollBehavior,
    /* A custom parse/stringify query is needed because by default
    ?types=HeadArmor&types=ShoulderArmor is parsed correctly as ["HeadArmor", "ShoulderArmor"]
    but ?types=HeadArmor is parsed as "HeadArmor" (not an array).
    To solve this issue query-string library adds brackets for arrays ?types[]=HeadArmor.
    */
    // @ts-ignore // FIXME: fix
    parseQuery,
    stringifyQuery,
  });

  router.beforeEach(authRouterMiddleware); // TODO: need implementation

  app.use(router);
};
