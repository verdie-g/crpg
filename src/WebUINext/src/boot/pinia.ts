import { createPinia } from 'pinia';
import { type BootModule } from '@/types/boot-module';

export const install: BootModule = app => {
  const pinia = createPinia();
  app.use(pinia);
};
