<template>
  <section class="section">
    <div class="container" v-if="clan !== null">
      <h1 class="is-size-2">[{{ clan.tag }}] {{ clan.name }}</h1>
      <b-table :data="clan.members" :hoverable="true">
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
import ClanWithMembers from '@/models/clan-with-members';
import PlatformComponent from '@/components/Platform.vue';
import ClanMember from '@/models/clan-member';
import clanModule from '@/store/clan-module';
import { notify } from '@/services/notifications-service';
import { arrayRemove } from '@/utils/array';
import userModule from '@/store/user-module';
import ClanMemberRole from '@/models/clan-member-role';

@Component({ components: { platform: PlatformComponent } })
export default class ClanComponent extends Vue {
  clan: ClanWithMembers | null = null;

  async created() {
    const clanId = parseInt(this.$route.params.id as string);
    if (Number.isNaN(clanId)) {
      this.$router.push({ name: 'not-found' });
      return;
    }

    this.clan = await clanService.getClan(clanId);
  }

  isMemberSelf(member: ClanMember): boolean {
    return member.user.id === userModule.user!.id;
  }

  memberKickable(member: ClanMember): boolean {
    // Current user not loaded yet.
    if (userModule.user === null) {
      return false;
    }

    if (
      this.isMemberSelf(member) &&
      (member.role !== ClanMemberRole.Leader || this.clan!.members.length === 1)
    ) {
      return true;
    }

    // Check that the current user is a member of the clan.
    const selfMember = this.clan!.members.find(m => m.user.id === userModule.user!.id);
    if (selfMember === undefined) {
      return false;
    }

    return (
      (selfMember.role === ClanMemberRole.Leader &&
        (member.role == ClanMemberRole.Admin || member.role == ClanMemberRole.Member)) ||
      (selfMember.role === ClanMemberRole.Admin && member.role == ClanMemberRole.Member)
    );
  }

  async kickMember(member: ClanMember) {
    await clanModule.kickClanMember({ clanId: this.clan!.id, userId: member.user.id });
    arrayRemove(this.clan!.members, m => m === member);
    if (this.isMemberSelf(member)) {
      notify('Clan left');
      this.$router.push({ name: 'clans' });
    } else {
      notify('Clan member kicked');
    }
  }
}
</script>

<style scoped lang="scss"></style>
