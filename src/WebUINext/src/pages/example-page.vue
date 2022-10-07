<script setup lang="ts">
console.log('example env', import.meta.env.VITE_API_BASE_URL);
</script>

<route lang="yaml">
meta:
  layout: example
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
