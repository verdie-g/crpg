<script setup lang="ts">
import { useAsyncState } from '@vueuse/core';
import { getUserRestrictions, getUserById } from '@/services/users-service';

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

const { state: restrictions, execute: loadRestrictions } = useAsyncState(
  () => getUserRestrictions(Number(props.id)),
  [],
  {
    immediate: false,
  }
);

await Promise.all([loadRestrictions(), loadUser()]);
</script>

<template>
  <div class="container">
    <h1 class="mb-14 flex items-center justify-center gap-4 text-center text-xl text-content-100">
      {{ $t('restriction.user.title') }}
      <UserMedia :user="user!" size="xl" />
    </h1>

    <section class="max-w-3xl">
      <h2 class="mb-8 text-lg">{{ $t('restriction.create.form.title') }}</h2>

      <CreateRestrictionForm
        class="rounded-xl border border-border-200 p-6"
        :userId="Number(props.id)"
        @restrictionCreated="loadRestrictions"
      />
    </section>

    <section class="mb-8">
      <h2 class="mb-8 text-lg">{{ $t('restriction.user.history') }}</h2>

      <RestrictionsTable :restrictions="restrictions" :hiddenCols="['restrictedUser']" />
    </section>
  </div>
</template>
