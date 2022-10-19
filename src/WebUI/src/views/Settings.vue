<template>
  <div class="container">
    <div class="section" v-if="restrictionsData.length">
      <h2 class="title">Restrictions</h2>
      <b-table :data="restrictionsData" :columns="restrictionsColumns"></b-table>
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
        <UserConfirmDeleteForm
          :userName="user.name"
          @close="props.close"
          @submit="onDeleteAccount"
        />
      </template>
    </b-modal>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import userModule from '@/store/user-module';
import { signOut } from '@/services/auth-service';
import User from '@/models/user';
import { timestampToTimeString } from '@/utils/date';
import UserConfirmDeleteForm from '@/components/UserConfirmDeleteForm.vue';

@Component({
  components: { UserConfirmDeleteForm },
})
export default class Settings extends Vue {
  isDeleteAcountDialogActive = false;

  created(): void {
    userModule.getUserRestrictions();
  }

  get user(): User | null {
    return userModule.user;
  }

  get restrictionsData() {
    return userModule.userRestrictions.map(b => ({
      ...b,
      createdAt: b.createdAt.toDateString(),
      duration: timestampToTimeString(b.duration),
      restrictedBy: `${b.restrictedByUser.name} (${b.restrictedByUser.platformUserId})`,
    }));
  }

  get restrictionsColumns() {
    return [
      {
        field: 'id',
        label: 'ID',
        numeric: true,
      },
      {
        field: 'createdAt',
        label: 'Created At',
      },
      {
        field: 'duration',
        label: 'Duration',
      },
      {
        field: 'type',
        label: 'Type',
      },
      {
        field: 'reason',
        label: 'Reason',
      },
      {
        field: 'restrictedBy',
        label: 'By',
      },
    ];
  }

  onDeleteAccount(): void {
    this.isDeleteAcountDialogActive = false;
    userModule.deleteUser();
    signOut();
  }
}
</script>

<style scoped lang="scss"></style>
