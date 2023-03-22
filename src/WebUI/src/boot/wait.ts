// @ts-ignore // TODO: need types defenition, PR to https://github.com/MeForma/vue-wait-for
import wait from '@meforma/vue-wait-for';
import { type InjectionKey } from 'vue';
import { type BootModule } from '@/types/boot-module';

declare class Wait {
  is(name: string): boolean;
  start(name: string): void;
  end(name: string): void;
  any(): number;
}

export const VueWaitKey: InjectionKey<Wait> = Symbol('wait');

export const install: BootModule = app => {
  app.use(wait).provide(VueWaitKey, app.config.globalProperties.$wait);
};
