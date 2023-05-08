import { VOnboardingWrapper, VOnboardingStep } from 'v-onboarding';
import { type BootModule } from '@/types/boot-module';

import 'v-onboarding/dist/style.css';

export const install: BootModule = app => {
  app
    .component('VOnboardingWrapper', VOnboardingWrapper)
    .component('VOnboardingStep', VOnboardingStep);
};
