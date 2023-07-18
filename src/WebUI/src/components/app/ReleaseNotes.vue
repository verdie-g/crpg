<script setup lang="ts">
import { type ReleaseNote } from '@/models/release-note';

const { releaseNotes = [] } = defineProps<{ releaseNotes: ReleaseNote[] }>();

const latestRelease = computed(() => releaseNotes[0]);
</script>

<template>
  <div class="absolute left-6 top-6 gap-6 space-y-1.5">
    <a
      :href="latestRelease.url"
      target="_blank"
      class="group flex items-center gap-2 rounded-full bg-base-500/20 p-4 shadow-xl hover:shadow-none"
    >
      <OIcon icon="trumpet" size="xl" class="text-primary" />
      <div class="font-bold text-content-100 group-hover:text-content-200">
        {{ latestRelease.title }}
      </div>
      <Tag variant="primary" :label="$t('releaseNotes.latest')" />
    </a>
    <div v-if="releaseNotes.length > 1" class="pl-5">
      <a
        href="https://github.com/verdie-g/crpg/releases"
        class="text-[0.85rem] text-content-300 underline hover:no-underline"
      >
        {{ $t('releaseNotes.showAllRelease', { count: releaseNotes.length - 1 }) }}
      </a>
    </div>
  </div>
</template>
