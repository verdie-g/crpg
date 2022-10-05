// TODO:
import type Role from '@/models/role';
import type { RouteLocationNormalized } from 'vue-router';
// import { getDecodedToken, getToken, signInSilent } from '@/services/auth-service';

const routeHasAnyRoles = (route: RouteLocationNormalized): boolean =>
  Boolean(route.meta?.roles?.length);

const userAllowedAccess = (route: RouteLocationNormalized, roles: Array<`${Role}`>): boolean =>
  routeHasAnyRoles(route) && Boolean(route.meta?.roles?.some(role => roles.includes(role)));

export default (to: RouteLocationNormalized) => {
  // TODO:
  return true;
};
