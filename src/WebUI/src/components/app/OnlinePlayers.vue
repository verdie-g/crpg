<script setup lang="ts">
import { type GameServerStats } from '@/models/game-server-stats';

defineProps<{ gameServerStats: GameServerStats }>();
</script>

<template>
  <VTooltip :disabled="Object.keys(gameServerStats.regions).length === 0">
    <div class="flex select-none items-center gap-1.5 hover:text-content-100">
      <OIcon icon="online" size="lg" class="text-[#53BC96]" style="--fa-secondary-opacity: 0.15" />
      {{
        $t('mainpage.online', {
          count: $n(gameServerStats.total.playingCount, 'decimal'),
        })
      }}
    </div>
    <template #popper>
      <div class="space-y-3">
        <div
          v-for="(regionServerStats, regionKey) in gameServerStats.regions"
          class="flex w-52 items-center justify-between gap-3"
        >
          <div class="text-content-100">{{ $t(`region.${regionKey}`, 0) }}</div>
          <div class="text-content-200">{{ regionServerStats!.playingCount }}</div>
        </div>
      </div>
    </template>
  </VTooltip>
</template>
