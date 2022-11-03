<template>
  <div class="card mb-6">
    <div class="card-content">
      <b-tabs v-model="activeSearchMode" type="is-toggle" :animated="false" @input="clearUser">
        <b-tab-item label="By Name" :value="searchModes.Name">
          <form @submit.prevent="findUser">
            <b-field label="Nickname" grouped>
              <b-input
                placeholder="User nickname"
                required
                :use-html5-validation="false"
                v-model="searchByNameModel.name"
                @input="clearUser"
              />
              <div class="control">
                <b-button native-type="submit" type="is-primary">Find</b-button>
              </div>
            </b-field>
          </form>
        </b-tab-item>

        <b-tab-item label="By Platform" :value="searchModes.Platfrom">
          <form @submit.prevent="findUser">
            <b-field grouped>
              <b-field label="Platform">
                <b-select
                  v-model="searchByPlatformModel.platform"
                  placeholder="Select a Platform"
                  required
                  @input="clearUser"
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
                  @input="clearUser"
                  v-model="searchByPlatformModel.platformUserId"
                />

                <p class="control">
                  <b-button native-type="submit" type="is-primary">Find</b-button>
                </p>
              </b-field>
            </b-field>
          </form>
        </b-tab-item>
      </b-tabs>

      <template v-if="user">
        <h4 class="title is-4">Matched user</h4>
        <div class="mt-4">
          <div class="media">
            <div class="media-left">
              <figure class="image is-64x64">
                <img :src="user.avatar" :alt="user.name" />
              </figure>
            </div>
            <div class="media-content">
              <p class="title is-4">
                <router-link
                  class="is-flex is-align-items-center"
                  :to="{
                    name: 'admin-user-restrictions',
                    params: { id: user.id },
                  }"
                >
                  {{ user.name }}
                  <b-icon icon="external-link-alt" class="is-size-6" />
                </router-link>
              </p>
              <p class="subtitle is-6">
                Id: {{ user.id }}, {{ user.platform }}: {{ user.platformUserId }}
                <platform :platform="user.platform" :platformUserId="user.platformUserId" />
              </p>
            </div>
          </div>
        </div>
      </template>
    </div>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import Platform from '@/models/platform';
import UserPublic from '@/models/user-public';
import * as userService from '@/services/users-service';
import PlatformComponent from '@/components/Platform.vue';

enum SearchMode {
  Name = 'Name',
  Platfrom = 'Platfrom',
}

@Component({
  components: { Platform: PlatformComponent },
})
export default class SearchUserComponent extends Vue {
  searchModes: Record<SearchMode, SearchMode> = {
    [SearchMode.Name]: SearchMode.Name,
    [SearchMode.Platfrom]: SearchMode.Platfrom,
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

  user: UserPublic | null = null;

  async findUser() {
    if (this.activeSearchMode === SearchMode.Name) {
      this.user = await userService.getUserByName(this.searchByNameModel.name);
      return;
    }

    this.user = await userService.getUserByPlatformUserId(
      this.searchByPlatformModel.platform,
      this.searchByPlatformModel.platformUserId
    );
  }

  clearUser() {
    this.user = null;
  }
}
</script>

<style scoped lang="scss"></style>
