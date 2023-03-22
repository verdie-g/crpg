<script setup lang="ts">
defineProps<{ bg: string }>();
</script>

<template>
  <div class="fixed inset-0">
    <img class="h-full w-full object-cover opacity-15" :src="`/images/bg/${bg}`" />
    <div class="absolute inset-0 bg-gradient-to-t from-base-100 opacity-75"></div>
  </div>
</template>
