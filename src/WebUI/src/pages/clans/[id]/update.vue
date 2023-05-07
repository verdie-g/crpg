<script setup lang="ts">
import { type Clan } from '@/models/clan';
import { useClan } from '@/composables/clan/use-clan';
import { useClanMembers } from '@/composables/clan/use-clan-members';
import { useUserStore } from '@/stores/user';
import { updateClan, kickClanMember } from '@/services/clan-service';
import { notify } from '@/services/notification-service';
import { t } from '@/services/translate-service';

definePage({
  props: true,
  meta: {
    layout: 'default',
    middleware: 'canUpdateClan', // TODO: ['clanIdParamValidate', 'canManageApplications']
    roles: ['User', 'Moderator', 'Admin'],
  },
});

const props = defineProps<{
  id: string;
}>();

const userStore = useUserStore();
const router = useRouter();

const { clanId, clan, loadClan } = useClan(props.id);
const { loadClanMembers, isLastMember } = useClanMembers();

const onSubmit = async (form: Omit<Clan, 'id'>) => {
  const clan = await updateClan(clanId.value, { ...form, id: clanId.value });
  await userStore.getUserClanAndRole();
  notify(t('clan.update.notify.success'));
  router.replace({ name: 'ClansId', params: { id: clan.id } });
};

const deleteClan = async () => {
  await kickClanMember(clanId.value, userStore.user!.id); // delete yourself from the clan as the only member === delete the clan
  userStore.getUserClanAndRole();
  notify(t('clan.delete.notify.success'));
  return router.replace({ name: 'Clans' });
};

await Promise.all([loadClan(0, { id: clanId.value }), loadClanMembers(0, { id: clanId.value })]);
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

    <div class="mx-auto max-w-2xl space-y-10 py-6">
      <div class="space-y-14">
        <h1 class="text-center text-xl text-content-100">
          {{ $t('clan.update.page.title') }}
        </h1>

        <div class="container">
          <div class="mx-auto max-w-3xl">
            <ClanForm :clanId="clanId" :clan="clan!" @submit="onSubmit" />
          </div>
        </div>
      </div>

      <i18n-t scope="global" keypath="clan.delete.title" tag="div" class="text-center">
        <template #link>
          <Modal closable>
            <span class="cursor-pointer text-status-danger hover:text-opacity-80">
              {{ $t('clan.delete.link') }}
            </span>
            <template #popper="{ hide }">
              <ConfirmActionForm
                v-if="isLastMember"
                :title="$t('clan.delete.dialog.title')"
                :description="$t('clan.delete.dialog.desc')"
                :name="clan!.name"
                :confirmLabel="$t('action.delete')"
                data-aq-clan-delete-confirm-action-form
                @cancel="hide"
                @confirm="
                  () => {
                    deleteClan();
                    hide();
                  }
                "
              />
              <div
                v-else
                class="px-12 pb-11 pt-20 text-center"
                data-aq-clan-delete-required-message
              >
                {{ $t('clan.delete.required') }}
              </div>
            </template>
          </Modal>
        </template>
      </i18n-t>
    </div>
  </div>
</template>
