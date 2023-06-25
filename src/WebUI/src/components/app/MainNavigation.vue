<script setup lang="ts">
import { useUserStore } from '@/stores/user';
import Role from '@/models/role';

const userStore = useUserStore();
</script>

<template>
  <nav class="flex items-center gap-8">
    <RouterLink
      activeClass="text-content-100"
      inactiveClass="text-content-300 hover:text-content-100"
      :to="{ name: 'Characters' }"
    >
      {{ $t('nav.main.Characters') }}
    </RouterLink>

    <RouterLink
      activeClass="text-content-100"
      inactiveClass="text-content-300 hover:text-content-100"
      :to="{ name: 'Shop' }"
    >
      {{ $t('nav.main.Shop') }}
    </RouterLink>

    <div class="flex items-center gap-2">
      <RouterLink
        activeClass="text-content-100"
        inactiveClass="text-content-300 hover:text-content-100"
        :to="{ name: 'Clans' }"
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

    <RouterLink :to="{ name: 'Leaderboard' }" class="inline-flex items-center gap-2">
      {{ $t('nav.main.LeaderBoard') }}
      <OIcon icon="trophy-cup" size="xl" class="text-more-support" />
    </RouterLink>

    <RouterLink
      v-if="[Role.Moderator, Role.Admin].includes(userStore.user!.role)"
      activeClass="text-content-100"
      inactiveClass="text-content-300 hover:text-content-100"
      :to="{ name: 'Moderator' }"
      data-aq-main-nav-link="Moderator"
    >
      {{ $t('nav.main.Moderator') }}
    </RouterLink>
  </nav>
</template>
