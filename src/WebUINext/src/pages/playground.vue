<script setup lang="ts"></script>

<route lang="yaml">
meta:
  layout: playground
</route>

<template>
  <div>
    <div class="mb-12 flex flex-wrap gap-4">
      <router-link :to="{ name: 'playground-oruga' }">oruga-playground</router-link>
    </div>
    <RouterView />
  </div>
</template>
