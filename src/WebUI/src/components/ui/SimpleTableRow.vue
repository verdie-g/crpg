<script setup lang="ts">
interface Tooltip {
  title: string;
  description?: string;
}

const props = defineProps<{
  label: string;
  value?: string;
  tooltip?: Tooltip;
}>();
</script>

<template>
  <VTooltip placement="auto" :disabled="tooltip === undefined">
    <div class="flex flex-wrap items-center justify-between gap-3 px-3 py-2.5 hover:bg-base-200">
      <div class="flex-1 text-2xs">
        {{ label }}
      </div>

      <slot v-if="$slots.default" />
      <div v-else class="text-right text-xs text-content-100">
        {{ value }}
      </div>

      <slot name="message" />
    </div>

    <template #popper>
      <div v-if="tooltip !== undefined" class="prose prose-invert space-y-3">
        <h5 class="text-content-100">{{ tooltip.title }}</h5>
        <div v-if="tooltip?.description" v-html="tooltip.description" class="text-2xs" />
      </div>
    </template>
  </VTooltip>
</template>
