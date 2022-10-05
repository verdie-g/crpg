<template>
  <svg aria-hidden="true" class="fill-current">
    <use :href="symbolId" />
  </svg>
</template>

<script setup lang="ts">
// ref: https://github.com/vbenjs/vite-plugin-svg-icons#vue-way
interface Props {
  prefix?: string;
  name: string;
}

const { prefix = 'icon', name } = defineProps<Props>();

const symbolId = computed(() => `#${prefix}-${name}`);
</script>
