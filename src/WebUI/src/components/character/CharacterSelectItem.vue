<script setup lang="ts">
import { type Character } from '@/models/character';

const { character } = defineProps<{
  character: Character;
}>();

const modelValue = defineModel<boolean>();
</script>

<template>
  <div class="flex flex-1 items-center gap-2">
    <VTooltip placement="auto">
      <OSwitch v-model="modelValue" @click.stop />

      <template #popper>
        <div class="prose prose-invert">
          <h5 class="text-content-100">
            {{ $t('character.settings.active.tooltip.title') }}
          </h5>
          <div v-html="$t('character.settings.active.tooltip.desc')"></div>
        </div>
      </template>
    </VTooltip>

    <CharacterMedia :character="character" :isActive="modelValue!" />
  </div>
</template>
