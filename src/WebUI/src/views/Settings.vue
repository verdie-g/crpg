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
        <b-button type="is-danger is-medium" @click="onDeleteAccountDialog">
          Delete your account
        </b-button>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import userModule from '@/store/user-module';
import { signOut } from '@/services/auth-service';
import { timestampToTimeString } from '@/utils/date';

@Component
export default class Settings extends Vue {
  created(): void {
    userModule.getUserRestrictions();
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

  onDeleteAccountDialog(): void {
    this.$buefy.dialog.confirm({
      title: 'Deleting account',
      message: 'Are you sure you want to delete your account? This action cannot be undone.',
      confirmText: 'Delete account',
      type: 'is-danger',
      hasIcon: true,
      onConfirm: () => {
        userModule.deleteUser();
        signOut();
      },
    });
  }
}
</script>

<style scoped lang="scss"></style>
