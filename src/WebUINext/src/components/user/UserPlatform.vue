<script setup lang="ts">
import Platform from '@/models/platform';

const props = defineProps<{
  platform: Platform;
  platformUserId: number | string;
}>();
</script>

<!-- TODO: Epic, GoG -->
<template>
  <a
    :href="`https://steamcommunity.com/profiles/${platformUserId}`"
    class="inline-flex hover:opacity-80"
    target="_blank"
    @click.stop
  >
    <OIcon
      icon="steam-duotone"
      size="sm"
      :style="{
        '--fa-secondary-opacity': 1,
        '--fa-secondary-color': '#2F3944',
        '--fa-primary-color': '#fff',
      }"
    />
  </a>
</template>
