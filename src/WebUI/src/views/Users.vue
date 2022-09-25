<template>
  <section class="section">
    <div class="container">
      <div class="columns is-vcentered">
        <h1 class="column is-size-2">Users</h1>
        <div class="column is-narrow">
          <b-field>
            <p class="control">
              <b-input
                placeholder="Search..."
                type="search"
                icon="search"
                size="is-medium"
                v-model.lazy="usersSearchQuery"
              />
            </p>
          </b-field>
        </div>
      </div>

      <b-table
        id="usersTable"
        :data="pageUsers"
        :hoverable="true"
        :loading="usersLoading"
        :row-class="() => 'is-clickable'"
        @click="onRowClick"
        :per-page="usersPerPage"
        :current-page="currentPage"
      >
        <b-table-column field="name" label="Name" v-slot="props">
          <b-field>
            <figure class="image is-32x32" style="margin: 0 5px">
              <img class="is-rounded" alt="avatar" :src="props.row.avatarSmall" />
            </figure>
            <span>{{ props.row.name }}</span>
          </b-field>
        </b-table-column>

        <template #empty>
          <div class="has-text-centered">No users</div>
        </template>
      </b-table>

      <b-pagination
        :total="filteredUsers.length"
        :current.sync="currentPage"
        :per-page="usersPerPage"
        order="is-centered"
        range-before="2"
        range-after="2"
        icon-prev="chevron-left"
        aria-controls="usersTable"
      >
        <b-pagination-button
          slot-scope="props"
          :page="props.page"
          :id="`page${props.page.number}`"
          tag="router-link"
          :to="{
            name: 'users',
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
            name: 'users',
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
            name: 'users',
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
import User from '@/models/user';
import userModule from '@/store/user-module';
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class UserComponent extends Vue {
  usersSearchQuery = '';
  usersLoading = false;
  usersPerPage = 20;
  users: User[] = [];

  async created() {
    const user = await userModule.getUser();
    const user1 = {
      id: 1232131,
      name: 'unrated',
      avatarMedium: user.avatarMedium,
      avatarSmall: user.avatarSmall,
    } as User;

    const user2 = {
      id: 1232131,
      name: 'buttergolem',
      avatarMedium: user.avatarMedium,
      avatarSmall: user.avatarSmall,
    } as User;

    const user3 = {
      id: 1232131,
      name: 'barsloch',
      avatarMedium: user.avatarMedium,
      avatarSmall: user.avatarSmall,
    } as User;
    for (let index = 0; index < 500; index++) {
      this.users.push(user1, user2, user3);
    }
  }

  get filteredUsers(): User[] {
    if (this.usersSearchQuery.length === 0) {
      return this.users;
    }

    return this.users.filter(user =>
      user.name.toLowerCase().includes(this.usersSearchQuery.toLowerCase())
    );
  }

  get currentPage(): number {
    const currentPage = this.$route.query.page
      ? parseInt(this.$route.query.page as string, 10)
      : undefined;

    if (!currentPage) {
      this.$router.replace('users?page=' + 1);
      return 1;
    }
    const minPage = Math.ceil(this.filteredUsers.length / this.usersPerPage);
    if (currentPage > minPage) {
      this.$router.replace('users?page=' + minPage);
      return minPage;
    }
    return currentPage;
  }

  get pageUsers(): User[] {
    const startIndex = (this.currentPage - 1) * this.usersPerPage;
    const endIndex = startIndex + this.usersPerPage;
    return this.filteredUsers.slice(startIndex, endIndex);
  }

  onRowClick(user: User): void {
    this.$router.push({ path: `users/${user.id}` });
  }
}
</script>

<style scoped lang="scss"></style>
