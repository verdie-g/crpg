import type { RouteLocationNormalized, NavigationGuard } from 'vue-router/auto';

import { useUserStore } from '@/stores/user';
import { userManager, signInSilent } from '@/services/auth-service';
import type Role from '@/models/role';

const routeHasAnyRoles = (route: RouteLocationNormalized<any>): boolean =>
  Boolean(route.meta?.roles?.length);

const userAllowedAccess = (route: RouteLocationNormalized<any>, role: Role): boolean =>
  Boolean(route.meta?.roles?.includes(role));

export const authRouterMiddleware: NavigationGuard = async to => {
  /*
    (1) service route, for example - oidc callback pages: /signin-callback, /signin-silent-callback
    (2) user is not logged in, try signInSilent then fetch user data ()
    (3) to-route has a role requirement
    (3) user has access
  */

  // (1)
  if (to.meta?.skipAuth) {
    return true;
  }

  const userStore = useUserStore();

  // (2)
  if (!userStore.user) {
    let token = null;

    token = await signInSilent();

    if (token) {
      await userStore.fetchUser();
    }
  }

  // (3)
  if (routeHasAnyRoles(to)) {
    // (4)
    if (userStore.user === null || !userAllowedAccess(to, userStore.user.role)) {
      return { name: 'Root' } as RouteLocationNormalized<'Root'>;
    }
  }

  return true;
};

export const signInCallback: NavigationGuard = async () => {
  await userManager.signinCallback();
  return { name: 'Characters' } as RouteLocationNormalized<'Characters'>;
};

export const signInSilentCallback: NavigationGuard = async () => {
  await userManager.signinSilentCallback();
  return true;
};
