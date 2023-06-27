<script setup lang="ts">
import { type Rank } from '@/models/competitive';
import { getRankByCompetitiveValue } from '@/services/leaderboard-service';

const { competitiveValue, rankTable } = defineProps<{
  competitiveValue: number;
  rankTable: Rank[];
}>();

const rank = computed(() => getRankByCompetitiveValue(rankTable, competitiveValue));
</script>

<template>
  <div class="font-black" :style="{ color: rank.color }">
    {{ rank.title }} - {{ $n(competitiveValue, { maximumFractionDigits: 0 }) }}
  </div>
</template>
