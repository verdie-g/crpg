import defu from 'defu';
import { type ComponentPublicInstance, defineComponent } from 'vue';
import { mount, flushPromises, type VueWrapper, MountingOptions } from '@vue/test-utils';
import {
  createRouter,
  createWebHistory,
  type Router,
  RouteRecordRaw,
  RouteLocationRaw,
} from 'vue-router';

// ref: https://github.com/vuejs/test-utils/issues/108#issuecomment-1124851726
export const mountWithRouter = async <Component extends ComponentPublicInstance, Props>(
  options: MountingOptions<Props> = {},
  routes: RouteRecordRaw[],
  route: RouteLocationRaw
): Promise<{
  wrapper: VueWrapper<ComponentPublicInstance>;
  router: Router;
}> => {
  const router = createRouter({
    history: createWebHistory(),
    routes,
  });

  router.push(route);

  await router.isReady();

  const app = defineComponent({
    template: `
        <router-view v-slot="{ Component }">
            <suspense>
                <component :is="Component" />
            </suspense>
        </router-view>
    `,
  });

  const wrapper = mount(
    app,
    defu(
      {
        global: {
          plugins: [router],
        },
      },
      options
    )
  );

  await flushPromises();

  return { wrapper, router };
};
