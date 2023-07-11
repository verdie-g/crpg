<script setup lang="ts">
import { IconedBucket, IconBucketType } from '@/services/item-service';

const {
  icon,
  label,
  description,
  size = 'xl',
  showTooltip = true,
} = defineProps<{
  icon: IconedBucket;
  label: string;
  description?: string | null;
  size?: 'xl' | '2xl';
  showTooltip?: boolean;
}>();
</script>

<template>
  <Tooltip
    v-bind="{
      disabled: !showTooltip,
      placement: 'top',
      title: label,
      ...(description !== null && { description: description }),
    }"
    class="flex items-center hover:opacity-80"
  >
    <SvgSpriteImg
      v-if="icon.type === IconBucketType.Asset"
      :name="icon.name"
      viewBox="0 0 48 48"
      :class="size === '2xl' ? 'w-8' : 'w-6'"
    />

    <OIcon v-else-if="icon.type === IconBucketType.Svg" :icon="icon.name" :size="size" />
  </Tooltip>
</template>
