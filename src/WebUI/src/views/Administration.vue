<template>
  <div class="container">
    <div class="section">
      <h2 class="title">Restrictions</h2>
      <b-table
        :data="restrictionsData"
        striped
        hoverable
        :columns="restrictionsColumns"
        paginated
        :per-page="2"
        default-sort="id"
        default-sort-direction="desc"
        sort-icon-size="is-small"
        :debounce-search="1000"
      >
        <template #empty>
          <div class="has-text-centered">No records</div>
        </template>
      </b-table>
    </div>

    <!-- FIXME: TODO:  -->
    <div>
      <div>
        <div>FINDER</div>
        <div>
          <!--  -->
          <form @submit.prevent="getUser">
            <b-field label="Platform">
              <b-select
                v-model="selectedPlatform"
                placeholder="Select a Platform"
                required
                expanded
              >
                <option v-for="platform in availablePlatforms" :key="platform" :value="platform">
                  {{ platform }}
                </option>
              </b-select>
            </b-field>

            <b-field label="User ID">
              <b-input placeholder="Platform-specific User ID" required v-model="platformUserId" />
            </b-field>

            <b-button native-type="submit">Find</b-button>
          </form>

          <!-- avatar:"https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/58/5838cfcd99e280d82f63d92472d6d5aecebfb812_medium.jpg"
          id:3
          name:"Baron Cyborg"
          platform:"Steam"
          platformUserId:"76561198026044780"
-->
          <template v-if="user">
            <div>
              <img :src="user.avatar" alt="" />
              <div>ID: {{ user.id }}</div>
              <div>Name: {{ user.name }}</div>
              <div>Platform: {{ user.platform }}</div>
              <div>platformUserId: {{ user.platformUserId }}</div>
            </div>
          </template>

          <template v-if="userRestrictions">
            <div>
              {{ userRestrictions }}
            </div>
          </template>
        </div>
      </div>

      <br />
      <br />
      <br />
      <br />
      <br />
      <br />
      <br />
      <br />
      <br />
      <br />

      <!--  -->
      <form>
        <b-field label="Platform">
          <b-select v-model="selectedPlatform" placeholder="Select a Platform" required expanded>
            <option v-for="platform in availablePlatforms" :key="platform" :value="platform">
              {{ platform }}
            </option>
          </b-select>
        </b-field>

        <b-field label="User ID">
          <b-input placeholder="Platform-specific User ID" required v-model="platformUserId" />
        </b-field>

        <b-field label="Reason" class="">
          <b-input placeholder="Toxicity, griefing, etc." required v-model="newRestrictionReason" />
        </b-field>

        <b-field label="Type">
          <b-select placeholder="Type" required v-model="newRestrictionType">
            <option
              v-for="restrictionType in availableRestrictionTypes"
              :key="restrictionType"
              :value="restrictionType"
            >
              {{ restrictionType }}
            </option>
          </b-select>
        </b-field>

        <b-field label="Duration (mins)" class="is-flex-grow-1 is-flex-shrink-2 mr-2">
          <b-input placeholder="Duration" required v-model="newRestrictionDuration" />
        </b-field>
      </form>
    </div>
  </div>
</template>

<script lang="ts">
/*
В идеале, чтобы ограничить пользователя, он должен запрашивать либо cRPG ID / cRPG name / Steam ID / GoG ID / ...., затем отображать его прошлые ограничения, а затем запрашивать продолжительность и тип ограничения.
I'll add an endpoint to search by username
*/

import { Component, Vue } from 'vue-property-decorator';
import restrictionModule from '@/store/restriction-module';
import { timestampToTimeString } from '@/utils/date';

import Platform from '@/models/platform';
import RestrictionType from '@/models/restriction-type';
import * as userService from '@/services/users-service';

@Component
export default class Administration extends Vue {
  selectedPlatform: Platform = Platform.Steam;
  availablePlatforms = Object.keys(Platform);
  availableRestrictionTypes = Object.keys(RestrictionType);
  platformUserId: string | null = '76561197987525637';
  newRestrictionReason: string | null = null;
  newRestrictionDuration = 0;
  newRestrictionType: RestrictionType | null = RestrictionType.Join;
  user: any = null;
  userRestrictions: any = null;

  //
  //
  //

  async getUser() {
    if (!this.platformUserId) return;
    //
    this.user = await userService.getUserByPlatformUserId(
      this.selectedPlatform,
      this.platformUserId
    );

    this.userRestrictions = await userService.getUserRestrictions(this.user.id);
  }

  created(): void {
    if (restrictionModule.restrictions.length === 0) {
      restrictionModule.getRestrictions();
    }
  }

  get restrictionsData() {
    return restrictionModule.restrictions.map(r => ({
      id: r.id,
      restrictedUser: `${r.restrictedUser!.name} (${r.restrictedUser!.platformUserId})`,
      createdAt: r.createdAt.toDateString(),
      duration: timestampToTimeString(r.duration),
      type: r.type,
      reason: r.reason,
      restrictedByUser: `${r.restrictedByUser.name} (${r.restrictedByUser.platformUserId})`,
    }));
  }

  get restrictionsColumns() {
    return [
      {
        field: 'id',
        label: 'ID',
        sortable: true,
        width: 60,
      },
      {
        field: 'restrictedUser',
        label: 'User',
        searchable: true,
        sortable: true,
        width: 300,
      },
      {
        field: 'createdAt',
        label: 'Created At',
        sortable: true,
        width: 180,
      },
      {
        field: 'duration',
        label: 'Duration',
        width: 180,
      },
      {
        field: 'type',
        label: 'Type',
        sortable: true,
        width: 80,
      },
      {
        field: 'reason',
        label: 'Reason',
        sortable: true,
        width: 100,
      },
      {
        field: 'restrictedByUser',
        label: 'By',
        searchable: true,
        sortable: true,
        width: 300,
      },
    ];
  }
}
</script>

<style scoped lang="scss"></style>
