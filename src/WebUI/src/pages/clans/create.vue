<script setup lang="ts">
import { type Clan } from '@/models/clan';
import { useUserStore } from '@/stores/user';
import { createClan } from '@/services/clan-service';
import { notify } from '@/services/notification-service';
import { t } from '@/services/translate-service';

definePage({
  meta: {
    layout: 'default',
    middleware: 'clanExistValidate',
    roles: ['User', 'Moderator', 'Admin'],
  },
});

const userStore = useUserStore();
const router = useRouter();

const onSubmit = async (form: Omit<Clan, 'id'>) => {
  const clan = await createClan(form);
  await userStore.getUserClan();
  notify(t('clan.create.notify.success'));
  return router.replace({ name: 'ClansId', params: { id: clan.id } });
};
</script>

<template>
  <div class="px-6 py-6">
    <div class="mx-auto max-w-2xl py-6">
      <h1 class="mb-14 text-center text-xl text-content-100">
        {{ $t('clan.create.page.title') }}
      </h1>

      <div class="container">
        <div class="mx-auto max-w-3xl">
          <ClanForm @submit="onSubmit" />
        </div>
      </div>
    </div>
  </div>
</template>
