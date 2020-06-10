import Vue from 'vue';
import VueRouter, { NavigationGuard, NavigationGuardNext, Route } from 'vue-router';
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

const isSignedInGuard: NavigationGuard = (to, from, next) => {
  if (getToken() === undefined) {
    next('/');
  } else {
    next();
  }
};

const isAdminGuard: NavigationGuard = (to, from, next) => {
  const { userRole } = getDecodedToken()!;
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
];

const router = new VueRouter({
  mode: 'history',
  base: process.env.BASE_URL,
  scrollBehavior,
  routes,
});

export default router;
