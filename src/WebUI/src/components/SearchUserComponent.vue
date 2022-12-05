<template>
  <div class="card mb-6">
    <div class="card-content">
      <b-tabs v-model="activeSearchMode" type="is-toggle" :animated="false" @input="clearUsers">
        <b-tab-item label="By Name" :value="searchModes.Name">
          <form @submit.prevent="searchUser">
            <b-field label="Nickname" grouped>
              <b-input
                placeholder="User nickname"
                required
                :use-html5-validation="false"
                v-model="searchByNameModel.name"
                @input="clearUsers"
              />
              <div class="control">
                <b-button native-type="submit" type="is-primary">Find</b-button>
              </div>
            </b-field>
          </form>
        </b-tab-item>

        <b-tab-item label="By Platform" :value="searchModes.Platform">
          <form @submit.prevent="searchUser">
            <b-field grouped>
              <b-field label="Platform">
                <b-select
                  v-model="searchByPlatformModel.platform"
                  placeholder="Select a Platform"
                  required
                  @input="clearUsers"
                >
                  <option v-for="platform in availablePlatforms" :key="platform" :value="platform">
                    {{ platform }}
                  </option>
                </b-select>
              </b-field>

              <b-field label="User platform ID" grouped>
                <b-input
                  placeholder="Platform-specific User ID"
                  required
                  :use-html5-validation="false"
                  @input="clearUsers"
                  v-model="searchByPlatformModel.platformUserId"
                />

                <p class="control">
                  <b-button native-type="submit" type="is-primary">Find</b-button>
                </p>
              </b-field>
            </b-field>
          </form>
        </b-tab-item>

        <b-tab-item label="By Id" :value="searchModes.Id">
          <form @submit.prevent="searchUser">
            <b-field label="Id" grouped>
              <b-input placeholder="id" required v-model="searchByIdModel.id" @input="clearUsers" />
              <div class="control">
                <b-button native-type="submit" type="is-primary">Find</b-button>
              </div>
            </b-field>
          </form>
        </b-tab-item>
      </b-tabs>

      <template v-if="users.length">
        <h4 class="title is-4">Matched user</h4>

        <UserCard v-for="user in users" class="mt-4" :key="user.id" :user="user" useLink />
      </template>
    </div>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import Platform from '@/models/platform';
import UserPublic from '@/models/user-public';
import * as userService from '@/services/users-service';
import UserCardComponent from '@/components/UserCard.vue';

enum SearchMode {
  Name = 'Name',
  Platform = 'Platform',
  Id = 'Id',
}

@Component({
  components: { UserCard: UserCardComponent },
})
export default class SearchUserComponent extends Vue {
  searchModes: Record<SearchMode, SearchMode> = {
    [SearchMode.Name]: SearchMode.Name,
    [SearchMode.Platform]: SearchMode.Platform,
    [SearchMode.Id]: SearchMode.Id,
  };
  activeSearchMode: SearchMode = SearchMode.Name;

  searchByNameModel: {
    name: string;
  } = {
    name: '',
  };

  availablePlatforms = Object.keys(Platform);
  searchByPlatformModel: {
    platform: Platform;
    platformUserId: string;
  } = {
    platform: Platform.Steam,
    platformUserId: '',
  };

  searchByIdModel: {
    id: number;
  } = {
    id: 0,
  };

  users: UserPublic[] = [];

  async searchUser() {
    if (this.activeSearchMode == SearchMode.Id) {
      this.users = [await userService.getUserById(this.searchByIdModel.id)];
      return;
    }

    const payload =
      this.activeSearchMode === SearchMode.Name
        ? {
            name: this.searchByNameModel.name,
          }
        : {
            platform: this.searchByPlatformModel.platform,
            platformUserId: this.searchByPlatformModel.platformUserId,
          };

    this.users = await userService.searchUser(payload);
  }

  clearUsers() {
    this.users = [];
  }
}
</script>

<style scoped lang="scss"></style>
