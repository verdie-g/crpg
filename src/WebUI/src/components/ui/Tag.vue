<script setup lang="ts">
type BadgeVariant = 'primary' | 'info' | 'success' | 'warning' | 'danger';
type BadgeSize = 'sm' | 'lg';

const props = withDefaults(
  defineProps<{
    variant?: BadgeVariant;
    size?: BadgeSize;
    rounded?: boolean;
    disabled?: boolean;
    label?: string;
    icon?: string;
  }>(),
  {
    variant: 'info',
    size: 'sm',
    rounded: false,
    disabled: false,
  }
);

const computedClass = computed(() => {
  const variants: Record<BadgeVariant, string> = /*@tw*/ {
    primary: 'bg-base-200 text-primary hover:bg-base-300 hover:text-primary-hover',
    info: 'bg-base-200 text-content-200 hover:bg-base-500 hover:text-content-100',
    success: 'bg-base-200 text-status-success hover:bg-status-success hover:text-content-600',
    warning: 'bg-base-200 text-status-warning hover:bg-status-warning hover:text-content-600',
    danger: 'bg-base-200 text-status-danger hover:bg-status-danger hover:text-content-600',
  };

  const sizes: Record<BadgeSize, string> = /*@tw*/ {
    sm: 'text-2xs',
    lg: 'text-xs',
  };

  const classes = [variants[props.variant], sizes[props.size]];

  if (props.rounded) {
    if (props.size === 'lg') {
      classes.push('h-7 w-7');
    } else {
      classes.push('h-5 w-5');
    }
  } else {
    classes.push('px-1.5 py-0.5');
  }

  if (props.disabled) {
    classes.push('pointer-events-none');
  }

  return classes;
});
</script>

<template>
  <div
    class="!inline-flex cursor-pointer items-center justify-center rounded-full"
    :class="computedClass"
  >
    <template v-if="label !== undefined">{{ label }}</template>
    <OIcon v-if="icon !== undefined" :icon="icon" :size="size === 'sm' ? 'xs' : 'sm'" />
  </div>
</template>
