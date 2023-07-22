<script setup lang="ts">
const {
  title,
  description,
  disabled = false,
} = defineProps<{
  title?: string | null | undefined;
  description?: string | null | undefined;
  disabled?: boolean;
}>();

const isDisabled = computed(
  () => disabled || title === undefined || title === null || title === ''
);
</script>

<template>
  <VTooltip placement="auto" :disabled="isDisabled">
    <slot />

    <template v-if="!isDisabled" #popper>
      <div class="prose prose-invert space-y-3">
        <h5 class="text-content-100">
          {{ title }}
        </h5>
        <div
          v-if="description !== undefined && description !== null && description !== ''"
          v-html="description"
          class="text-2xs"
        />
      </div>
    </template>
  </VTooltip>
</template>
