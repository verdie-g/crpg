<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core';
import { sameAs } from '@/services/validators-service';

const props = defineProps<{
  title: string;
  description?: string;
  name: string;
  confirmLabel: string;
}>();

const emit = defineEmits<{
  (e: 'cancel'): void;
  (e: 'confirm'): void;
}>();

const confirmNameModel = ref<string>('');
const $v = useVuelidate(
  {
    confirmNameModel: {
      sameAs: sameAs(props.name, props.name),
    },
  },
  { confirmNameModel }
);

const onCancel = () => {
  confirmNameModel.value = '';
  $v.value.$reset();
  emit('cancel');
};

const onConfirm = async () => {
  if (!(await $v.value.$validate())) return;

  emit('confirm');
};
</script>

<template>
  <div class="max-w-lg space-y-6 px-12 py-11 text-center">
    <h4 class="text-xl">{{ title }}</h4>

    <div class="space-y-4">
      <slot v-if="$slots.description" name="description"></slot>
      <p v-else-if="description !== undefined">{{ description }}</p>

      <i18n-t scope="global" keypath="confirm.name" tag="p">
        <template #name>
          <span class="font-bold text-primary">{{ name }}</span>
        </template>
      </i18n-t>

      <OField
        v-bind="{
          ...($v.confirmNameModel.$error && {
            variant: 'danger',
            message: $v.$errors[0].$message,
          }),
        }"
        data-aq-confirm-field
        class="mx-auto max-w-sm"
      >
        <OInput
          v-model="confirmNameModel"
          :placeholder="$t('confirm.placeholder')"
          size="sm"
          expanded
          data-aq-confirm-input
          @blur="$v.$touch"
          @focus="$v.$reset"
        />
      </OField>
    </div>

    <div class="flex items-center justify-center gap-4">
      <OButton
        variant="primary"
        outlined
        size="xl"
        :label="$t('action.cancel')"
        data-aq-confirm-action="cancel"
        @click="onCancel"
      />
      <OButton
        :disabled="$v.confirmNameModel.$invalid"
        variant="primary"
        size="xl"
        :label="confirmLabel"
        data-aq-confirm-action="submit"
        @click="onConfirm"
      />
    </div>
  </div>
</template>
