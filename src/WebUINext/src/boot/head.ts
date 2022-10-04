import { createHead } from '@vueuse/head';
import { type BootModule } from '../types/boot';

export const install: BootModule = app => {
  const head = createHead();
  app.use(head);
};
