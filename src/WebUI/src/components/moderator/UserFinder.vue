<script setup lang="ts">
import { Platform } from '@/models/platform';
import { type UserPublic } from '@/models/user';
import { getUserById, searchUser } from '@/services/users-service';
import { platformToIcon } from '@/services/platform-service';

enum SearchMode {
  Name = 'Name',
  Platform = 'Platform',
  Id = 'Id',
}

const activeSearchMode = ref<SearchMode>(SearchMode.Name);
const users = ref<UserPublic[]>([]);
const searchByNameModel = ref<string>('');
const searchByIdModel = ref<number>(0);
const searchByPlatformModel = ref<{ platform: Platform; platformUserId: string }>({
  platform: Platform.Steam,
  platformUserId: '',
});

const search = async () => {
  if (activeSearchMode.value === SearchMode.Id) {
    users.value = [await getUserById(searchByIdModel.value)];
    return;
  }

  const payload =
    activeSearchMode.value === SearchMode.Name
      ? {
          name: searchByNameModel.value,
        }
      : {
          platform: searchByPlatformModel.value.platform,
          platformUserId: searchByPlatformModel.value.platformUserId,
        };

  users.value = await searchUser(payload);
};

const clearUsers = () => {
  users.value = [];
};
</script>

<template>
  <div>
    <OTabs v-model="activeSearchMode" :animated="false" class="mb-8">
      <OTabItem :label="$t('findUser.mode.Name.label')" :value="SearchMode.Name">
        <form @submit.prevent="search" class="rounded-xl border border-border-200 p-6">
          <OField :label="$t('findUser.mode.Name.field.name.label')">
            <OInput
              :placeholder="$t('findUser.mode.Name.field.name.placeholder')"
              v-model="searchByNameModel"
              size="lg"
              class="w-72"
              required
              icon="search"
              clearable
              @input="clearUsers"
            />
            <OButton
              class="w-28"
              native-type="submit"
              variant="primary"
              size="lg"
              :label="$t('action.find')"
            />
          </OField>
        </form>
      </OTabItem>

      <OTabItem :label="$t('findUser.mode.Platform.label')" :value="SearchMode.Platform">
        <form @submit.prevent="search" class="rounded-xl border border-border-200 p-6">
          <OField>
            <OField :label="$t('findUser.mode.Platform.field.platform.label')">
              <VDropdown :triggers="['click']">
                <template #default="{ shown }">
                  <OButton
                    variant="secondary"
                    size="lg"
                    :iconLeft="platformToIcon[searchByPlatformModel.platform]"
                    :label="$t(`platform.${searchByPlatformModel.platform}`)"
                    :iconRight="shown ? 'chevron-up' : 'chevron-down'"
                  />
                </template>

                <template #popper="{ hide }">
                  <DropdownItem
                    v-for="p in Object.values(Platform)"
                    :checked="p === searchByPlatformModel.platform"
                    :label="$t(`platform.${p}`)"
                    :icon="platformToIcon[p]"
                    data-aq-platform-item
                    @click="
                      () => {
                        searchByPlatformModel.platform = p;
                        hide();
                      }
                    "
                  />
                </template>
              </VDropdown>
            </OField>

            <OField :label="$t('findUser.mode.Platform.field.platformId.label')">
              <OInput
                :placeholder="$t('findUser.mode.Platform.field.platformId.placeholder')"
                v-model="searchByPlatformModel.platformUserId"
                size="lg"
                class="w-80"
                required
                icon="search"
                clearable
                @input="clearUsers"
              />
              <OButton
                class="w-28"
                native-type="submit"
                variant="primary"
                size="lg"
                :label="$t('action.find')"
              />
            </OField>
          </OField>
        </form>
      </OTabItem>

      <OTabItem :label="$t('findUser.mode.Id.label')" :value="SearchMode.Id">
        <form @submit.prevent="search" class="rounded-xl border border-border-200 p-6">
          <OField :label="$t('findUser.mode.Id.field.id.label')">
            <OInput
              :placeholder="$t('findUser.mode.Id.field.id.placeholder')"
              v-model="searchByIdModel"
              size="lg"
              class="w-72"
              icon="search"
              type="number"
              clearable
              @input="clearUsers"
            />
            <OButton
              class="w-28"
              native-type="submit"
              variant="primary"
              size="lg"
              :label="$t('action.find')"
            />
          </OField>
        </form>
      </OTabItem>
    </OTabs>

    <template v-if="users.length">
      <h4 class="mb-4">{{ $t('findUser.result.title') }}</h4>
      <div class="max-h-[480px] space-y-6 overflow-y-auto">
        <div v-for="user in users" :key="user.id" class="flex items-center gap-2">
          <RouterLink
            :to="{ name: 'ModeratorUserIdRestrictions', params: { id: user.id } }"
            class="inline-block hover:text-content-100"
          >
            <UserMedia :user="user" size="xl" />
          </RouterLink>

          <slot name="user-prepend" v-bind="user" />
        </div>
      </div>
    </template>
  </div>
</template>
