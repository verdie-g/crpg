<template>
  <section class="section">
    <div class="container">
      <div class="columns is-vcentered">
        <h1 class="column is-size-2">Clans</h1>
        <div class="column is-narrow">
          <b-button
            type="is-link"
            size="is-medium"
            tag="router-link"
            :to="userClanRoute"
            :disabled="userClan === null"
          >
            My clan
          </b-button>
          <b-button
            type="is-link"
            size="is-medium"
            tag="router-link"
            :to="{ name: 'clan-create' }"
            :disabled="userClan !== null"
          >
            Create new clan
          </b-button>
        </div>
      </div>

      <b-table
        :data="clans"
        :hoverable="true"
        :loading="clansLoading"
        :row-class="() => 'is-clickable'"
        @click="onRowClick"
      >
        <b-table-column field="tag" label="Tag" width="100" v-slot="props">
          <div class="box clan-color" :style="`background-color: ${props.row.clan.color}`"></div>
          {{ props.row.clan.tag }}
        </b-table-column>

        <b-table-column field="name" label="Name" v-slot="props">
          {{ props.row.clan.name }}
        </b-table-column>

        <b-table-column field="memberCount" label="Members" v-slot="props">
          {{ props.row.memberCount }}
        </b-table-column>

        <template #empty>
          <div class="has-text-centered">No clans</div>
        </template>
      </b-table>
    </div>
  </section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import clanModule from '@/store/clan-module';
import Clan from '@/models/clan';
import userModule from '@/store/user-module';
import ClanWithMemberCount from '@/models/clan-with-member-count';

@Component
export default class Clans extends Vue {
  clansLoading = false;

  get userClan(): Clan | null {
    return userModule.clan;
  }

  get userClanRoute(): string {
    return this.userClan === null ? '' : `clans/${this.userClan.id}`;
  }

  get clans(): ClanWithMemberCount[] {
    return clanModule.clans;
  }

  created(): void {
    this.clansLoading = true;
    clanModule.getClans().finally(() => (this.clansLoading = false));
    userModule.getUserClan();
  }

  onRowClick(clan: ClanWithMemberCount): void {
    this.$router.push({ path: `clans/${clan.clan.id}` });
  }
}
</script>

<style scoped lang="scss">
.clan-color {
  display: inline-block;
  padding: 0;
  width: 20px;
  height: 20px;
  vertical-align: text-bottom;
}
</style>
