<script setup lang="ts">
import { userManager } from '@/services/auth-service';

onMounted(async () => {
  await userManager.signinSilentCallback();
});
</script>

<route lang="yaml">
meta:
  layout: default
  skipAuth: true
</route>

<template></template>
