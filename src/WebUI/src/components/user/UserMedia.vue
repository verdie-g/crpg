<script setup lang="ts">
import { type Clan, ClanMemberRole } from '@/models/clan';
import { type UserPublic } from '@/models/user';

const {
  user,
  clan = null,
  clanRole = null,
  isSelf = false,
  hiddenPlatform = false,
  size = 'sm',
} = defineProps<{
  user: UserPublic;
  clan?: Clan | null;
  clanRole?: ClanMemberRole | null;
  isSelf?: boolean;
  hiddenPlatform?: boolean;
  size?: 'sm' | 'xl';
}>();
</script>

<template>
  <div class="flex items-center gap-2">
    <img
      :src="user.avatar"
      alt=""
      class="rounded-full"
      :class="size === 'xl' ? 'h-10 w-10' : 'h-7 w-7'"
    />

    <div class="flex items-center gap-1">
      <template v-if="clan">
        <RouterLink
          class="group flex items-center gap-1 hover:opacity-75"
          :to="{ name: 'ClansId', params: { id: clan.id } }"
        >
          <ClanRoleIcon
            v-if="
              clanRole !== null &&
              [ClanMemberRole.Leader, ClanMemberRole.Officer].includes(clanRole)
            "
            :role="clanRole"
          />
          <ClanTagIcon :color="clan.primaryColor" />
          [{{ clan.tag }}]
        </RouterLink>
      </template>

      <span>
        {{ user.name }}
        <template v-if="isSelf">({{ $t('you') }})</template>
      </span>
    </div>

    <UserPlatform
      v-if="!hiddenPlatform"
      :platform="user.platform"
      :platformUserId="user.platformUserId"
      :userName="user.name"
      :size="size"
    />
  </div>
</template>
