<template>
  <section class="section">
    <div class="container" v-if="clan !== null">
      <h1 class="is-size-2">[{{ clan.tag }}] {{ clan.name }} Applications</h1>

      <b-table :data="applications" :hoverable="true">
        <b-table-column field="name" label="Name" v-slot="props">
          {{ props.row.invitee.name }}
          <platform
            :platform="props.row.invitee.platform"
            :platformUserId="props.row.invitee.platformUserId"
          />
        </b-table-column>

        <b-table-column v-slot="props">
          <b-icon icon="check" @click.native="accept(props.row)" class="is-clickable" />
          <b-icon icon="times" @click.native="decline(props.row)" class="is-clickable" />
        </b-table-column>

        <template #empty>
          <div class="has-text-centered">No applications</div>
        </template>
      </b-table>
    </div>
  </section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import * as clanService from '@/services/clan-service';
import ClanWithMembers from '@/models/clan-with-members';
import PlatformComponent from '@/components/Platform.vue';
import ClanInvitation from '@/models/clan-invitation';
import ClanInvitationStatus from '@/models/clan-invitation-status';
import ClanInvitationType from '@/models/clan-invitation-type';
import { arrayRemove } from '@/utils/array';
import { notify } from '@/services/notifications-service';

@Component({ components: { platform: PlatformComponent } })
export default class ClanApplicationsComponent extends Vue {
  clan: ClanWithMembers | null = null;
  applications: ClanInvitation[] = [];

  created() {
    const clanId = parseInt(this.$route.params.id as string);
    if (Number.isNaN(clanId)) {
      this.$router.push({ name: 'not-found' });
      return;
    }

    clanService.getClan(clanId).then(clan => (this.clan = clan));
    clanService
      .getClanInvitations(clanId, [ClanInvitationType.Request], [ClanInvitationStatus.Pending])
      .then(applications => (this.applications = applications));
  }

  accept(application: ClanInvitation) {
    this.respondToClanInvitation(application, true).then(() => notify('Application accepted'));
  }

  decline(application: ClanInvitation) {
    this.respondToClanInvitation(application, false).then(() => notify('Application declined'));
  }

  async respondToClanInvitation(application: ClanInvitation, accept: boolean) {
    await clanService.respondToClanInvitation(this.clan!.id, application.id, accept);
    arrayRemove(this.applications, ci => ci.id === application.id);
  }
}
</script>

<style scoped lang="scss"></style>
