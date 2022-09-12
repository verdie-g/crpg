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
        id="clansTable"
        :data="pageItems"
        :hoverable="true"
        :loading="clansLoading"
        :row-class="() => 'is-clickable'"
        @click="onRowClick"
        :per-page="itemsPerPage"
        :current-page="currentPage"
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

      <b-pagination
        :total="clans.length"
        :current.sync="currentPage"
        :per-page="itemsPerPage"
        order="is-centered"
        range-before="2"
        range-after="2"
        icon-prev="chevron-left"
        aria-controls="clansTable"
      >
        <b-pagination-button
          slot-scope="props"
          :page="props.page"
          :id="`page${props.page.number}`"
          tag="router-link"
          :to="{
            name: 'clans',
            query: { ...$route.query, page: props.page.number },
          }"
        >
          {{ props.page.number }}
        </b-pagination-button>

        <b-pagination-button
          slot="previous"
          slot-scope="props"
          :page="props.page"
          tag="router-link"
          :to="{
            name: 'clans',
            query: { ...$route.query, page: props.page.number },
          }"
        >
          <b-icon icon="chevron-left" size="is-small" />
        </b-pagination-button>

        <b-pagination-button
          slot="next"
          slot-scope="props"
          :page="props.page"
          tag="router-link"
          :to="{
            name: 'clans',
            query: { ...$route.query, page: props.page.number },
          }"
        >
          <b-icon icon="chevron-right" size="is-small" />
        </b-pagination-button>
      </b-pagination>
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
  itemsPerPage = 10;

  get userClan(): Clan | null {
    return userModule.clan;
  }

  get userClanRoute(): string {
    return this.userClan === null ? '' : `clans/${this.userClan.id}`;
  }

  get rows(): number {
    return this.clans.length;
  }

  get clans(): ClanWithMemberCount[] {
    return clanModule.clans;
  }

  get currentPage(): number {
    const currentPage = this.$route.query.page
      ? parseInt(this.$route.query.page as string, 10)
      : undefined;

    if (!currentPage) {
      this.$router.replace('clans?page=' + 1);
      return 1;
    }
    const minPage = Math.ceil(this.clans.length / this.itemsPerPage);
    if (currentPage > minPage) {
      this.$router.replace('clans?page=' + minPage);
      return minPage;
    }
    return currentPage;
  }

  get pageItems(): ClanWithMemberCount[] {
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    return this.clans.slice(startIndex, endIndex);
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
