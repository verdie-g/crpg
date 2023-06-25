<script setup lang="ts">
import { type Rank } from '@/models/competitive';
import { groupBy } from '@/utils/array';
import { inRange } from '@/utils/math';

const { competitiveValue, rankTable } = defineProps<{
  competitiveValue: number;
  rankTable: Rank[];
}>();

const groupedRankTable = computed(() => groupBy(rankTable.reverse(), r => r.groupTitle));
</script>

<template>
  <OTable :data="Object.entries(groupedRankTable)" bordered>
    <OTableColumn #default="{ row }: { row: [string, Rank[]] }">
      <span :style="{ color: row[1][0].color }">
        {{ row[0] }}
      </span>
    </OTableColumn>

    <OTableColumn
      v-for="(col, idx) in 5"
      #default="{ row }: { row: [string, Rank[]] }"
      :label="col"
    >
      <span
        v-if="inRange(competitiveValue, row[1][idx].min, row[1][idx].max)"
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
</template>
