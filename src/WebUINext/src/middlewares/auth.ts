// TODO:
import type Role from '@/models/role';
import type { RouteLocationNormalized, NavigationGuard } from 'vue-router';
// import { getDecodedToken, getToken, signInSilent } from '@/services/auth-service';

const routeHasAnyRoles = (route: RouteLocationNormalized): boolean =>
  Boolean(route.meta?.roles?.length);

const userAllowedAccess = (route: RouteLocationNormalized, roles: Array<`${Role}`>): boolean =>
  routeHasAnyRoles(route) && Boolean(route.meta?.roles?.some(role => roles.includes(role)));

export const authRouterMiddleware: NavigationGuard = async (to, _from, _next) => {
  // TODO:

  return true;
};
