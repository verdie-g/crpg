<script setup lang="ts">
definePage({
  meta: {
    bg: 'background-2.webp',
    layout: 'default',
    middleware: 'activeCharacterRedirect',
    roles: ['User', 'Moderator', 'Admin'],
  },
});
</script>

<template>
  <div class="flex h-full min-h-[500px] w-full items-center justify-center">
    <div class="space-y-8">
      <Divider />

      <div class="prose prose-invert text-center">
        <h3 class="text-xl text-content-100">{{ $t('character.empty.title') }}</h3>
        <div>{{ $t('character.empty.desc') }}</div>
      </div>

      <Divider />
    </div>
  </div>
</template>
