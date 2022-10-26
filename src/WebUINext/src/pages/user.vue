<script setup lang="ts">
import { useUserStore } from '@/stores/user';
import { logout } from '@/services/auth-service';

const { user } = toRefs(useUserStore());
</script>

<route lang="yaml">
meta:
  layout: default
  roles:
    - User
</route>

<template>
  <div>
    <h1>USER</h1>
    <div v-if="user">
      <div>name {{ user.name }}</div>
      <div>platform {{ user.platform }}</div>
      <div>gold {{ user.gold }}</div>
      <div>heirloomPoints {{ user.heirloomPoints }}</div>
      <div>role {{ user.role }}</div>
      <img :src="(user.avatarFull as string)" alt="avatar" />
      <OButton variant="primary" @click="logout">Logout</OButton>
    </div>
  </div>
</template>
