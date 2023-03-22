<script setup lang="ts">
defineProps<{ message?: string }>();
</script>

<template>
  <div class="flex flex-col items-center justify-center gap-4 py-24 text-content-400">
    <OIcon icon="error" size="4x" class="text-content-400" />
    <div class="text-xs">{{ message ?? $t('results-not-found') }}</div>
  </div>
</template>
