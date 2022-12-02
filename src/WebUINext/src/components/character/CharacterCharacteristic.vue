<script setup lang="ts">
import {
  CharacteristicConversion,
  type CharacterCharacteristics,
  type SkillKey,
} from '@/models/character';
import { useCharacterCharacteristic } from '@/composables/character/use-character-characteristic';

const props = defineProps<{
  characteristics: CharacterCharacteristics;
}>();

const emit = defineEmits<{
  (e: 'convertCharacterCharacteristics', conversion: CharacteristicConversion): void;
  (e: 'commit', characteristics: CharacterCharacteristics): void;
}>();

const {
  characteristics,
  //
  wasChangeMade,
  isChangeValid,
  currentSkillRequirementsSatisfied,
  canConvertSkillsToAttributes,
  canConvertAttributesToSkills,
  //
  formSchema,
  //
  onInput,
  getInputProps,
  convertAttributeToSkills,
  convertSkillsToAttribute,
  reset,
} = useCharacterCharacteristic(toRef(props, 'characteristics'));

const commit = () => {
  emit('commit', characteristics.value);
  reset();
};

const convertCharacteristics = (conversion: CharacteristicConversion) => {
  if (conversion === CharacteristicConversion.AttributesToSkills) {
    convertAttributeToSkills();
  } else if (conversion === CharacteristicConversion.SkillsToAttributes) {
    convertSkillsToAttribute();
  }

  emit('convertCharacterCharacteristics', conversion);
};
</script>

<template>
  <div class="">
    <div class="statsGrid mb-8 grid gap-6">
      <div
        class="space-y-3"
        v-for="fieldsGroup in formSchema"
        :style="{ 'grid-area': fieldsGroup.key }"
      >
        <div
          class="flex items-center justify-between gap-4"
          :data-aq-fields-group="fieldsGroup.key"
        >
          <div class="">
            {{ fieldsGroup.label }} -
            <span class="font-bold">{{ characteristics[fieldsGroup.key].points }}</span>
          </div>

          <template v-if="fieldsGroup.key === 'attributes'">
            <OButton
              variant="secondary"
              size="xs"
              :disabled="!canConvertAttributesToSkills"
              data-aq-convert-attributes-action
              @click="convertCharacteristics(CharacteristicConversion.AttributesToSkills)"
            >
              Convert to skills
            </OButton>
          </template>

          <template v-else-if="fieldsGroup.key === 'skills'">
            <OButton
              variant="secondary"
              size="xs"
              :disabled="!canConvertSkillsToAttributes"
              data-aq-convert-skills-action
              @click="convertCharacteristics(CharacteristicConversion.SkillsToAttributes)"
            >
              Convert to attributes
            </OButton>
          </template>
        </div>

        <div class="rounded-xl border border-border-200 py-2">
          <div
            v-for="field in fieldsGroup.children"
            class="flex items-center justify-between px-4 py-2.5 hover:bg-base-200"
          >
            <div class="text-2xs">{{ field.label }}</div>

            <div class="flex items-center gap-2">
              <div
                v-if="fieldsGroup.key === 'skills' && !currentSkillRequirementsSatisfied(field.key as SkillKey)"
                data-aq-validation-warning
              >
                <!-- TODO: to field variant -->
                ooops!
              </div>
              <!-- value, min, max, controls - hide +/- -->
              <!-- <OButton variant="neutral">-</OButton> -->
              <NumericInput
                :exponential="0.5"
                :data-aq-control="`${fieldsGroup.key}:${field.key}`"
                v-bind="getInputProps(fieldsGroup.key, field.key)"
                @update:modelValue="onInput(fieldsGroup.key, field.key, $event)"
              />
              <!-- <OButton variant="neutral">+</OButton> -->
            </div>
          </div>
        </div>
      </div>
    </div>

    <div
      class="sticky left-0 bottom-0 flex w-full items-center justify-center gap-2 bg-bg-main bg-opacity-10 py-4 backdrop-blur-sm"
    >
      <OButton
        :disabled="!wasChangeMade"
        variant="secondary"
        outlined
        size="sm"
        iconLeft="close"
        label="Reset"
        data-aq-reset-action
        @click="reset"
      />
      <OButton
        variant="primary"
        size="sm"
        :disabled="!wasChangeMade || !isChangeValid"
        label="Commit"
        data-aq-commit-action
        @click="commit"
      />
    </div>
  </div>
</template>

<style lang="css">
.statsGrid {
  grid-template-areas:
    'attributes skills'
    'weaponProficiencies skills';

  grid-template-columns: 1fr 1fr;
  grid-template-rows: auto auto;
}
</style>
