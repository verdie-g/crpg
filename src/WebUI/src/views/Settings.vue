<template>
  <div class="container">
    <div class="section">
      <h2 class="title">Restrictions</h2>
      <RestrictionsTable
        :data="restrictions"
        :hiddenCols="['id', 'restrictedUser', 'restrictedByUser']"
      />
      <div v-if="activeJoinRestriction" class="mt-4 content is-medium box">
        <p>If you'd like to discuss your punishment further, please follow the below steps:</p>
        <ol>
          <li>
            Join our
            <a href="https://discord.gg/c-rpg" target="_blank">Discord</a>
          </li>
          <li>
            Navigate to our
            <a
              href="https://discord.com/channels/279063743839862805/1034895358435799070"
              target="_blank"
            >
              Modmail
            </a>
            channel
          </li>
          <li>Follow the instructions in that channel to reach out to us</li>
        </ol>
        <p>
          * Please note that at this time, we do not accept appeals through any platform other than
          Discord.
        </p>
      </div>
    </div>

    <div class="section">
      <h2 class="title">Delete account</h2>
      <div class="content">
        <p>Make your character, items, gold and all your progression disappear.</p>
        <b-button type="is-danger is-medium" @click="isDeleteAcountDialogActive = true">
          Delete your account
        </b-button>
      </div>
    </div>

    <b-modal
      v-if="user"
      v-model="isDeleteAcountDialogActive"
      has-modal-card
      trap-focus
      aria-role="dialog"
      aria-modal
    >
      <template #default="props">
        <ConfirmActionForm :confirmValue="user.name" @close="props.close" @submit="onDeleteAccount">
          <template #title>Deleting account</template>
          <template #description>
            <p>
              Are you sure you want to delete your account?
              <br />
              This action cannot be undone.
            </p>
          </template>
        </ConfirmActionForm>
      </template>
    </b-modal>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import userModule from '@/store/user-module';
import User from '@/models/user';
import { RestrictionWithActive } from '@/models/restriction';
import * as userService from '@/services/users-service';
import { signOut } from '@/services/auth-service';
import RestrictionsTable from '@/components/RestrictionsTable.vue';
import ConfirmActionForm from '@/components/ConfirmActionForm.vue';
import { getActiveJoinRestriction } from '@/services/restriction-service';

@Component({
  components: { ConfirmActionForm, RestrictionsTable },
})
export default class Settings extends Vue {
  isDeleteAcountDialogActive = false;
  restrictions: RestrictionWithActive[] = [];

  get user(): User | null {
    return userModule.user;
  }

  async created() {
    this.restrictions = await userService.getUserRestrictions(this.user!.id);
  }

  get activeJoinRestriction() {
    return getActiveJoinRestriction(this.restrictions);
  }

  onDeleteAccount(): void {
    this.isDeleteAcountDialogActive = false;
    userModule.deleteUser();
    signOut();
  }
}
</script>

<style scoped lang="scss"></style>
