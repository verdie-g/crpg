<script setup lang="ts">
import { ActivityLogType } from '@/models/activity-logs';
import { getActivityLogsWithUsers } from '@/services/activity-logs-service';
import { moderationUserKey } from '@/symbols/moderator';
import { Sort, useSort } from '@/composables/use-sort';

definePage({
  props: true,
  meta: {
    layout: 'default',
    roles: ['Moderator', 'Admin'],
  },
});

const props = defineProps<{ id: string }>();
const user = injectStrict(moderationUserKey);
const router = useRouter();
const route = useRoute();

const from = computed({
  set(val: Date) {
    router.push({
      query: {
        ...route.query,
        from: val.toISOString(),
      },
    });
  },

  get() {
    if (route.query?.from === undefined) {
      const fromDate = new Date();
      fromDate.setMinutes(fromDate.getMinutes() - 5); // Show logs for the last 5 minutes by default
      return fromDate;
    }

    return new Date(route.query.from as string);
  },
});

const to = computed({
  set(val: Date) {
    router.push({
      query: {
        ...route.query,
        to: String(val.toISOString()),
      },
    });
  },

  get() {
    return route.query?.to ? new Date(route.query.to as string) : new Date();
  },
});

const types = computed({
  set(val: ActivityLogType[]) {
    router.push({
      query: {
        ...route.query,
        types: val,
      },
    });
  },

  get() {
    return (route.query?.types as ActivityLogType[]) || [];
  },
});

const addType = (type: ActivityLogType) => {
  types.value = [...new Set([...((route.query?.types as ActivityLogType[]) || []), type])];
};

const additionalUsers = computed({
  async set(val: number[]) {
    await router.push({
      query: {
        ...route.query,
        additionalUsers: val,
      },
    });

    fetchActivityLogsWithUsers();
  },

  get() {
    return ((route.query?.additionalUsers as string[]) || []).map(Number);
  },
});

const addAdditionalUser = (id: number) => {
  additionalUsers.value = [...new Set([...additionalUsers.value, id])];
};

const removeAdditionalUser = (userId: number) => {
  additionalUsers.value = additionalUsers.value.filter(id => id !== userId);
};

const { sort, toggleSort } = useSort('createdAt');

const {
  state: activityLogsWithUsers,
  execute: fetchActivityLogsWithUsers,
  isLoading: isLoadingActivityLogsWithUsers,
} = useAsyncState(
  () =>
    getActivityLogsWithUsers({
      from: from.value,
      to: to.value,
      type: types.value,
      userId: [Number(props.id), ...additionalUsers.value.map(Number)],
    }),
  { logs: [], users: {} },
  {
    immediate: false,
    resetOnExecute: false,
  }
);

const sortedActivityLogs = computed(() =>
  activityLogsWithUsers.value.logs.sort((a, b) =>
    sort.value === Sort.ASC
      ? a.createdAt.getTime() - b.createdAt.getTime()
      : b.createdAt.getTime() - a.createdAt.getTime()
  )
);

await fetchActivityLogsWithUsers();
</script>

<template>
  <div class="mx-auto max-w-3xl space-y-8 pb-8">
    <OField class="w-full">
      <OField :label="$t('activityLog.form.type')">
        <VDropdown :triggers="['click']">
          <template #default="{ shown }">
            <OInput
              class="w-44 cursor-pointer overflow-x-hidden text-ellipsis"
              :modelValue="types.join(',')"
              type="text"
              size="sm"
              expanded
              :placeholder="$t('activityLog.form.type')"
              :icon="shown ? 'chevron-up' : 'chevron-down'"
              :iconRight="types.length !== 0 ? 'close' : undefined"
              iconRightClickable
              readonly
              @iconRightClick.stop="types = []"
            />
          </template>
          <template #popper>
            <div class="max-h-60 min-w-60 max-w-xs overflow-y-auto">
              <DropdownItem v-for="activityLogType in Object.keys(ActivityLogType)">
                <OCheckbox v-model="types" :nativeValue="activityLogType">
                  {{ activityLogType }}
                </OCheckbox>
              </DropdownItem>
            </div>
          </template>
        </VDropdown>
      </OField>

      <OField :label="$t('activityLog.form.from')">
        <ODateTimePicker
          v-model="from"
          size="sm"
          locale="en"
          clearable
          expanded
          iconRight="calendar"
          datepickerWrapperClass="w-44"
        />
      </OField>

      <OField :label="$t('activityLog.form.to')">
        <ODateTimePicker
          v-model="to"
          size="sm"
          locale="en"
          expanded
          :max="new Date()"
          iconRight="calendar"
          datepickerWrapperClass="w-44"
        />
      </OField>

      <div class="flex items-end gap-4">
        <OButton
          size="sm"
          iconLeft="search"
          :label="$t('action.search')"
          expanded
          variant="primary"
          :loading="isLoadingActivityLogsWithUsers"
          @click="fetchActivityLogsWithUsers"
        />
      </div>
    </OField>

    <div class="flex flex-wrap items-center gap-4">
      <div
        v-for="additionalUserId in additionalUsers"
        class="flex items-center gap-1"
        data-aq-activityLogs-additionalUser
      >
        <OButton
          size="2xs"
          iconLeft="close"
          rounded
          variant="transparent"
          data-aq-activityLogs-additionalUser-remove
          @click="removeAdditionalUser(Number(additionalUserId))"
        />

        <RouterLink
          :to="{ name: 'ModeratorUserIdRestrictions', params: { id: additionalUserId } }"
          class="inline-block hover:text-content-100"
        >
          <UserMedia
            v-if="Number(additionalUserId) in activityLogsWithUsers.users"
            :user="activityLogsWithUsers.users[Number(additionalUserId)]"
          />
        </RouterLink>
      </div>

      <Modal closable :autoHide="false" class="self-end">
        <OButton
          size="xs"
          iconLeft="add"
          :label="$t('activityLog.form.addUser')"
          variant="secondary"
        />

        <template #popper="{ hide }">
          <div class="min-w-[720px] space-y-6 px-12 py-8">
            <div class="pb-4 text-center text-xl text-content-100">
              {{ $t('findUser.title') }}
            </div>
            <UserFinder>
              <template #user-prepend="userData">
                <OButton
                  size="2xs"
                  iconLeft="add"
                  :label="$t('activityLog.form.addUser')"
                  variant="secondary"
                  data-aq-activityLogs-userFinder-addUser-btn
                  @click="
                    {
                      addAdditionalUser(userData.id);
                      hide();
                    }
                  "
                />
              </template>
            </UserFinder>
          </div>
        </template>
      </Modal>

      <div class="ml-auto mr-0">
        <OButton
          size="xs"
          :iconRight="sort === Sort.ASC ? 'chevron-up' : 'chevron-down'"
          variant="secondary"
          :label="$t('activityLog.sort.createdAt')"
          v-tooltip="sort === Sort.ASC ? $t('sort.directions.asc') : $t('sort.directions.desc')"
          data-aq-activityLogs-sort-btn
          @click="toggleSort"
        />
      </div>
    </div>

    <OLoading v-if="isLoadingActivityLogsWithUsers" :fullPage="false" active iconSize="xl" />

    <div v-else class="flex flex-col flex-wrap gap-4">
      <ActivityLogItem
        v-for="activityLog in sortedActivityLogs"
        :activityLog="activityLog"
        :isSelfUser="activityLog.userId === user.id"
        :user="
          activityLog.userId === user.id ? user : activityLogsWithUsers.users[activityLog.userId]
        "
        :users="activityLogsWithUsers.users"
        @addUser="addAdditionalUser"
        @addType="addType"
      />
    </div>
  </div>
</template>
