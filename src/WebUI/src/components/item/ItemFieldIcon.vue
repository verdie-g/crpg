<script setup lang="ts">
import { IconedBucket, IconBucketType } from '@/services/item-service';

const { icon, size = 'xl' } = defineProps<{
  icon: IconedBucket;
  size?: 'xl' | '2xl';
}>();
</script>

<template>
  <div class="flex items-center hover:opacity-80">
    <SvgSpriteImg
      v-if="icon.type === IconBucketType.Asset"
      :name="icon.name"
      viewBox="0 0 48 48"
      :class="size === '2xl' ? 'w-8' : 'w-6'"
    />
    <OIcon v-else-if="icon.type === IconBucketType.Svg" :icon="icon.name" :size="size" />
  </div>
</template>
