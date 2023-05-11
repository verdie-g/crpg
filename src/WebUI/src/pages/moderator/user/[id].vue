<script setup lang="ts">
import { getUserById } from '@/services/users-service';
import { moderationUserKey } from '@/symbols/moderator';

definePage({
  props: true,
  meta: {
    layout: 'default',
    roles: ['Moderator', 'Admin'],
  },
});

const props = defineProps<{ id: string }>();

const { state: user, execute: loadUser } = useAsyncState(
  () => getUserById(Number(props.id)),
  null,
  {
    immediate: false,
  }
);

provide(moderationUserKey, user);

await loadUser();
</script>

<template>
  <div class="container">
    <div class="mb-14 flex items-center justify-center gap-8">
      <h1 class="text-xl text-content-100">
        <UserMedia :user="user!" size="xl" />
      </h1>

      <div class="flex items-center justify-center gap-2">
        <RouterLink :to="{ name: 'ModeratorUserIdRestrictions' }" v-slot="{ isExactActive }">
          <OButton
            :variant="isExactActive ? 'transparent-active' : 'transparent'"
            size="lg"
            :label="$t('restriction.title')"
          />
        </RouterLink>

        <RouterLink :to="{ name: 'ModeratorUserIdActivityLogs' }" v-slot="{ isExactActive }">
          <OButton
            :variant="isExactActive ? 'transparent-active' : 'transparent'"
            size="lg"
            :label="'Logs'"
          />
        </RouterLink>
      </div>
    </div>

    <RouterView />
  </div>
</template>
