import {
  createRouter,
  createWebHistory,
  type RouteRecordRaw,
  type NavigationGuard,
} from 'vue-router/auto';
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
import { parseQuery, stringifyQuery, scrollBehavior } from '@/utils/router';

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
    scrollBehavior,
    /* A custom parse/stringify query is needed because by default
    ?types=HeadArmor&types=ShoulderArmor is parsed correctly as ["HeadArmor", "ShoulderArmor"]
    but ?types=HeadArmor is parsed as "HeadArmor" (not an array).
    To solve this issue qs library adds brackets for arrays ?types[]=HeadArmor.
    ref: https://router.vuejs.org/api/interfaces/RouterOptions.html#parsequery
    spec: src/WebUI/src/utils/router.spec.ts */
    parseQuery,
    stringifyQuery,
  });

  router.beforeEach(authRouterMiddleware);

  app.use(router);
};
