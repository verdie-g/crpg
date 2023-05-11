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
        @click="emit('addType', activityLog.type)"
        data-aq-addLogItem-type
      />
    </div>

    <i18n-t
      :keypath="`activityLog.tpl.${activityLog.type}`"
      tag="div"
      scope="global"
      class="flex items-center gap-1.5"
    >
      <template v-for="(value, key) in activityLog.metadata">
        <template v-if="['gold', 'price'].includes(key)">
          <Coin :value="Number(value)" data-aq-addLogItem-tpl-goldPrice />
        </template>

        <template v-else-if="key === 'heirloomPoints'">
          <span
            class="inline-flex gap-1.5 align-text-bottom font-bold text-primary"
            data-aq-addLogItem-tpl-heirloomPoints
          >
            <OIcon icon="blacksmith" size="lg" />
            {{ $n(Number(value)) }}
          </span>
        </template>

        <template v-else-if="key === 'itemId'">
          <span class="inline" data-aq-addLogItem-tpl-itemId>
            <VTooltip placement="auto" class="inline-block">
              <span class="font-bold text-content-100">{{ value }}</span>
              <template #popper>
                <img :src="getItemImage(value)" class="h-full w-full object-contain" />
              </template>
            </VTooltip>
          </span>
        </template>

        <template v-else-if="key === 'experience'">
          <span class="font-bold text-content-100" data-aq-addLogItem-tpl-experience>
            {{ $n(Number(value)) }}
          </span>
        </template>

        <template v-else-if="key === 'damage'">
          <span class="font-bold text-status-danger" data-aq-addLogItem-tpl-damage>
            {{ $n(Number(value)) }}
          </span>
        </template>

        <template v-else-if="key === 'targetUserId' && Number(value) in users">
          <div class="inline-flex items-center gap-1" data-aq-addLogItem-tpl-targetUserId>
            <RouterLink
              :to="{ name: 'ModeratorUserIdRestrictions', params: { id: value } }"
              class="inline-block hover:text-content-100"
            >
              <UserMedia :user="users[Number(value)]" />
            </RouterLink>
            <OButton
              v-if="isSelfUser"
              size="2xs"
              iconLeft="add"
              rounded
              variant="secondary"
              data-aq-addLogItem-addUser-btn
              @click="emit('addUser', Number(value))"
            />
          </div>
        </template>

        <span v-else class="font-bold text-content-100">{{ value }}</span>
      </template>
    </i18n-t>
  </div>
</template>
