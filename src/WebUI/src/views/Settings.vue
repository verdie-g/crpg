<template>
  <div class="container">
    <div class="section" v-if="bansData.length">
      <h2 class="title">Bans</h2>
      <b-table :data="bansData" :columns="bansColumns"></b-table>
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
    userModule.getUserBans();
  }

  get bansData() {
    return userModule.userBans.map(b => ({
      ...b,
      createdAt: b.createdAt.toDateString(),
      duration: timestampToTimeString(b.duration),
      bannedBy: `${b.bannedByUser.name} (${b.bannedByUser.platformUserId})`,
    }));
  }

  get bansColumns() {
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
        field: 'reason',
        label: 'Reason',
      },
      {
        field: 'bannedBy',
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

<style scoped lang="scss">
</style>
