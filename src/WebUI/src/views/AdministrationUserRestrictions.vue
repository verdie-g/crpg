<template>
  <div class="container">
    <div class="section">
      <div class="mb-5">
        <b-button
          label="Back"
          tag="router-link"
          :to="{ name: 'admin' }"
          type="is-link is-light "
          icon-left="arrow-left"
        />
      </div>

      <h1 class="title">User Restrictions</h1>

      <template v-if="user">
        <div class="card mb-6">
          <div class="card-content">
            <UserCard :user="user" />

            <form @submit.prevent="addRestriction">
              <h3 class="title is-4">New Restriction</h3>

              <b-field grouped>
                <b-field label="Type">
                  <b-select required v-model="newRestrictionModel.type">
                    <option
                      v-for="restrictionType in availableRestrictionTypes"
                      :key="restrictionType"
                      :value="restrictionType"
                    >
                      {{ restrictionType }}
                    </option>
                  </b-select>
                </b-field>

                <b-field grouped message="Use a duration of 0 to un-restrict">
                  <b-field label="Days">
                    <b-input
                      style="width: 80px"
                      min="0"
                      placeholder="Duration"
                      type="number"
                      v-model="duration.days"
                    />
                  </b-field>

                  <b-field label="Hours">
                    <b-input
                      style="width: 80px"
                      min="0"
                      placeholder="Duration"
                      type="number"
                      v-model="duration.hours"
                    />
                  </b-field>

                  <b-field label="Minutes">
                    <b-input
                      style="width: 80px"
                      min="0"
                      placeholder="Duration"
                      type="number"
                      v-model="duration.minutes"
                    />
                  </b-field>
                </b-field>
              </b-field>

              <b-field label="Reason" style="width: 50%">
                <b-input
                  type="textarea"
                  placeholder="Toxicity, griefing, etc."
                  required
                  v-model="newRestrictionModel.reason"
                />
              </b-field>

              <b-button label="Add Restriction" native-type="submit" type="is-primary" />
            </form>
          </div>
        </div>
      </template>

      <h2 class="title">Restrictions history</h2>
      <RestrictionsTable :data="restrictions" :hiddenCols="['restrictedUser']" />
    </div>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import RestrictionType from '@/models/restriction-type';
import UserPublic from '@/models/user-public';
import Restriction from '@/models/restriction';
import RestrictionCreation from '@/models/restriction-creation';
import * as userService from '@/services/users-service';
import * as restrictionService from '@/services/restriction-service';
import { NotificationType, notify } from '@/services/notifications-service';
import RestrictionsTable from '@/components/RestrictionsTable.vue';
import PlatformComponent from '@/components/Platform.vue';
import { convertHumanDurationToMs } from '@/utils/date';
import type { HumanDuration } from '@/utils/date';
import UserCardComponent from '@/components/UserCard.vue';

@Component({
  components: { RestrictionsTable, Platform: PlatformComponent, UserCard: UserCardComponent },
})
export default class Administration extends Vue {
  availableRestrictionTypes = Object.keys(RestrictionType);
  user: UserPublic | null = null;
  restrictions: Restriction[] = [];
  newRestrictionModel: Omit<RestrictionCreation, 'restrictedUserId'> = {
    reason: '',
    duration: 0,
    type: RestrictionType.Join,
  };
  duration: HumanDuration = {
    days: 0,
    hours: 0,
    minutes: 0,
  };

  get durationSeconds() {
    return convertHumanDurationToMs(this.duration);
  }

  async created() {
    await this.getUser();
    await this.getRestrictions();
  }

  async getUser() {
    this.user = await userService.getUserByUserId(parseInt(this.$route.params.id, 10));
  }

  async getRestrictions() {
    this.restrictions = await userService.getUserRestrictions(this.user!.id);
  }

  async addRestriction() {
    await restrictionService.restrictUser({
      restrictedUserId: this.user!.id,
      ...this.newRestrictionModel,
      duration: this.durationSeconds,
    });

    notify('User restricted.', NotificationType.Info);

    await this.getRestrictions();

    this.duration = {
      days: 0,
      hours: 0,
      minutes: 0,
    };

    this.newRestrictionModel = {
      reason: '',
      duration: 0,
      type: RestrictionType.Join,
    };
  }
}
</script>

<style scoped lang="scss"></style>
