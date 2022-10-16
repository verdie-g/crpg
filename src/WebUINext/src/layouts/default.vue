<script setup lang="ts">
import { useUserStore } from '@/stores/user';
const { user } = toRefs(useUserStore());
</script>

<template>
  <main>
    <div class="flex">
      <div class="flex gap-4">
        <OButton tag="router-link" variant="primary" :to="{ name: 'index' }">Index</OButton>
        <OButton tag="router-link" variant="primary" :to="{ name: 'user' }">User</OButton>
        <OButton tag="router-link" variant="primary" :to="{ name: 'admin' }">Admin</OButton>
      </div>
      <div>
        <div v-if="user">{{ user.name }}</div>
      </div>
    </div>
    <RouterView />
  </main>
</template>
