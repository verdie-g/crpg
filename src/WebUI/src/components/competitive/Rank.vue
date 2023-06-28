<script setup lang="ts">
import { useTransition } from '@vueuse/core';
import { type Rank } from '@/models/competitive';
import { getRankByCompetitiveValue } from '@/services/leaderboard-service';

const { competitiveValue, rankTable } = defineProps<{
  competitiveValue: number;
  rankTable: Rank[];
}>();

const animatedCompetitiveValue = useTransition(toRef(() => competitiveValue));

const rank = computed(() => getRankByCompetitiveValue(rankTable, competitiveValue));
</script>

<template>
  <div class="font-black" :style="{ color: rank.color }">
    {{ rank.title }} - {{ $n(animatedCompetitiveValue, { maximumFractionDigits: 0 }) }}
  </div>
</template>
