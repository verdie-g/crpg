<template>
  <div class="container">
    <div class="section">
      <h2 class="title">Restrictions</h2>
      <RestrictionsTable
        :data="restrictions"
        :hiddenCols="['id', 'restrictedUser', 'restrictedByUser']"
      />
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
import * as userService from '@/services/users-service';
import { signOut } from '@/services/auth-service';
import RestrictionsTable from '@/components/RestrictionsTable';
import Restriction from '@/models/restriction';
import ConfirmActionForm from '@/components/ConfirmActionForm.vue';

@Component({
  components: { ConfirmActionForm, RestrictionsTable },
})
export default class Settings extends Vue {
  isDeleteAcountDialogActive = false;
  restrictions: Restriction[] = [];

  get user(): User | null {
    return userModule.user;
  }

  async created() {
    this.restrictions = await userService.getRestrictions();
  }

  onDeleteAccount(): void {
    this.isDeleteAcountDialogActive = false;
    userModule.deleteUser();
    signOut();
  }
}
</script>

<style scoped lang="scss"></style>
