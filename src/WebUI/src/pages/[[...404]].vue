<script setup lang="ts">
definePage({
  name: 'PageNotFound',
  meta: {
    skipAuth: true,
    layout: 'empty',
  },
});
</script>

<template>
  <div>
    <!-- TODO: Custom design -->
    {{ $t('not-found') }}
  </div>
</template>
