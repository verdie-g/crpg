import Vue from 'vue';
import VueRouter, { NavigationGuard } from 'vue-router';
import Home from '../views/Home.vue';
import userModule from '@/store/user-module';

Vue.use(VueRouter);

const isSignedInGuard: NavigationGuard = (to, from, next) => {
  if (!userModule.isSignedIn) {
    next('/');
  } else {
    next();
  }
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
  },
];

const router = new VueRouter({
  mode: 'history',
  base: process.env.BASE_URL,
  routes,
});

export default router;
