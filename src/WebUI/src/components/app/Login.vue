<script setup lang="ts">
import { useUserStore } from '@/stores/user';
import { login } from '@/services/auth-service';
import { platformToIcon } from '@/services/platform-service';
import { Platform } from '@/models/platform';
import { usePlatform } from '@/composables/use-platform';
import { useAsyncCallback } from '@/utils/useAsyncCallback';

const { user } = toRefs(useUserStore());
const { platform, changePlatform } = usePlatform();
const { execute: loginUser, loading: logging } = useAsyncCallback(() => login(platform.value));
</script>

<template>
  <OField v-if="user === null">
    <OButton
      variant="primary"
      size="xl"
      :iconLeft="platformToIcon[platform]"
      :label="$t(`platform.${platform}`)"
      :loading="logging"
      data-aq-login-btn
      @click="loginUser"
    >
      <div class="flex flex-col text-left leading-tight">
        <span class="text-[10px]">{{ $t('login.label') }}</span>
        <span>{{ $t(`platform.${platform}`) }}</span>
      </div>
    </OButton>

    <VDropdown :triggers="['click']" placement="bottom-end">
      <template #default="{ shown }">
        <OButton variant="primary" :iconRight="shown ? 'chevron-up' : 'chevron-down'" size="xl" />
      </template>

      <template #popper="{ hide }">
        <DropdownItem
          v-for="p in Object.values(Platform)"
          :checked="p === platform"
          :label="$t(`platform.${p}`)"
          :icon="platformToIcon[p]"
          data-aq-platform-item
          @click="
            () => {
              changePlatform(p);
              hide();
            }
          "
        />
      </template>
    </VDropdown>
  </OField>

  <RouterLink v-else :to="{ name: 'Characters' }" data-aq-character-link>
    <OButton variant="primary" size="xl" iconLeft="member" :label="$t('action.enter')" />
  </RouterLink>
</template>
