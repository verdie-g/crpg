import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router';
import { setupLayouts } from 'virtual:generated-layouts';
import generatedRoutes from 'virtual:generated-pages';

import { type BootModule } from '../types/boot';

export const install: BootModule = app => {
  const routes: RouteRecordRaw[] = setupLayouts(generatedRoutes);

  const router = createRouter({
    history: createWebHistory(),
    routes,
  });

  app.use(router);
};
