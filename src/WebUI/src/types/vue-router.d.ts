import 'vue-router';
import Role from '@/models/role';

// see getRouteMiddleware in src/boot/router.ts
export enum RouteMiddleware {
  'signInCallback' = 'signInCallback',

  'characterValidate' = 'characterValidate',
  'activeCharacterRedirect' = 'activeCharacterRedirect',

  'clanIdParamValidate' = 'clanIdParamValidate',
  'clanExistValidate' = 'clanExistValidate',
  'canUpdateClan' = 'canUpdateClan',
  'canManageApplications' = 'canManageApplications',
}

declare module 'vue-router' {
  interface RouteMeta {
    layout?: string;
    title?: string;
    roles?: Array<`${Role}`>;
    /*
      TODO: Implement the ability to add multiple routeGuards
      https://github.com/vuejs/vue-router/issues/2688
    */
    middleware?: `${RouteMiddleware}`;
    skipAuth?: boolean;
    scrollToTop?: boolean;
    bg?: string;
    noStickyHeader?: boolean;
    noFooter?: boolean;
  }
}
