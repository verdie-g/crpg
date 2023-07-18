<script setup lang="ts">
import { useTimeAgo } from '@vueuse/core';
import { type ReleaseNote } from '@/models/release-note';

const { releaseNotes = [] } = defineProps<{ releaseNotes: ReleaseNote[] }>();

const latestRelease = computed(() => releaseNotes[1]);

// TODO: i18n? https://github.com/vueuse/vueuse/issues/1592
const timeAgo = useTimeAgo(new Date(latestRelease.value.createdAt));
</script>

<template>
  <div class="absolute left-6 top-6 gap-6 space-y-1.5">
    <a
      :href="latestRelease.url"
      target="_blank"
      class="group flex flex-col gap-1 rounded-full bg-base-500/20 px-5 pb-2.5 pt-4 shadow-xl hover:shadow-none"
    >
      <div class="flex items-center gap-2">
        <OIcon icon="trumpet" size="xl" class="text-primary" />
        <div
          class="max-w-[18rem] overflow-hidden overflow-ellipsis whitespace-nowrap font-bold text-content-100 group-hover:text-content-200"
        >
          {{ latestRelease.title }}
        </div>
        <Tag variant="primary" :label="$t('releaseNotes.latest')" />
      </div>
      <div class="pl-8 text-[0.7rem] leading-none">{{ timeAgo }}</div>
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
