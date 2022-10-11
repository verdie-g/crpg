<template>
  <svg aria-hidden="true" class="fill-current" :viewBox="viewBox">
    <use :href="symbolId" />
  </svg>
</template>

<script setup lang="ts">
// ref: https://github.com/vbenjs/vite-plugin-svg-icons#vue-way
interface Props {
  basePrefix?: string;
  name: string;
  viewBox: string;
}

const { basePrefix = 'icon', name, viewBox } = defineProps<Props>();

const symbolId = computed(() => `#${basePrefix}-${name}`);
</script>

<script lang="ts">
export default {
  name: 'SvgIcon',
};
</script>
