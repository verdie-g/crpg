import 'vue-router';
import Role from '@/models/role';

// see getRouteMiddleware in src/boot/router.ts
export enum RouteMiddleware {
  'signInCallback' = 'signInCallback',
  'signInSilentCallback' = 'signInSilentCallback',

  'characterValidate' = 'characterValidate',
  'activeCharacterRedirect' = 'activeCharacterRedirect',

  'clanIdParamValidate' = 'clanIdParamValidate',
  'clanExistValidate' = 'clanExistValidate',
}

declare module 'vue-router' {
  interface RouteMeta {
    layout?: string;
    title?: string;
    roles?: Array<`${Role}`>;
    middleware?: `${RouteMiddleware}`;
    skipAuth?: boolean;
    scrollToTop?: boolean;
    bg?: string;
    noStickyHeader?: boolean;
  }
}
