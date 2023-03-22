<script setup lang="ts">
import Coin from '@/assets/themes/oruga-tailwind/img/coin.svg';

defineProps<{ value: number }>();
</script>

<template>
  <span class="inline-flex gap-1.5 align-middle font-bold text-primary">
    <img class="m-0 w-4" :src="Coin" />
    {{ $n(value) }}
  </span>
</template>
