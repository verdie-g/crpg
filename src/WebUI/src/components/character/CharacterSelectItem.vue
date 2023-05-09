<script setup lang="ts">
import { type Character } from '@/models/character';
import { characterClassToIcon } from '@/services/characters-service';

const props = defineProps<{
  character: Character;
  isCharacterActive: boolean;
}>();

const activeCharacterModel = computed({
  get() {
    return props.isCharacterActive;
  },

  set(_val: boolean) {
    emit('activate');
  },
});

const emit = defineEmits<{
  (e: 'activate'): void;
}>();
</script>

<template>
  <div class="flex items-center gap-2">
    <OIcon
      :icon="characterClassToIcon[character.class]"
      size="lg"
      v-tooltip="$t(`character.class.${character.class}`)"
    />

    {{ $t('character.format.select', { name: character.name, level: character.level }) }}

    <div class="flex items-center gap-0.5">
      <Tag
        v-if="character.forTournament"
        :label="$t('character.status.forTournament.short')"
        v-tooltip="$t('character.status.forTournament.title')"
        variant="warning"
        size="sm"
      />

      <VTooltip v-else placement="auto">
        <OSwitch
          v-model="activeCharacterModel"
          data-onboarding-character-action="switch-active-status"
          @click.stop
        />

        <template #popper>
          <div class="prose prose-invert">
            <h5 class="text-content-100">
              {{ $t('character.settings.active.tooltip.title') }}
            </h5>
            <div v-html="$t('character.settings.active.tooltip.desc')"></div>
          </div>
        </template>
      </VTooltip>
    </div>
  </div>
</template>
