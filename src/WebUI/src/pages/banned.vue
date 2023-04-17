<script setup lang="ts">
definePage({
  meta: {
    layout: 'empty',
    skipAuth: true,
  },
});
</script>

<template>
  <div class="flex h-screen w-full items-center justify-center">
    <div class="mx-auto max-w-lg space-y-8 rounded-xl border border-border-200 px-6 py-10">
      <h4 class="px-12 text-center text-lg text-status-danger">{{ $t('banned.title') }}</h4>

      <UserRestrictionGuide />
    </div>
  </div>
</template>
