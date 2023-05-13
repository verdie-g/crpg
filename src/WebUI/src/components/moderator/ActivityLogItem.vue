<script setup lang="ts">
import { type ActivityLog, ActivityLogType } from '@/models/activity-logs';
import { type UserPublic } from '@/models/user';
import { getItemImage } from '@/services/item-service';

const props = withDefaults(
  defineProps<{
    activityLog: ActivityLog;
    user: UserPublic;
    users: Record<number, UserPublic>;
    isSelfUser: boolean;
  }>(),
  {
    users: () => ({}),
  }
);

const emit = defineEmits<{
  (e: 'addUser', id: number): void;
  (e: 'addType', type: ActivityLogType): void;
}>();
</script>

<template>
  <div
    class="flex-0 inline-flex w-auto flex-col space-y-2 rounded-lg bg-base-200 p-4"
    :class="[isSelfUser ? 'self-start' : 'self-end']"
  >
    <div class="flex items-center gap-2">
      <RouterLink
        :to="{ name: 'ModeratorUserIdRestrictions', params: { id: user.id } }"
        class="inline-block hover:text-content-100"
      >
        <UserMedia :user="user" />
      </RouterLink>

      <div class="text-2xs text-content-300">
        {{ $d(activityLog.createdAt, 'long') }}
      </div>

      <Tag
        variant="primary"
        :label="activityLog.type"
        data-aq-addLogItem-type
        @click="emit('addType', activityLog.type)"
      />
    </div>

    <i18n-t :keypath="`activityLog.tpl.${activityLog.type}`" tag="div" scope="global">
      <template #price v-if="'price' in activityLog.metadata">
        <Coin :value="Number(activityLog.metadata.price)" data-aq-addLogItem-tpl-goldPrice />
      </template>

      <template #gold v-if="'gold' in activityLog.metadata">
        <Coin :value="Number(activityLog.metadata.gold)" data-aq-addLogItem-tpl-goldPrice />
      </template>

      <template #heirloomPoints v-if="'heirloomPoints' in activityLog.metadata">
        <span
          class="inline-flex gap-1.5 align-text-bottom font-bold text-primary"
          data-aq-addLogItem-tpl-heirloomPoints
        >
          <OIcon icon="blacksmith" size="lg" />
          {{ $n(Number(activityLog.metadata.heirloomPoints)) }}
        </span>
      </template>

      <template #itemId v-if="'itemId' in activityLog.metadata">
        <span class="inline" data-aq-addLogItem-tpl-itemId>
          <VTooltip placement="auto" class="inline-block">
            <span class="font-bold text-content-100">{{ activityLog.metadata.itemId }}</span>
            <template #popper>
              <img
                :src="getItemImage(activityLog.metadata.itemId)"
                class="h-full w-full object-contain"
              />
            </template>
          </VTooltip>
        </span>
      </template>

      <template #experience v-if="'experience' in activityLog.metadata">
        <span class="font-bold text-content-100" data-aq-addLogItem-tpl-experience>
          {{ $n(Number(activityLog.metadata.experience)) }}
        </span>
      </template>

      <template #damage v-if="'damage' in activityLog.metadata">
        <span class="font-bold text-status-danger" data-aq-addLogItem-tpl-damage>
          {{ $n(Number(activityLog.metadata.damage)) }}
        </span>
      </template>

      <template #targetUserId v-if="Number(activityLog.metadata.targetUserId) in users">
        <div
          class="inline-flex items-center gap-1 align-middle"
          data-aq-addLogItem-tpl-targetUserId
        >
          <RouterLink
            :to="{
              name: 'ModeratorUserIdRestrictions',
              params: { id: activityLog.metadata.targetUserId },
            }"
            class="inline-block hover:text-content-100"
          >
            <UserMedia :user="users[Number(activityLog.metadata.targetUserId)]" />
          </RouterLink>
          <OButton
            v-if="isSelfUser"
            size="2xs"
            iconLeft="add"
            rounded
            variant="secondary"
            data-aq-addLogItem-addUser-btn
            @click="emit('addUser', Number(activityLog.metadata.targetUserId))"
          />
        </div>
      </template>

      <template #instance v-if="'instance' in activityLog.metadata">
        <Tag variant="info" :label="activityLog.metadata.instance" />
      </template>

      <template #oldName v-if="'oldName' in activityLog.metadata">
        <span class="font-bold text-content-100">{{ activityLog.metadata.oldName }}</span>
      </template>

      <template #newName v-if="'newName' in activityLog.metadata">
        <span class="font-bold text-content-100">{{ activityLog.metadata.newName }}</span>
      </template>

      <template #characterId v-if="'characterId' in activityLog.metadata">
        <span class="font-bold text-content-100">{{ activityLog.metadata.characterId }}</span>
      </template>

      <template #generation v-if="'generation' in activityLog.metadata">
        <span class="font-bold text-content-100">{{ activityLog.metadata.generation }}</span>
      </template>

      <template #level v-if="'level' in activityLog.metadata">
        <span class="font-bold text-content-100">{{ activityLog.metadata.level }}</span>
      </template>

      <template #message v-if="'message' in activityLog.metadata">
        <span class="font-bold text-content-100">{{ activityLog.metadata.message }}</span>
      </template>
    </i18n-t>
  </div>
</template>
