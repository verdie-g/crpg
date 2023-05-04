<script setup lang="ts">
import { type ClanInvitation } from '@/models/clan';
import { respondToClanInvitation } from '@/services/clan-service';
import { notify } from '@/services/notification-service';
import { t } from '@/services/translate-service';
import { useClan } from '@/composables/clan/use-clan';
import { useClanApplications } from '@/composables/clan/use-clan-applications';
import { usePagination } from '@/composables/use-pagination';

definePage({
  props: true,
  meta: {
    layout: 'default',
    middleware: 'canManageApplications',
    roles: ['User', 'Moderator', 'Admin'],
  },
});

const props = defineProps<{
  id: string;
}>();

const { clanId } = useClan(props.id);
const { applications, loadClanApplications } = useClanApplications();
const { pageModel, perPage } = usePagination();

const respond = async (application: ClanInvitation, status: boolean) => {
  await respondToClanInvitation(clanId.value, application.id, status);
  await loadClanApplications(0, { id: clanId.value });
  status
    ? notify(t('clan.application.respond.accept.notify.success'))
    : notify(t('clan.application.respond.decline.notify.success'));
};

await loadClanApplications(0, { id: clanId.value });
</script>

<template>
  <div class="px-6 py-6">
    <OButton
      v-tooltip.bottom="$t('nav.back')"
      tag="router-link"
      :to="{ name: 'ClansId', params: { id: clanId } }"
      variant="secondary"
      size="xl"
      outlined
      rounded
      icon-left="arrow-left"
      data-aq-link="back-to-clan"
    />
    <div class="mx-auto max-w-2xl py-6">
      <h1 class="mb-14 text-center text-xl text-content-100">
        {{ $t('clan.application.page.title') }}
      </h1>

      <div class="container">
        <div class="mx-auto max-w-3xl">
          <OTable
            :data="applications"
            :perPage="perPage"
            bordered
            v-model:current-page="pageModel"
            :paginated="applications.length > perPage"
          >
            <OTableColumn
              #default="{ row: application }: { row: ClanInvitation }"
              field="name"
              :label="$t('clan.application.table.column.name')"
            >
              <UserMedia :user="application.invitee" />
            </OTableColumn>

            <OTableColumn
              #default="{ row: application }: { row: ClanInvitation }"
              field="action"
              position="right"
              :label="$t('clan.application.table.column.actions')"
              width="160"
            >
              <div class="flex items-center justify-center gap-1">
                <OButton
                  variant="primary"
                  inverted
                  :label="$t('action.accept')"
                  size="xs"
                  data-aq-clan-application-action="accept"
                  @click="respond(application, true)"
                />
                <OButton
                  variant="primary"
                  inverted
                  :label="$t('action.decline')"
                  size="xs"
                  data-aq-clan-application-action="decline"
                  @click="respond(application, false)"
                />
              </div>
            </OTableColumn>

            <template #empty>
              <ResultNotFound />
            </template>
          </OTable>
        </div>
      </div>
    </div>
  </div>
</template>
