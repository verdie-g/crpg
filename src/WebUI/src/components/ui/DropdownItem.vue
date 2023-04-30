<script setup lang="ts">
withDefaults(
  defineProps<{
    active?: boolean;
    checked?: boolean;
    tag?: string;
    label?: string;
    icon?: string; // TODO: add size prop
  }>(),
  {
    active: false,
    checked: false,
    tag: 'div',
  }
);
</script>

<template>
  <component
    :is="tag"
    class="flex cursor-pointer flex-wrap items-center gap-3 bg-base-200 px-5 py-3"
    :class="[
      active || checked
        ? 'text-content-100 hover:text-content-200'
        : 'text-content-300 hover:text-content-100',
    ]"
  >
    <slot v-if="$slots.default" />

    <template v-else>
      <OIcon v-if="icon" :icon="icon" size="sm" />
      <div v-if="label">{{ label }}</div>
    </template>

    <FontAwesomeLayers v-if="checked" class="fa-lg">
      <FontAwesomeIcon :icon="['crpg', 'fa-circle']" size="lg" class="text-content-100" />
      <FontAwesomeIcon :icon="['crpg', 'fa-check']" size="sm" class="text-base-200" />
    </FontAwesomeLayers>
  </component>
</template>
