<script setup lang="ts">
withDefaults(defineProps<{ value?: number }>(), { value: 0 });
</script>

<template>
  <span class="inline-flex gap-1.5 align-middle font-bold text-primary">
    <SvgSpriteImg name="coin" viewBox="0 0 18 18" class="w-4" />
    <slot v-if="$slots.default" />
    <template v-else>{{ $n(value) }}</template>
  </span>
</template>
