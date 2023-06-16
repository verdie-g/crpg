<script setup lang="ts">
const { point = null } = defineProps<{ point?: number | null }>();
</script>

<template>
  <div
    class="inline-flex items-center gap-1.5 align-bottom font-bold text-primary"
    v-tooltip.bottom="$t('user.field.heirloom')"
  >
    <OIcon icon="blacksmith" size="lg" />
    <slot v-if="$slots.default" />
    <template v-else-if="point !== null">{{ $n(point) }}</template>
  </div>
</template>
