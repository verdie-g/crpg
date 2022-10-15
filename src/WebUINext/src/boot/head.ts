import { createHead } from '@vueuse/head';
import { type BootModule } from '@/types/boot-module';

export const install: BootModule = app => {
  const head = createHead();
  app.use(head);
};
