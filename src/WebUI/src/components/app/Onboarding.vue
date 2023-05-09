<script setup lang="ts">
import { type VOnboardingWrapperOptions, useVOnboarding, VOnboardingWrapper } from 'v-onboarding';

type AttachableElement = string | (() => Element | null);

const props = defineProps<{
  steps: {
    attachTo: {
      element: AttachableElement;
      classList?: string[];
    };
    content?: {
      title: string;
      description?: string;
    };
  }[];
}>();

const options = {
  popper: {
    modifiers: [
      {
        name: 'offset',
        options: {
          offset: [0, 8],
        },
      },
    ],
  },
  overlay: {
    padding: 8,
  },
} as VOnboardingWrapperOptions;

const wrapper = ref<ComponentPublicInstance<typeof VOnboardingWrapper> | null>(null);
const { start, goToStep, finish } = useVOnboarding(wrapper);
</script>

<template>
  <div>
    <div class="fixed left-4 top-32 z-30">
      <OButton @click="start" size="sm" variant="primary" rounded iconLeft="help" />
    </div>

    <VOnboardingWrapper ref="wrapper" :steps="steps" @exit="finish" :options="options">
      <template #default="{ previous, next, step, exit, isFirst, isLast, index }">
        <OnboardingStep
          :step="step"
          :isFirst="isFirst"
          :isLast="isLast"
          :index="index"
          :total="steps.length"
          @previous="previous"
          @next="next"
          @exit="exit"
        />
      </template>
    </VOnboardingWrapper>
  </div>
</template>
