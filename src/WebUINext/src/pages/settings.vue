<script setup lang="ts">
import { useUserStore } from '@/stores/user';
import { deleteUser } from '@/services/users-service';
import { logout } from '@/services/auth-service';
import { t } from '@/services/translate-service';
import { notify } from '@/services/notification-service';

definePage({
  meta: {
    layout: 'default',
    roles: ['User', 'Moderator', 'Admin'],
  },
});

const userStore = useUserStore();

const onDeleteUser = async () => {
  await deleteUser();
  notify(t('user.settings.delete.notify.success'));
  logout();
};
</script>

<template>
  <div class="container">
    <div class="mx-auto max-w-2xl py-12">
      <h1 class="mb-14 text-center text-xl text-content-100">{{ $t('user.settings.title') }}</h1>

      <i18n-t scope="global" keypath="user.settings.delete.title" tag="div" class="text-center">
        <template #link>
          <Modal>
            <span class="cursor-pointer text-status-danger">
              {{ $t('user.settings.delete.link') }}
            </span>

            <template #popper="{ hide }">
              <ConfirmActionForm
                :title="$t('user.settings.delete.dialog.title')"
                :description="$t('user.settings.delete.dialog.desc')"
                :name="userStore.user!.name"
                :confirmLabel="$t('action.delete')"
                @cancel="hide"
                @confirm="
                  () => {
                    onDeleteUser();
                    hide();
                  }
                "
              />
            </template>
          </Modal>
        </template>
      </i18n-t>
    </div>
  </div>
</template>
