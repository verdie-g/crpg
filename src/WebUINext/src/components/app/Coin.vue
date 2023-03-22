<script setup lang="ts">
defineProps<{ value: number }>();
</script>

<template>
  <span class="inline-flex gap-1.5 align-middle font-bold text-primary">
    <img class="m-0 w-4" :src="getAssetUrl('themes/oruga-tailwind/img/coin.svg')" />
    {{ $n(value) }}
  </span>
</template>
