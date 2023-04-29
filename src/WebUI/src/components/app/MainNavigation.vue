<script setup lang="ts">
import { useUserStore } from '@/stores/user';
import { useNavigation } from '@/composables/use-main-navigation';

const userStore = useUserStore();

const { mainNavigation } = useNavigation();
</script>

<template>
  <nav class="flex items-center gap-8">
    <div v-for="navLink in mainNavigation" class="flex items-center gap-2">
      <RouterLink
        activeClass="text-content-100"
        inactiveClass="text-content-300 hover:text-content-100"
        :to="{ name: navLink.name as never }"
      >
        {{ $t(`nav.main.${navLink.name as string}`) }}
      </RouterLink>

      <VTooltip v-if="userStore.clan === null && navLink.name === 'Clans'">
        <Tag icon="tag" variant="primary" rounded size="sm" />
        <template #popper>
          <div class="prose prose-invert" v-html="$t('clanBalancingExplanation')"></div>
        </template>
      </VTooltip>
    </div>
  </nav>
</template>
