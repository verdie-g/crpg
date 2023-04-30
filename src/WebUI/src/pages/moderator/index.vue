<script setup lang="ts">
import { getRestrictions } from '@/services/restriction-service';

definePage({
  meta: {
    layout: 'default',
    roles: ['Moderator', 'Admin'],
  },
});

const { state: restrictions, execute: loadRestrictions } = useAsyncState(
  () => getRestrictions(),
  [],
  {
    immediate: false,
  }
);

await loadRestrictions();
</script>

<template>
  <div class="container">
    <div class="mb-16">
      <RestrictionsTable :restrictions="restrictions" />
    </div>
  </div>
</template>
