<template>
  <platform-modal
    v-model="isModalActive"
    title="Restrict User"
    icon="ban"
  >
    <div class="columns">
      <div class="column is-flex is-flex-direction-column">
        <div class="is-flex-grow-1 is-flex is-flex-direction-column is-align-items-stretch is-justify-content-center">
          <b-field horizontal label="Platform">
            <b-select
              placeholder="Select a Platform"
              required
              expanded
              v-model="selectedPlatform"
            >
              <option
                v-for="platform in availablePlatforms"
                :key="platform"
                :value="platform"
              >
                {{ platform }}
              </option>
            </b-select>
          </b-field>

          <b-field horizontal label="User ID">
            <b-input
              placeholder="Platform-specific User ID"
              required
              v-model="platformUserId"
            />
          </b-field>
        </div>

        <div class="is-paddingless is-flex-grow-0 has-text-centered">
          <b-button
            icon-left="search"
            type="is-primary"
            :loading="pendingLookupUser"
            @click.native="lookupUser"
          >
            Lookup
          </b-button>
        </div>
      </div>
      <div class="column">
        <div v-show="matchedUser" class="is-flex is-flex-direction-column is-align-items-center">
          <div class="has-text-centered pb-4">
            <h3 class="is-size-4">
              Matched User:
            </h3>
          </div>
          <div class="has-text-centered">
            <b-image
              class="matched-user--avatar box is-paddingless is-flex is-align-items-center"
              alt="Avatar"
              responsive
              :src="matchedUser ? matchedUser.avatar : undefined"
            />
          </div>
          <div class="matched-user--name has-text-centered is-size-5 mt-3">
            <strong v-if="matchedUser">
              {{ matchedUser.name }}
            </strong>
          </div>
          <div class="pt-2 is-align-self-stretch">
            <div class="is-flex">
              <b-field label="Reason" class="is-flex-grow-1 mr-2">
                <b-input
                  placeholder="Toxicity, griefing, etc."
                  required
                  v-model="newRestrictionReason"
                />
              </b-field>
              <b-field label="Type">
                <b-select
                  placeholder="Type"
                  required
                  v-model="newRestrictionType"
                >
                  <option
                    v-for="platform in availableRestrictionTypes"
                    :key="platform"
                    :value="platform"
                  >
                    {{ platform }}
                  </option>
                </b-select>
              </b-field>
            </div>
            <b-field class="is-flex is-justify-content-center">
              <b-button
                icon-left="ban"
                type="is-danger"
                :disabled="!matchedUser || pendingCreateRestriction"
                :loading="pendingCreateRestriction"
                @click.native="banTargetUser"
              >
                Restrict
              </b-button>
            </b-field>
          </div>
        </div>
      </div>
    </div>
  </platform-modal>
</template>

<script lang="ts">
import PlatformModal from '@/components/shared/PlatformModal.vue'
import Platform from '@/models/platform'
import RestrictionType from '@/models/restriction-type'
import User from '@/models/user'
import { NotificationType, notify } from '@/services/notifications-service'
import restrictionModule from '@/store/restriction-module'
import userModule from '@/store/user-module'
import { Component, Prop, Vue } from 'vue-property-decorator'

@Component({
  components: {
    PlatformModal
  },
})
export default class CreateRestrictionModal extends Vue {
  @Prop(Boolean) readonly value: boolean;
  @Prop(String) readonly title: string;

  availablePlatforms = Object.keys(Platform);
  availableRestrictionTypes = Object.keys(RestrictionType);
  selectedPlatform: Platform | null = Platform.Steam;
  platformUserId: string | null = null;
  newRestrictionType: RestrictionType | null = RestrictionType.Join;
  newRestrictionReason: string | null = null;
  matchedUser: User | null = null;
  pendingLookupUser = false;
  pendingCreateRestriction = false;

  get isModalActive(): boolean {
    return this.value;
  }
  set isModalActive(val: boolean) {
    this.$emit('input', val);
  }

  async lookupUser(): Promise<void> {
    const platform = this.selectedPlatform;
    const platformUserId = this.platformUserId;
    if (!platform || !platformUserId) {
      return;
    }
    const payload = {
      platform,
      platformUserId
    }
    this.pendingCreateRestriction = false;
    this.pendingLookupUser = true;
    try {
      this.matchedUser = await userModule.getUserByPlatform(payload);
    } catch (err) {
      console.error('Failed to lookup User by Platform and PlatformUserId.', payload);
      throw err;
    } finally {
      this.pendingLookupUser = false;
    }
  }

  async banTargetUser(): Promise<void> {
    const restrictedUserId = this.matchedUser?.id;
    const reason = this.newRestrictionReason;
    const type = this.newRestrictionType;
    if (!restrictedUserId || !reason || !type) {
      return;
    }
    const payload = {
      restrictedUserId,
      reason,
      type,
    }
    this.pendingCreateRestriction = true;
    try {
      await restrictionModule.createRestriction(payload);
    } catch (err) {
      console.error('Failed to create Restriction for User', payload);
      notify('Failed to restrict user.', NotificationType.Error);
      throw err;
    } finally {
      this.pendingCreateRestriction = true;
    }
    notify('User restricted.', NotificationType.Info);
    this.isModalActive = false;
    this.$emit('created');
  }
}
</script>

<style scoped lang="scss">
.matched-user--name {
  min-height: 1em;
}
.matched-user--avatar {
  width: 96px;
  height: 96px;
}
</style>
