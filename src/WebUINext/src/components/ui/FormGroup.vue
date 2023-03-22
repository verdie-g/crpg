<script setup lang="ts">
const props = withDefaults(
  defineProps<{
    icon?: string;
    label?: string;
    collapsed?: boolean;
    collapsable?: boolean;
    bordered?: boolean;
  }>(),
  {
    collapsed: false,
    collapsable: true,
    bordered: true,
  }
);

const collapsedModel = ref<boolean>(props.collapsed);
</script>

<template>
  <div class="rounded-3xl px-6 py-7" :class="{ 'border border-border-200': bordered }">
    <div
      class="group flex items-center justify-between gap-4 text-content-100"
      :class="{ 'cursor-pointer': collapsable }"
      @click="collapsable && (collapsedModel = !collapsedModel)"
    >
      <div class="flex w-full items-center gap-3 text-title-lg">
        <slot v-if="$slots.label" name="label" />
        <template v-else>
          <OIcon v-if="icon" :icon="icon" size="lg" />
          {{ label }}
        </template>
      </div>

      <OIcon
        v-if="collapsable"
        icon="chevron-down"
        size="lg"
        class="transform text-content-400 group-hover:text-content-100"
        :class="{ 'rotate-180': collapsedModel }"
      />
    </div>

    <div v-if="!collapsedModel" class="mt-7 flex flex-col gap-4">
      <slot name="default"></slot>
    </div>
  </div>
</template>
