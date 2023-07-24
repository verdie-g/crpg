<script setup lang="ts">
definePage({
  meta: {
    layout: 'empty',
    skipAuth: true,
    middleware: 'signInCallback',
  },
});

console.log('signin-callback');
</script>

<template></template>
