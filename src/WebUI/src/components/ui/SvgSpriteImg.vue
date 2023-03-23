<script setup lang="ts">
const props = withDefaults(defineProps<{ prefix?: string; name: string; viewBox: string }>(), {
  prefix: 'icon',
});

const symbolId = computed(() => `#${props.prefix}-${props.name}`);
</script>

<template>
  <svg aria-hidden="true" :viewBox="viewBox">
    <use :href="symbolId" />
  </svg>
</template>
