<script setup lang="ts">
import { ClanMemberRole } from '@/models/clan';

const props = defineProps<{
  role: ClanMemberRole;
}>();
</script>

<template>
  <OIcon
    :icon="role === ClanMemberRole.Leader ? 'leader' : 'officer'"
    :class="
      role === ClanMemberRole.Leader
        ? 'text-more-support'
        : role === ClanMemberRole.Officer
        ? 'text-content-100'
        : 'text-content-400'
    "
    size="sm"
    :style="{
      '--fa-secondary-opacity': 1,
      '--fa-secondary-color': '#fff',
      '--fa-primary-color': '#626262',
    }"
  />
</template>
