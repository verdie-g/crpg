<script setup lang="ts">
import { userManager } from '@/services/auth-service';
const router = useRouter();

onMounted(async () => {
  await userManager.signinCallback();
  router.replace({ name: 'user' });
});
</script>

<route lang="yaml">
meta:
  layout: default
  skipAuth: true
</route>

<template></template>
