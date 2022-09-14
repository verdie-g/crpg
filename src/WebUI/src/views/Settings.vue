<template>
  <div class="container">
    <div class="section" v-if="restrictionsData.length">
      <h2 class="title">{{ $t('settingsRestrictions') }}</h2>
      <b-table :data="restrictionsData" :columns="restrictionsColumns"></b-table>
    </div>

    <div class="section">
      <h2 class="title">{{ $t('settingsDeleteAccount') }}</h2>
      <div class="content">
        <p>{{ $t('settingsDeleteAccountDescription') }}</p>
        <b-button type="is-danger is-medium" @click="onDeleteAccountDialog">
          {{ $t('settingsDeleteAccountButton') }}
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
        label: this.$t('settingsRestrictionColumnID'),
        numeric: true,
      },
      {
        field: 'createdAt',
        label: this.$t('settingsRestrictionColumnRestrictedUser'),
      },
      {
        field: 'duration',
        label: this.$t('settingsRestrictionColumnDuration'),
      },
      {
        field: 'type',
        label: this.$t('settingsRestrictionColumnType'),
      },
      {
        field: 'reason',
        label: this.$t('settingsRestrictionColumnReason'),
      },
      {
        field: 'restrictedBy',
        label: this.$t('settingsRestrictionColumnRestrictedByUser'),
      },
    ];
  }

  onDeleteAccountDialog(): void {
    this.$buefy.dialog.confirm({
      title: this.$t('settingsDeleteDialogTitle').toString(),
      message: this.$t('settingsDeleteDialogMessage').toString(),
      confirmText: this.$t('settingsDeleteDialogConfirmText').toString(),
      cancelText: this.$t('settingsDeleteDialogCancelText').toString(),
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
