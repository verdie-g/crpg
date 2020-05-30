<template>
  <div class="container">
    <div class="section">
      <h2 class="title">Delete account</h2>
      <div class="content">
        <p>Make your character, items, gold and all your progression disappear.</p>
        <b-button type="is-danger is-medium" @click="onDeleteAccountDialog">Delete your account</b-button>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import userModule from '@/store/user-module';
import { notify } from '@/services/notifications-service';
import { clearToken } from '@/services/auth-service';

@Component
export default class Settings extends Vue {
  onDeleteAccountDialog() {
    this.$buefy.dialog.confirm({
      title: 'Deleting account',
      message: 'Are you sure you want to delete your account? This action cannot be undone.',
      confirmText: 'Delete account',
      type: 'is-danger',
      hasIcon: true,
      onConfirm: () => {
        userModule.deleteUser();
        clearToken();
        userModule.signOut();
        this.$router.replace('/');
      },
    });
  }
}
</script>

<style scoped lang="scss">
</style>
