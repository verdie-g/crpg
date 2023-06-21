<script setup lang="ts">
import { getLeaderBoard, createRankTable } from '@/services/leaderboard-service';
import { characterClassToIcon } from '@/services/characters-service';

definePage({
  meta: {
    layout: 'default',
    bg: 'background-2.webp',
    roles: ['User', 'Moderator', 'Admin'],
  },
});

const { state: leaderBoard } = useAsyncState(() => getLeaderBoard(), []);

const rankTable = computed(() => createRankTable());
</script>

<template>
  <div class="container">
    <div class="mx-auto max-w-3xl py-8 md:py-16">
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
          <h1 class="text-2xl text-content-100">Leader Board</h1>
          <SvgSpriteImg name="logo-decor" viewBox="0 0 108 10" class="w-16 md:w-28" />
        </div>
      </div>

      <OTable
        :data="leaderBoard"
        hoverable
        bordered
        sortIcon="chevron-up"
        sortIconSize="xs"
        :defaultSort="['idx', 'asc']"
      >
        <OTableColumn #default="{ row }" field="idx" label="Top" :width="60" sortable>
          {{ row.idx }}
        </OTableColumn>

        <OTableColumn #default="{ row }" field="rating" label="Rank" :width="210">
          <Rank :rankTable="rankTable" :competitiveValue="row.rating.competitiveValue" />
        </OTableColumn>

        <OTableColumn #default="{ row }" field="user.name" label="Player" :width="140">
          <UserMedia :user="row.user" hiddenPlatform />
        </OTableColumn>

        <OTableColumn #default="{ row }" field="class" label="Class" :width="60" sortable>
          <OIcon
            :icon="characterClassToIcon[row.class]"
            size="lg"
            v-tooltip="$t(`character.class.${row.class}`)"
          />
        </OTableColumn>

        <OTableColumn #default="{ row }" field="level" label="Lvl" :width="60">
          {{ row.level }}
        </OTableColumn>

        <OTableColumn #default="{ row }" field="user.region" label="Region" sortable>
          {{ $t(`region.${row.user.region}`, 0) }}
        </OTableColumn>

        <template #empty>
          <ResultNotFound />
        </template>
      </OTable>
    </div>
  </div>
</template>
