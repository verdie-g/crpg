<script setup lang="ts">
import { type Clan } from '@/models/clan';
import { useClan } from '@/composables/clan/use-clan';
import { useUserStore } from '@/stores/user';
import { updateClan } from '@/services/clan-service';
import { notify } from '@/services/notification-service';
import { t } from '@/services/translate-service';

definePage({
  props: true,
  meta: {
    layout: 'default',
    middleware: 'canManageApplications', // TODO: ['clanIdParamValidate', 'canManageApplications']
    roles: ['User', 'Moderator', 'Admin'],
  },
});

const props = defineProps<{
  id: string;
}>();

const userStore = useUserStore();
const router = useRouter();

const { clanId, clan, loadClan } = useClan(props.id);

const onSubmit = async (form: Omit<Clan, 'id'>) => {
  const clan = await updateClan(clanId.value, { ...form, id: clanId.value });
  await userStore.getUserClan();
  notify(t('clan.update.notify.success'));
  router.replace({ name: 'ClansId', params: { id: clan.id } });
};

await loadClan(0, { id: clanId.value });
</script>

<template>
  <div class="px-6 py-6">
    <RouterLink :to="{ name: 'ClansId', params: { id: clanId } }">
      <OButton
        v-tooltip.bottom="$t('nav.back')"
        variant="secondary"
        size="xl"
        outlined
        rounded
        icon-left="arrow-left"
      />
    </RouterLink>

    <div class="mx-auto max-w-2xl py-6">
      <h1 class="mb-14 text-center text-xl text-content-100">
        {{ $t('clan.update.page.title') }}
      </h1>

      <div class="container">
        <div class="mx-auto max-w-3xl">
          <ClanForm :clanId="clanId" :clan="clan!" @submit="onSubmit" />
        </div>
      </div>
    </div>
  </div>
</template>
