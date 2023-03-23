<script setup lang="ts">
defineProps<{ value: number }>();
</script>

<template>
  <span class="inline-flex gap-1.5 align-middle font-bold text-primary">
    <SvgSpriteImg name="coin" viewBox="0 0 18 18" class="w-4" />
    {{ $n(value) }}
  </span>
</template>
