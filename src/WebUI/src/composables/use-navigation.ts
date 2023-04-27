import type { RouteRecordRaw } from 'vue-router/auto';
import Role from '@/models/role';
import { flatten } from '@/utils/object';

const filterRoute = (role: Role) => (route: RouteRecordRaw) =>
  route.name !== undefined &&
  Boolean(route.meta?.showInNav) !== false &&
  (route.meta?.roles !== undefined ? route.meta.roles.includes(role) : true);

const sortRoutes = (a: RouteRecordRaw, b: RouteRecordRaw) =>
  b.meta?.sortInNav !== undefined && a.meta?.sortInNav !== undefined
    ? b.meta.sortInNav - a.meta.sortInNav
    : 0;

export const useNavigation = (routes: RouteRecordRaw[], role: Role) => {
  const mainNavigation = computed(() =>
    flatten<RouteRecordRaw>(routes).filter(filterRoute(role)).sort(sortRoutes)
  );

  return {
    mainNavigation,
  };
};
