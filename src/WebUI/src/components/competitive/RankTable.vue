<script setup lang="ts">
import { type Rank } from '@/models/competitive';
import { groupBy } from '@/utils/array';
import { inRange } from '@/utils/math';

const { competitiveValue = null, rankTable } = defineProps<{
  competitiveValue?: number | null;
  rankTable: Rank[];
}>();

const groupedRankTable = computed(() => groupBy([...rankTable].reverse(), r => r.groupTitle));
</script>

<template>
  <div class="max-h-[90vh] space-y-8 overflow-y-auto px-12 pb-6 pt-8 text-center">
    <h4 class="text-xl">{{ $t('rankTable.title') }}</h4>

    <OTable :data="Object.entries(groupedRankTable)" bordered>
      <OTableColumn #default="{ row }: { row: [string, Rank[]] }">
        <span :style="{ color: row[1][0].color }">
          {{ row[0] }}
        </span>
      </OTableColumn>

      <OTableColumn
        v-for="(col, idx) in 5"
        #default="{ row }: { row: [string, Rank[]] }"
        :label="String(col)"
      >
        <span
          v-if="
            competitiveValue !== null && inRange(competitiveValue, row[1][idx].min, row[1][idx].max)
          "
          :style="{ color: row[1][idx].color }"
          class="font-black"
        >
          {{ row[1][idx].min }} - {{ row[1][idx].max }} ({{ $t('you') }})
        </span>

        <span v-else :style="{ color: row[1][idx].color }">
          {{ row[1][idx].min }} - {{ row[1][idx].max }}
        </span>
      </OTableColumn>
    </OTable>

    <div class="prose prose-invert space-y-3 text-left">
      <h5 class="text-content-100">{{ $t('character.statistics.rank.tooltip.title') }}</h5>
      <div v-html="$t('character.statistics.rank.tooltip.desc')" class="text-2xs" />
    </div>
  </div>
</template>
