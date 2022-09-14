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
            {{ $t('clanApplications') }}
          </b-button>
          <b-button
            v-else
            type="is-primary"
            size="is-medium"
            @click="apply"
            :disabled="applicationSent"
          >
            {{ $t('clanApplyToJoin') }}
          </b-button>
        </div>
      </div>

      <b-table :data="members" :hoverable="true">
        <b-table-column field="name" :label="$t('clanName')" v-slot="props">
          {{ props.row.user.name }}
          <platform
            :platform="props.row.user.platform"
            :platformUserId="props.row.user.platformUserId"
          />
        </b-table-column>

        <b-table-column field="role" :label="$t('clanRole')" v-slot="props">
          {{ props.row.role }}
        </b-table-column>

        <b-table-column v-slot="props">
          <div v-if="memberKickable(props.row)" class="is-flex is-justify-content-end">
            <b-tooltip position="is-top">
              <b-icon
                icon="cog"
                class="action-icon__hover is-clickable"
                @click.native="selected(props.row)"
              />
              <template v-slot:content>{{ $t('clanManageClanMember') }}</template>
            </b-tooltip>
          </div>
        </b-table-column>
      </b-table>
    </div>

    <b-modal display="inline-block" v-model="isManageMemberWindowActive">
      <div v-if="selectedMember && selectedMember.user" class="card">
        <div class="card-header is-align-items-center px-3 py-3">
          <b-icon icon="user-cog" size="is-large" class="mr-2" />
          <h2 class="title is-3">
            {{ $t('clanManageClanMemberWithMembername', { username: selectedMember.user.name }) }}
          </h2>
        </div>

        <div class="card-content">
          <div class="columns is-flex-direction-column px-1">
            <div class="pt-3 pb-4">
              <b-field label="Role">
                <b-radio v-model="selectedMemberRole" native-value="Member">
                  {{ $t('clanMember') }}
                </b-radio>
                <b-radio v-model="selectedMemberRole" native-value="Officer">
                  {{ $t('clanOfficer') }}
                </b-radio>
                <b-radio v-model="selectedMemberRole" native-value="Leader">
                  {{ $t('clanLeader') }}
                </b-radio>
              </b-field>
            </div>

            <div>
              <b-button
                icon-left="user-minus"
                type="is-danger"
                class="is-clickable mt-5"
                @click.native="kickMember(selectedMember)"
              >
                {{ $t('clanKickMember') }}
              </b-button>
            </div>
          </div>
        </div>
      </div>
    </b-modal>
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
  isManageMemberWindowActive = false;
  selectedMember: ClanMember | null = null;

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

    return selfMember.role === ClanMemberRole.Officer || selfMember.role === ClanMemberRole.Leader;
  }

  set selectedMemberRole(role: ClanMemberRole | undefined) {
    const member = this.selectedMember;
    if (!member || !role) {
      return;
    }
    this.updateMember(member, role);
  }

  get selectedMemberRole(): ClanMemberRole | undefined {
    return this.selectedMember?.role;
  }

  selected(member: ClanMember) {
    this.selectedMember = member;
    this.isManageMemberWindowActive = true;
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
    if (member === null) {
      return false;
    }

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
        (member.role == ClanMemberRole.Officer || member.role == ClanMemberRole.Member)) ||
      (selfMember.role === ClanMemberRole.Officer && member.role == ClanMemberRole.Member)
    );
  }

  apply() {
    this.applicationSent = true;
    clanService
      .inviteToClan(this.clan!.id, userModule.user!.id)
      .then(() => notify(this.$t('clanApplicationSent').toString()));
  }

  async kickMember(member: ClanMember) {
    await clanModule.kickClanMember({ clanId: this.clan!.id, userId: member.user.id });
    if (member.user.id === this.selfMember?.user.id) {
      notify(this.$t('clanClanLeft').toString());
      this.$router.push({ name: 'clans' });
    } else {
      notify(this.$t('clanMemberKicked').toString());
      arrayRemove(this.members, m => m === member);
    }
    this.selectedMember = null;
    this.isManageMemberWindowActive = false;
  }

  async updateMember(member: ClanMember, selectedRole: ClanMemberRole) {
    await clanModule
      .updateClanMember({
        clanId: this.clan!.id,
        memberId: member.user.id,
        role: selectedRole,
      })
      .then(() => notify(this.$t('clanMemberUpdated').toString()));
    clanService.getClanMembers(this.clan!.id).then(m => (this.members = m));
    this.isManageMemberWindowActive = false;
  }
}
</script>

<style scoped lang="scss">
.action-icon__hover {
  opacity: 0.4;
  transition: opacity 250ms;
  &:hover {
    opacity: 1;
  }
}
</style>
