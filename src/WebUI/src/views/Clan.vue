<template>
  <section class="section">
    <div class="container" v-if="clan !== null">
      <div class="columns is-vcentered">
        <h1 class="column is-size-2">[{{ clan.tag }}] {{ clan.name }}</h1>
        <div class="column is-narrow">
          <b-button
            v-if="canManageApplications"
            type="is-link"
            size="is-medium"
            tag="router-link"
            :to="{ name: 'clan-applications', params: { id: $route.params.id } }"
          >
            Clan Applications
          </b-button>
          <b-button
            v-else
            type="is-primary"
            size="is-medium"
            @click="apply"
            :disabled="applicationSent"
          >
            Apply to join
          </b-button>
        </div>
      </div>

      <b-table :data="members" :hoverable="true">
        <b-table-column field="name" label="Name" v-slot="props">
          {{ props.row.user.name }}
          <platform
            :platform="props.row.user.platform"
            :platformUserId="props.row.user.platformUserId"
          />
        </b-table-column>

        <b-table-column field="role" label="Role" v-slot="props">
          {{ props.row.role }}
        </b-table-column>

        <b-table-column v-slot="props" cell-class="is-clickable">
          <b-icon
            icon="ban"
            @click.native="kickMember(props.row)"
            v-if="memberKickable(props.row)"
          />
        </b-table-column>
      </b-table>
    </div>
  </section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import * as clanService from '@/services/clan-service';
import PlatformComponent from '@/components/Platform.vue';
import Clan from '@/models/clan';
import ClanMember from '@/models/clan-member';
import clanModule from '@/store/clan-module';
import { notify } from '@/services/notifications-service';
import { arrayRemove } from '@/utils/array';
import userModule from '@/store/user-module';
import ClanMemberRole from '@/models/clan-member-role';

@Component({ components: { platform: PlatformComponent } })
export default class ClanComponent extends Vue {
  clan: Clan | null = null;
  members: ClanMember[] = [];
  applicationSent = false;

  get selfMember(): ClanMember | null {
    // Clan or current user not loaded yet.
    if (this.clan === null || userModule.user === null) {
      return null;
    }

    const selfMember = this.members.find(m => m.user.id === userModule.user!.id);
    return selfMember === undefined ? null : selfMember;
  }

  get canManageApplications(): boolean {
    const selfMember = this.selfMember;
    if (selfMember === null) {
      return false;
    }

    return selfMember.role === ClanMemberRole.Admin || selfMember.role === ClanMemberRole.Leader;
  }

  created() {
    const clanId = parseInt(this.$route.params.id as string);
    if (Number.isNaN(clanId)) {
      this.$router.push({ name: 'not-found' });
      return;
    }

    clanService.getClan(clanId).then(c => (this.clan = c));
    clanService.getClanMembers(clanId).then(m => (this.members = m));
  }

  memberKickable(member: ClanMember): boolean {
    const selfMember = this.selfMember;
    if (
      selfMember !== null &&
      member.user.id === selfMember.user.id &&
      (member.role !== ClanMemberRole.Leader || this.members.length === 1)
    ) {
      return true;
    }

    if (selfMember === null) {
      return false;
    }

    return (
      (selfMember.role === ClanMemberRole.Leader &&
        (member.role == ClanMemberRole.Admin || member.role == ClanMemberRole.Member)) ||
      (selfMember.role === ClanMemberRole.Admin && member.role == ClanMemberRole.Member)
    );
  }

  apply() {
    this.applicationSent = true;
    clanService
      .inviteToClan(this.clan!.id, userModule.user!.id)
      .then(() => notify('Application sent!'));
  }

  async kickMember(member: ClanMember) {
    await clanModule.kickClanMember({ clanId: this.clan!.id, userId: member.user.id });
    if (member.user.id === this.selfMember?.user.id) {
      notify('Clan left');
      this.$router.push({ name: 'clans' });
    } else {
      notify('Clan member kicked');
      arrayRemove(this.members, m => m === member);
    }
  }
}
</script>

<style scoped lang="scss"></style>
