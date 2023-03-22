import { createHead } from '@vueuse/head';
import { type BootModule } from '@/types/boot-module';

export const install: BootModule = app => {
  app.use(createHead());
};
