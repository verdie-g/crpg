<script setup lang="ts">
const { title, description } = defineProps<{
  title?: string;
  description?: string;
}>();
</script>

<template>
  <VTooltip placement="auto">
    <slot />

    <template #popper>
      <div class="prose prose-invert space-y-3">
        <h5 v-if="title" class="text-content-100">{{ title }}</h5>
        <div v-if="description" v-html="description" class="text-2xs" />
      </div>
    </template>
  </VTooltip>
</template>
