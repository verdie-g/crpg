<script setup lang="ts">
definePage({
  meta: {
    layout: 'default',
    roles: ['Moderator', 'Admin'],
  },
});
</script>

<template>
  <div class="container">
    <div class="mx-auto max-w-3xl">
      <UserFinder />
    </div>
  </div>
</template>
