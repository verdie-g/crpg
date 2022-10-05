<script setup lang="ts">
const { t } = useI18n();
</script>

<route lang="yaml">
meta:
  layout: 404
</route>

<template>
  <div>
    {{ t('not-found') }}
  </div>
</template>
