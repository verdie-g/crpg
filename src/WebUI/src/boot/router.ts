import {
  createRouter,
  createWebHistory,
  type RouteRecordRaw,
  type RouterScrollBehavior,
  type NavigationGuard,
} from 'vue-router/auto';
import qs from 'qs';
import { setupLayouts } from 'virtual:generated-layouts';
import { type BootModule } from '@/types/boot-module';
import { RouteMiddleware } from '@/types/vue-router';
import { authRouterMiddleware, signInCallback, signInSilentCallback } from '@/middlewares/auth';
import {
  clanIdParamValidate,
  clanExistValidate,
  canUpdateClan,
  canManageApplications,
} from '@/middlewares/clan';
import { characterValidate, activeCharacterRedirect } from '@/middlewares/character';

const scrollBehavior: RouterScrollBehavior = (to, _from, savedPosition) => {
  if (savedPosition) {
    return savedPosition;
  }

  // check if any matched route config has meta that requires scrolling to top
  if (to.matched.some(m => m.meta.scrollToTop)) {
    return { top: 0, behavior: 'smooth' };
  }
};

// TODO: FIXME: UNIT!!!!!!!!!!! to utils
const numberCandidate = (candidate: string) => /^[+-]?\d+(\.\d+)?$/.test(candidate);
const tryParseFloat = (str: string) => (numberCandidate(str) ? parseFloat(str) : str);
const decoder = (str: string): string | number | boolean | null | undefined => {
  // TODO: unit

  const candidateToNumber = tryParseFloat(str);

  if (typeof candidateToNumber == 'number' && !isNaN(candidateToNumber)) {
    return candidateToNumber;
  }

  const keywords: Record<string, any> = {
    true: true,
    false: false,
    null: null,
    undefined: undefined,
  };

  if (str in keywords) {
    return keywords[str];
  }

  return str;

  // TODO: ???
  // try {
  //   const strWithoutPlus = str.replace(/\+/g, ' ');
  //   return decodeURIComponent(strWithoutPlus);
  // } catch (_e) {
  //   return strWithoutPlus;
  // }
};

const parseQuery = (query: string) => {
  return qs.parse(query, {
    ignoreQueryPrefix: true,
    strictNullHandling: true,

    decoder,
  });
};

const stringifyQuery = (query: Record<string, any>) => {
  return qs.stringify(query, {
    encode: false,
    strictNullHandling: true,
    arrayFormat: 'brackets',
    skipNulls: true,
  });
};

const getRouteMiddleware = (name: RouteMiddleware) => {
  const middlewareMap: Record<RouteMiddleware, NavigationGuard> = {
    signInCallback: signInCallback,
    signInSilentCallback: signInSilentCallback,

    characterValidate: characterValidate,
    activeCharacterRedirect: activeCharacterRedirect,

    clanIdParamValidate: clanIdParamValidate,
    clanExistValidate: clanExistValidate,
    canUpdateClan: canUpdateClan,
    canManageApplications: canManageApplications,
  };

  return middlewareMap[name];
};

// TODO: FIXME: SPEC
const setRouteMiddleware = (routes: RouteRecordRaw[]) => {
  routes.forEach(route => {
    if (route.children !== undefined) {
      setRouteMiddleware(route.children);
    }

    if (route.meta?.middleware === undefined) return;
    route.beforeEnter = getRouteMiddleware(route.meta.middleware as RouteMiddleware);
  });
};

export const install: BootModule = app => {
  const router = createRouter({
    extendRoutes: routes => {
      setRouteMiddleware(routes); // auto-register route guard
      return setupLayouts(routes);
    },
    history: createWebHistory(),
    // routes,
    scrollBehavior,

    /* A custom parse/stringify query is needed because by default
    ?types=HeadArmor&types=ShoulderArmor is parsed correctly as ["HeadArmor", "ShoulderArmor"]
    but ?types=HeadArmor is parsed as "HeadArmor" (not an array).
    To solve this issue qs library adds brackets for arrays ?types[]=HeadArmor.
    https://router.vuejs.org/api/interfaces/RouterOptions.html#parsequery
    */
    parseQuery, // TODO: fix ts errors
    stringifyQuery,
  });

  router.beforeEach(authRouterMiddleware);

  app.use(router);
};
