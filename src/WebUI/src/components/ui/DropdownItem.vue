<script setup lang="ts">
withDefaults(defineProps<{ active?: boolean; checked?: boolean; tag?: string }>(), {
  active: false,
  checked: false,
  tag: 'div',
});
</script>

<template>
  <component
    :is="tag"
    class="flex cursor-pointer flex-wrap items-center gap-3 bg-base-200 py-3 px-5"
    :class="[
      active || checked
        ? 'text-content-100 hover:text-content-200'
        : 'text-content-300 hover:text-content-100',
    ]"
  >
    <slot />

    <FontAwesomeLayers v-if="checked" class="fa-lg">
      <FontAwesomeIcon :icon="['crpg', 'fa-circle']" size="lg" class="text-content-100" />
      <FontAwesomeIcon :icon="['crpg', 'fa-check']" size="sm" class="text-base-200" />
    </FontAwesomeLayers>
  </component>
</template>
