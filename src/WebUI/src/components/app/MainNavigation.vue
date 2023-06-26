<script setup lang="ts">
import { useUserStore } from '@/stores/user';
import Role from '@/models/role';

const userStore = useUserStore();
</script>

<template>
  <nav class="flex items-center gap-8">
    <RouterLink
      :to="{ name: 'Characters' }"
      class="text-content-300 hover:text-content-100"
      activeClass="!text-content-100"
    >
      {{ $t('nav.main.Characters') }}
    </RouterLink>

    <RouterLink
      :to="{ name: 'Shop' }"
      class="text-content-300 hover:text-content-100"
      activeClass="!text-content-100"
    >
      {{ $t('nav.main.Shop') }}
    </RouterLink>

    <div class="flex items-center gap-2">
      <RouterLink
        :to="{ name: 'Clans' }"
        class="text-content-300 hover:text-content-100"
        activeClass="!text-content-100"
      >
        {{ $t('nav.main.Clans') }}
      </RouterLink>

      <VTooltip v-if="userStore.clan === null" data-aq-main-nav-link-tooltip="Explanation">
        <Tag icon="tag" variant="primary" rounded size="sm" />
        <template #popper>
          <div class="prose prose-invert" v-html="$t('clanBalancingExplanation')"></div>
        </template>
      </VTooltip>
    </div>

    <RouterLink
      :to="{ name: 'Leaderboard' }"
      class="inline-flex items-center gap-2 text-content-300 hover:text-content-100"
      activeClass="!text-content-100"
    >
      {{ $t('nav.main.Leaderboard') }}
      <OIcon icon="trophy-cup" size="xl" class="text-more-support" />
    </RouterLink>

    <RouterLink
      v-if="[Role.Moderator, Role.Admin].includes(userStore.user!.role)"
      :to="{ name: 'Moderator' }"
      class="text-content-300 hover:text-content-100"
      activeClass="!text-content-100"
      data-aq-main-nav-link="Moderator"
    >
      {{ $t('nav.main.Moderator') }}
    </RouterLink>
  </nav>
</template>
