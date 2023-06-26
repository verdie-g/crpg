<script setup lang="ts">
const { label, value, tooltip } = defineProps<{
  label: string;
  value?: string;
  tooltip?: { title: string; description?: string };
}>();
</script>

<template>
  <Tooltip v-bind="tooltip" :disabled="tooltip === undefined">
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
  </Tooltip>
</template>
