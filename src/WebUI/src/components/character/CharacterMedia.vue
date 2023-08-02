<script setup lang="ts">
import { type Character } from '@/models/character';
import { characterClassToIcon } from '@/services/characters-service';

const { character, isActive = false } = defineProps<{
  character: Character;
  isActive: boolean;
}>();
</script>

<template>
  <div class="flex items-center gap-2">
    <OIcon
      :icon="characterClassToIcon[character.class]"
      size="lg"
      v-tooltip="$t(`character.class.${character.class}`)"
    />

    <div class="flex items-center gap-1">
      <div class="max-w-[150px] overflow-x-hidden text-ellipsis whitespace-nowrap">
        {{ character.name }}
      </div>

      <div>({{ character.level }})</div>
    </div>

    <Tag
      v-if="isActive"
      :label="$t('character.status.active.short')"
      v-tooltip="$t('character.status.active.title')"
      variant="success"
      size="sm"
    />

    <Tag
      v-if="character.forTournament"
      :label="$t('character.status.forTournament.short')"
      v-tooltip="$t('character.status.forTournament.title')"
      variant="warning"
      size="sm"
    />
  </div>
</template>
