<script setup lang="ts">
console.log('ex. env', import.meta.env.VITE_API_BASE_URL);
</script>

<route lang="yaml">
meta:
  layout: default
</route>

<template>
  <div>
    <div>Example page</div>
    <br />
    <br />
    <Example msg="Crpg" />
    <br />
    <br />
  </div>
</template>
