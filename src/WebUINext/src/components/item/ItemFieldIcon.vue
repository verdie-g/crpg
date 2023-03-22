<script setup lang="ts">
import { IconedBucket, IconBucketType } from '@/services/item-service';

const props = withDefaults(
  defineProps<{
    icon: IconedBucket;
    label: string;
    size?: 'xl' | '2xl';
    showTooltip?: boolean;
  }>(),
  {
    size: 'xl',
    showTooltip: true,
  }
);

const cultureIcons = import.meta.glob('@/assets/themes/oruga-tailwind/img/culture/*.svg', {
  eager: true,
});
</script>

<!-- TODO: -->
<template>
  <div v-tooltip="showTooltip ? label : null" class="flex items-center">
    <template v-if="icon.type === IconBucketType.Asset">
      <img
        :src="
          cultureIcons[`/src/assets/themes/oruga-tailwind/img/culture/${icon.name}.svg`].default
        "
        class="object-contain"
        :class="size === '2xl' ? 'w-8' : 'w-6'"
      />
    </template>

    <template v-else-if="icon.type === IconBucketType.Svg">
      <OIcon :icon="icon.name" :size="size" />
    </template>
  </div>
</template>
