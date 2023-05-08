<script setup lang="ts">
const props = defineProps<{
  step?: {
    content?: {
      title?: string;
      description?: string;
    };
  };
  isFirst: boolean;
  isLast: boolean;
  index: number;
  total: number;
}>();

const emit = defineEmits<{
  (e: 'previous'): void;
  (e: 'next'): void;
  (e: 'exit'): void;
}>();
</script>

<template>
  <VOnboardingStep>
    <div class="px-4 py-5 rounded-lg bg-base-300 relative">
      <OButton
        class="!absolute right-4 top-4"
        iconRight="close"
        rounded
        size="2xs"
        variant="secondary"
        @click="emit('exit')"
      />

      <div class="space-y-2">
        <Tag :label="`${index + 1}/${total}`" />

        <div v-if="step && step.content" class="prose prose-invert max-w-sm">
          <h3 v-if="step.content.title" class="pr-12">
            {{ step.content.title }}
          </h3>

          <div v-if="step.content.description">
            <p>{{ step.content.description }}</p>
          </div>
        </div>

        <div class="relative flex gap-4 items-center">
          <template v-if="!isFirst">
            <OButton size="xs" variant="primary" @click="emit('previous')" label="Previous" />
          </template>

          <OButton
            size="xs"
            variant="primary"
            :label="isLast ? 'Finish' : 'Next'"
            @click="emit('next')"
          />
        </div>
      </div>
    </div>
  </VOnboardingStep>
</template>
