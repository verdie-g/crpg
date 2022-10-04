import { createPinia } from 'pinia';
import { type BootModule } from '~/types/boot';

export const install: BootModule = app => {
  const pinia = createPinia();
  app.use(pinia);
};
