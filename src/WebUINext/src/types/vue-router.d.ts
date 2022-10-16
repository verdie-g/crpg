import 'vue-router';
import Role from '@/models/role';

declare module 'vue-router' {
  interface RouteMeta {
    layout?: string;
    roles?: Array<`${Role}`>;
    skipAuth?: boolean;
  }
}
