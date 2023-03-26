<script setup lang="ts">
import { useTransition } from '@vueuse/core';
import { type GameServerStats } from '@/models/game-server-stats';

const props = withDefaults(
  defineProps<{ gameServerStats: GameServerStats; showLabel?: boolean }>(),
  {
    showLabel: false,
  }
);

const animatedPlayingCount = useTransition(
  computed(() => props.gameServerStats.total.playingCount)
);
</script>

<template>
  <VTooltip :disabled="Object.keys(gameServerStats.regions).length === 0">
    <div class="flex select-none items-center gap-1.5 hover:text-content-100">
      <OIcon icon="online" size="lg" class="text-[#53BC96]" style="--fa-secondary-opacity: 0.15" />
      <div v-if="showLabel">
        {{
          $t('onlinePlayers.format', {
            count: $n(Number(animatedPlayingCount.toFixed(0)), 'decimal'),
          })
        }}
      </div>
      <div v-else class="w-8">
        {{ $n(Number(animatedPlayingCount.toFixed(0)), 'decimal') }}
      </div>
    </div>
    <template #popper>
      <div class="prose prose-invert space-y-5">
        <h5 class="text-content-100">{{ $t('onlinePlayers.tooltip.title') }}</h5>
        <div class="space-y-3">
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
