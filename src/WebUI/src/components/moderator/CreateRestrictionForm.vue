<script setup lang="ts">
import { type HumanDuration } from '@/models/datetime';
import { restrictUser } from '@/services/restriction-service';
import { type RestrictionCreation, RestrictionType } from '@/models/restriction';
import { convertHumanDurationToMs } from '@/utils/date';
import { notify } from '@/services/notification-service';
import { t } from '@/services/translate-service';

const props = defineProps<{ userId: number }>();

const emit = defineEmits<{
  (e: 'restrictionCreated'): void;
}>();

const newRestrictionModel = ref<Omit<RestrictionCreation, 'restrictedUserId'>>({
  reason: '',
  duration: 0,
  type: RestrictionType.Join,
});

const durationModel = ref<HumanDuration>({
  days: 0,
  hours: 0,
  minutes: 0,
});

const durationSeconds = computed(() => convertHumanDurationToMs(durationModel.value));

const addRestriction = async () => {
  await restrictUser({
    ...newRestrictionModel.value,
    restrictedUserId: props.userId,
    duration: durationSeconds.value,
  });

  notify(t('restriction.create.notify.success'));

  durationModel.value = {
    days: 0,
    hours: 0,
    minutes: 0,
  };

  newRestrictionModel.value = {
    reason: '',
    duration: 0,
    type: RestrictionType.Join,
  };

  emit('restrictionCreated');
};
</script>

<template>
  <div class="mb-12 space-y-8">
    <form @submit.prevent="addRestriction" class="space-y-8">
      <OField>
        <OField :label="$t('restriction.create.form.field.type.label')">
          <VDropdown :triggers="['click']">
            <template #default="{ shown }">
              <OButton
                :label="$t(`restriction.type.${newRestrictionModel.type}`)"
                variant="secondary"
                size="lg"
                :iconRight="shown ? 'chevron-up' : 'chevron-down'"
              />
            </template>

            <template #popper="{ hide }">
              <DropdownItem v-for="rt in Object.keys(RestrictionType)" class="min-w-60 max-w-xs">
                <ORadio v-model="newRestrictionModel.type" :native-value="rt" @change="hide">
                  {{ $t(`restriction.type.${rt}`) }}
                </ORadio>
              </DropdownItem>
            </template>
          </VDropdown>
        </OField>

        <OField message="Use a duration of 0 to un-restrict">
          <OField :label="$t('restriction.create.form.field.days.label')">
            <OInput v-model="durationModel.days" size="lg" class="w-20" required type="number" />
          </OField>

          <OField :label="$t('restriction.create.form.field.hours.label')">
            <OInput v-model="durationModel.hours" size="lg" class="w-20" required type="number" />
          </OField>

          <OField :label="$t('restriction.create.form.field.minutes.label')">
            <OInput v-model="durationModel.minutes" size="lg" class="w-20" required type="number" />
          </OField>
        </OField>
      </OField>

      <OField :label="$t('restriction.create.form.field.reason.label')">
        <OInput
          placeholder=""
          v-model="newRestrictionModel.reason"
          size="lg"
          class="w-96"
          required
          type="textarea"
          rows="5"
        />
      </OField>

      <OButton
        native-type="submit"
        variant="primary"
        size="lg"
        :label="$t('restriction.create.form.action.submit')"
      />
    </form>
  </div>
</template>
