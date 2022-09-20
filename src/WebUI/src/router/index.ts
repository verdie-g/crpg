import Vue from 'vue';
import VueRouter, { NavigationGuard, NavigationGuardNext, Route } from 'vue-router';
import queryString, { ParsedQuery } from 'query-string';
import Role from '@/models/role';
import { getDecodedToken, getToken, signInSilent } from '@/services/auth-service';
import Home from '../views/Home.vue';

Vue.use(VueRouter);

function combineGuards(...guards: NavigationGuard[]): NavigationGuard {
  function callGuardsRec(
    guards: NavigationGuard[],
    idx: number,
    to: Route,
    from: Route,
    next: NavigationGuardNext
  ): any {
    guards[idx](
      to,
      from,
      idx === guards.length - 1 ? next : () => callGuardsRec(guards, idx + 1, to, from, next)
    );
  }

  return (to, from, next) => callGuardsRec(guards, 0, to, from, next);
}

const isSignedInGuard: NavigationGuard = async (to, from, next) => {
  if ((await getToken()) === null) {
    try {
      const token = await signInSilent();
      if (token) next();
      else next('/');
    } catch {
      next('/');
    }
  } else {
    next();
  }
};

const isModeratorGuard: NavigationGuard = async (to, from, next) => {
  const { userRole } = (await getDecodedToken())!;
  if (userRole !== Role.Moderator && userRole !== Role.Admin) {
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

function parseQuery(query: string): ParsedQuery<string | number | boolean> {
  return queryString.parse(query, {
    arrayFormat: 'bracket',
    parseNumbers: true,
    parseBooleans: true,
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
    path: '/clans',
    name: 'clans',
    component: () => import('../views/Clans.vue'),
    beforeEnter: isSignedInGuard,
  },
  {
    path: '/clans/create',
    name: 'clan-create',
    component: () => import('../views/ClanCreation.vue'),
    beforeEnter: isSignedInGuard,
  },
  {
    path: '/clans/:id',
    name: 'clan',
    component: () => import('../views/Clan.vue'),
    beforeEnter: isSignedInGuard,
  },
  {
    path: '/clans/:id/settings',
    name: 'clan-settings',
    component: () => import('../views/ClanSettings.vue'),
    beforeEnter: isSignedInGuard,
  },
  {
    path: '/clans/:id/applications',
    name: 'clan-applications',
    component: () => import('../views/ClanApplications.vue'),
    beforeEnter: isSignedInGuard,
  },
  {
    path: '/strategus',
    name: 'strategus',
    component: () => import('../views/Strategus.vue'),
    beforeEnter: isSignedInGuard,
    meta: { footer: false },
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
    beforeEnter: combineGuards(isSignedInGuard, isModeratorGuard),
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
  /* A custom parse/stringify query is needed because by default
    ?types=HeadArmor&types=ShoulderArmor is parsed correctly as ["HeadArmor", "ShoulderArmor"]
    but ?types=HeadArmor is parsed as "HeadArmor" (not an array).
    To solve this issue query-string library adds brackets for arrays ?types[]=HeadArmor.
  */
  parseQuery,
  stringifyQuery,
});

export default router;
