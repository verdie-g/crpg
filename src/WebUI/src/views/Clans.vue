<template>
  <section class="section">
    <div class="container">
      <div class="columns is-vcentered">
        <h1 class="column is-one-fifth clanTitle is-size-3">Clans</h1>

        <div class="column">
          <b-input
            v-model="filterText"
            placeholder="Search..."
            type="search"
            icon="search"
            size="is-medium"
          ></b-input>
        </div>

        <div class="column"></div>

        <div class="buttons column">
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
        :data="pageClans"
        :hoverable="true"
        :loading="clansLoading"
        :row-class="() => 'is-clickable'"
        @click="onRowClick"
        :per-page="clansPerPage"
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
        :total="tableClans.length"
        :current.sync="currentPage"
        :per-page="clansPerPage"
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
import { Component, Vue, Watch } from 'vue-property-decorator';
import clanModule from '@/store/clan-module';
import Clan from '@/models/clan';
import userModule from '@/store/user-module';
import ClanWithMemberCount from '@/models/clan-with-member-count';

@Component
export default class Clans extends Vue {
  clansLoading = false;
  clansPerPage = 20;
  filterText = '';
  currentPage = 1;
  tableClans = [] as ClanWithMemberCount[];
  pageClans = [] as ClanWithMemberCount[];

  get userClan(): Clan | null {
    return userModule.clan;
  }

  get userClanRoute(): string {
    return this.userClan === null ? '' : `clans/${this.userClan.id}`;
  }

  get clans(): ClanWithMemberCount[] {
    return clanModule.clans;
  }

  async created(): Promise<void> {
    this.clansLoading = true;
    clanModule.getClans().finally(() => {
      this.tableClans = this.getTableClans(this.clans);
      this.clansLoading = false;
      this.pageClans = this.getPageClans();
    });
    userModule.getUserClan();

    const currentPage = this.getCurrentPageQueryParameter();
    if (currentPage) this.currentPage = currentPage;
  }

  onRowClick(clan: ClanWithMemberCount): void {
    this.$router.push({ path: `clans/${clan.clan.id}` });
  }

  getTableClans(clans: ClanWithMemberCount[]): ClanWithMemberCount[] {
    return clans.filter(clan => {
      if (!this.filterText) return true;
      const isFilterMatchingClanName =
        clan.clan.name.toLowerCase().indexOf(this.filterText.toLowerCase()) !== -1;
      return isFilterMatchingClanName;
    });
  }

  getPageClans(): ClanWithMemberCount[] {
    const startIndex = (this.currentPage - 1) * this.clansPerPage;
    const endIndex = startIndex + this.clansPerPage;
    return this.tableClans.slice(startIndex, endIndex).filter(clan => {
      if (!this.filterText) return true;
      const isFilterMatchingClanName =
        clan.clan.name.toLowerCase().indexOf(this.filterText.toLowerCase()) !== -1;
      return isFilterMatchingClanName;
    });
  }

  updateCurrentPage(): void {
    const currentPage = this.getCurrentPageQueryParameter();

    if (!currentPage) {
      this.$router.replace('clans?page=' + 1);
      this.currentPage = 1;
    } else {
      const minPage = Math.ceil(this.tableClans.length / this.clansPerPage);
      if (currentPage > minPage) {
        this.$router.replace('clans?page=' + minPage);
        this.currentPage = minPage;
      } else {
        this.currentPage = currentPage;
      }
    }
  }

  getCurrentPageQueryParameter(): number | null {
    return this.$route.query.page ? parseInt(this.$route.query.page as string, 10) : null;
  }

  @Watch('filterText')
  onFilterTextChanged(): void {
    this.currentPage = 1;
    if (this.$router.currentRoute.query.page + '' !== '1') {
      this.$router.replace('clans?page=' + 1);
    }
    this.tableClans = this.getTableClans(this.clans);
    this.pageClans = this.getPageClans();
  }

  @Watch('$route.query.page')
  onPageQueryParameterChanged(): void {
    this.updateCurrentPage();
    this.pageClans = this.getPageClans();
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
.clanTitle {
  display: contents;
}
</style>
