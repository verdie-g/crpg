import type { RouteRecordRaw, RouteLocationRaw } from 'vue-router/auto';
import Role from '@/models/role';

const filterRoute = (role: Role) => (route: RouteRecordRaw) =>
  route.name !== undefined &&
  Boolean(route.meta?.showInNav) !== false &&
  (route.meta?.roles !== undefined ? route.meta.roles.includes(role) : true);

const sortRoutes = (a: RouteRecordRaw, b: RouteRecordRaw) =>
  b.meta?.sortInNav !== undefined && a.meta?.sortInNav !== undefined
    ? b.meta.sortInNav - a.meta.sortInNav
    : 0;

const flattenArray = (members: any[]): any[] => {
  let children: any[] = [];

  return members
    .map(mem => {
      const m = { ...mem };
      if (m.children && m.children.length) {
        children = [...children, ...m.children];
      }
      delete m.children;
      return m;
    })
    .concat(children.length ? flattenArray(children) : children);
};

export const useNavigation = (routes: RouteRecordRaw[], role: Role) => {
  const mainNavigation = computed<RouteLocationRaw[]>(() =>
    flattenArray(routes).filter(filterRoute(role)).sort(sortRoutes)
  );

  return {
    mainNavigation,
  };
};
