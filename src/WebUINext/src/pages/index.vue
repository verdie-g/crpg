<script setup lang="ts">
import { useUserStore } from '@/stores/user';
import { login } from '@/services/auth-service';

const { user } = toRefs(useUserStore());
</script>

<route lang="yaml">
meta:
  layout: default
</route>

<template>
  <div>
    <h1>Main page</h1>
    <div>
      <div v-if="!user">
        <OButton variant="primary" @click="login">Login</OButton>
      </div>
    </div>
  </div>
</template>
