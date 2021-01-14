import Vue from 'vue';
import VueRouter, { NavigationGuard, NavigationGuardNext, Route } from 'vue-router';
import queryString, { ParsedQuery } from 'query-string';
import Role from '@/models/role';
import { getDecodedToken, getToken } from '@/services/auth-service';
import Home from '../views/Home.vue';

Vue.use(VueRouter);

function combineGuards(...guards: NavigationGuard[]): NavigationGuard {
  function callGuardsRec(guards: NavigationGuard[], idx: number, to: Route, from: Route, next: NavigationGuardNext): any {
    guards[idx](to, from, idx === guards.length - 1
      ? next
      : () => callGuardsRec(guards, idx + 1, to, from, next));
  }

  return (to, from, next) => callGuardsRec(guards, 0, to, from, next);
}

const isSignedInGuard: NavigationGuard = async (to, from, next) => {
  if (await getToken() === null) {
    next('/');
  } else {
    next();
  }
};

const isAdminGuard: NavigationGuard = async (to, from, next) => {
  const { userRole } = (await getDecodedToken())!;
  if (userRole !== Role.Admin && userRole !== Role.SuperAdmin) {
    next('/');
  } else {
    next();
  }
};

const scrollBehavior = (to: Route, from: Route, savedPosition: any) => {
  if (savedPosition) {
    return savedPosition;
  }

  // check if any matched route config has meta that requires scrolling to top
  if (to.matched.some(m => m.meta.scrollToTop)) {
    return window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  return null;
};

function parseQuery(query: string): ParsedQuery<string | number> {
  return queryString.parse(query, {
    arrayFormat: 'bracket',
    parseNumbers: true,
  });
}

function stringifyQuery(query: Record<string, any>): string {
  const result = queryString.stringify(query, {
    encode: true,
    arrayFormat: 'bracket',
    skipEmptyString: true,
    skipNull: true,
  });

  return result ? `?${result}` : '';
}

const routes = [
  {
    path: '/',
    name: 'home',
    component: Home,
  },
  {
    path: '/characters',
    name: 'characters',
    component: () => import('../views/Characters.vue'),
    beforeEnter: isSignedInGuard,
  },
  {
    path: '/shop',
    name: 'shop',
    component: () => import('../views/Shop.vue'),
    beforeEnter: isSignedInGuard,
    meta: { scrollToTop: true },
  },
  {
    path: '/settings',
    name: 'settings',
    component: () => import('../views/Settings.vue'),
    beforeEnter: isSignedInGuard,
  },
  {
    path: '/admin',
    name: 'admin',
    component: () => import('../views/Administration.vue'),
    beforeEnter: combineGuards(isSignedInGuard, isAdminGuard),
  },
  {
    path: '*',
    name: 'not-found',
    component: () => import('../views/NotFound.vue'),
  },
];

const router = new VueRouter({
  mode: 'history',
  base: process.env.BASE_URL,
  scrollBehavior,
  routes,
  parseQuery,
  stringifyQuery,
});

export default router;
