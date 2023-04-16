<script setup lang="ts">
import { useTransition } from '@vueuse/core';
import { type GameServerStats } from '@/models/game-server-stats';
import { n } from '@/services/translate-service';

const gameStatsErrorIndicator = "?";

const props = withDefaults(
  defineProps<{ gameServerStats: GameServerStats | null; showLabel?: boolean }>(),
  {
    showLabel: false,
  }
);

const animatedPlayingCount = useTransition(
  computed(() => props.gameServerStats !== null ? props.gameServerStats.total.playingCount : -1)
);

const animatedPlayingCountString = computed(()=>
  props.gameServerStats !== null
    ? n(Number(animatedPlayingCount.value.toFixed(0)), 'decimal')
    : gameStatsErrorIndicator
);
</script>

<template>
  <VTooltip :disabled="gameServerStats === null || Object.keys(gameServerStats.regions).length === 0">
    <div class="flex select-none items-center gap-1.5 hover:text-content-100">
      <OIcon icon="online" size="lg" class="text-[#53BC96]" style="--fa-secondary-opacity: 0.15" />
      <div v-if="showLabel" data-aq-online-players-count>
        {{ $t('onlinePlayers.format', {count: animatedPlayingCountString}) }}
      </div>
      <div v-else class="w-8" data-aq-online-players-count>
        {{ animatedPlayingCountString }}
      </div>
    </div>
    <template #popper>
      <div class="prose prose-invert space-y-5">
        <h5 class="text-content-100">{{ $t('onlinePlayers.tooltip.title') }}</h5>
        <div class="space-y-3" v-if="gameServerStats !== null" data-aq-region-stats>
          <div
            v-for="(regionServerStats, regionKey) in gameServerStats.regions"
            class="flex w-52 items-center justify-between gap-3"
          >
            <div>{{ $t(`region.${regionKey}`, 0) }}</div>
            <div>{{ regionServerStats!.playingCount }}</div>
          </div>
        </div>
      </div>
    </template>
  </VTooltip>
</template>
