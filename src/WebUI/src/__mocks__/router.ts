import type { RouteLocationNormalized } from 'vue-router';

export const getRoute = (
  routePart: Partial<RouteLocationNormalized> = {}
): RouteLocationNormalized => ({
  name: '',
  path: '',
  hash: '',
  meta: {},
  params: {},
  matched: [],
  fullPath: '',
  query: {},
  redirectedFrom: undefined,
  ...routePart,
});

export const next = vi.fn();
