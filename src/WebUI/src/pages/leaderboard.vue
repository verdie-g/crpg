<script setup lang="ts">
import { type CharacterCompetitiveNumbered } from '@/models/competitive';
import { CharacterClass } from '@/models/character';

import { getLeaderBoard, createRankTable } from '@/services/leaderboard-service';
import { characterClassToIcon } from '@/services/characters-service';
import { useUserStore } from '@/stores/user';
import { useRegion } from '@/composables/use-region';

definePage({
  meta: {
    layout: 'default',
    bg: 'background-2.webp',
    roles: ['User', 'Moderator', 'Admin'],
  },
});

const route = useRoute();
const router = useRouter();

const userStore = useUserStore();

const { regionModel, regions } = useRegion();

const characterClassModel = computed({
  get() {
    return (route.query?.class as CharacterClass) || undefined;
  },

  set(characterClass: CharacterClass | undefined) {
    router.replace({
      query: {
        ...route.query,
        class: characterClass,
      },
    });
  },
});

const characterClasses = Object.values(CharacterClass);

const {
  state: leaderboard,
  execute: loadLeaderBoard,
  isLoading: leaderBoardLoading,
} = useAsyncState(
  () => getLeaderBoard({ region: regionModel.value, characterClass: characterClassModel.value }),
  [],
  {}
);

watch(
  () => route.query,
  async () => {
    await loadLeaderBoard();
  }
);

const rankTable = computed(() => createRankTable());

const isSelfUser = (row: CharacterCompetitiveNumbered) => row.user.id === userStore.user!.id;

const rowClass = (row: CharacterCompetitiveNumbered) =>
  isSelfUser(row) ? 'text-primary' : 'text-content-100';
</script>

<template>
  <div class="container">
    <div class="mx-auto max-w-4xl py-8 md:py-16">
      <div class="container mb-20">
        <div class="mb-5 flex justify-center">
          <OIcon icon="trophy-cup" size="5x" class="text-more-support" />
        </div>

        <div class="item-center flex select-none justify-center gap-4 md:gap-8">
          <SvgSpriteImg
            name="logo-decor"
            viewBox="0 0 108 10"
            class="w-16 rotate-180 transform md:w-28"
          />
          <h1 class="text-2xl text-content-100">{{ $t('leaderboard.title') }}</h1>

          <SvgSpriteImg name="logo-decor" viewBox="0 0 108 10" class="w-16 md:w-28" />
        </div>
      </div>

      <div class="flex items-center justify-between gap-4">
        <OTabs v-model="regionModel" contentClass="hidden" class="mb-6">
          <OTabItem v-for="region in regions" :label="$t(`region.${region}`, 0)" :value="region" />
        </OTabs>

        <Modal closable>
          <Tag icon="popup" variant="primary" rounded size="lg" />
          <template #popper>
            <RankTable :rankTable="rankTable" />
          </template>
        </Modal>
      </div>

      <OTable
        :data="leaderboard"
        hoverable
        bordered
        sortIcon="chevron-up"
        sortIconSize="xs"
        :loading="leaderBoardLoading"
        :rowClass="rowClass"
        :defaultSort="['position', 'asc']"
      >
        <OTableColumn
          #default="{ row }: { row: CharacterCompetitiveNumbered }"
          field="position"
          :label="$t('leaderboard.table.cols.top')"
          :width="80"
          sortable
        >
          {{ row.position }}
        </OTableColumn>

        <OTableColumn
          #default="{ row }: { row: CharacterCompetitiveNumbered }"
          field="rating.competitiveValue"
          :label="$t('leaderboard.table.cols.rank')"
          :width="230"
        >
          <Rank :rankTable="rankTable" :competitiveValue="row.rating.competitiveValue" />
        </OTableColumn>

        <OTableColumn
          #default="{ row }: { row: CharacterCompetitiveNumbered }"
          field="user.name"
          :label="$t('leaderboard.table.cols.player')"
        >
          <UserMedia :user="row.user" :clan="row.user.clan" hiddenPlatform class="max-w-[20rem]" />
        </OTableColumn>

        <OTableColumn field="class" :width="80">
          <template #header>
            <div class="relative flex items-center gap-1">
              <OIcon
                v-if="characterClassModel"
                class="absolute -left-5 top-1/2 -translate-y-1/2 transform cursor-pointer hover:text-status-danger"
                v-tooltip.bottom="$t('action.reset')"
                icon="close"
                size="xs"
                @click="characterClassModel = undefined"
              />
              <VDropdown :triggers="['click']">
                <div
                  class="max-w-[90px] cursor-pointer overflow-x-hidden text-ellipsis whitespace-nowrap border-b-2 border-dashed border-border-300 pb-0.5 text-2xs hover:text-content-100 2xl:max-w-[120px]"
                >
                  {{ $t('leaderboard.table.cols.class') }}
                </div>

                <template #popper="{ hide }">
                  <div class="max-w-md">
                    <DropdownItem
                      v-for="characterClass in characterClasses"
                      :checked="characterClass === characterClassModel"
                      @click="
                        () => {
                          characterClassModel = characterClass;
                          hide();
                        }
                      "
                    >
                      <OIcon :icon="characterClassToIcon[characterClass]" size="lg" />
                      {{ $t(`character.class.${characterClass}`) }}
                    </DropdownItem>
                  </div>
                </template>
              </VDropdown>
            </div>
          </template>

          <template #default="{ row }: { row: CharacterCompetitiveNumbered }">
            <OIcon
              :icon="characterClassToIcon[row.class]"
              size="lg"
              v-tooltip="$t(`character.class.${row.class}`)"
            />
          </template>
        </OTableColumn>

        <OTableColumn
          #default="{ row }: { row: CharacterCompetitiveNumbered }"
          field="level"
          :label="$t('leaderboard.table.cols.level')"
          sortable
          :width="80"
        >
          {{ row.level }}
        </OTableColumn>

        <template #empty>
          <ResultNotFound />
        </template>
      </OTable>
    </div>
  </div>
</template>
