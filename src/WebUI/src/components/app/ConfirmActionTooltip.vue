<script setup lang="ts">
const props = defineProps<{
  title?: string;
  confirmLabel?: string;
}>();

const emit = defineEmits<{
  (e: 'cancel'): void;
  (e: 'confirm'): void;
}>();
</script>

<template>
  <VTooltip :triggers="['click']">
    <slot />
    <template #popper="{ hide }">
      <div class="space-y-3">
        <div>
          {{ title !== undefined ? title : $t('confirmAction') }}
        </div>

        <div class="flex items-center gap-2">
          <OButton
            variant="success"
            size="2xs"
            iconLeft="check"
            :label="confirmLabel !== undefined ? confirmLabel : $t('action.confirm')"
            @click="
              () => {
                emit('confirm');
                hide();
              }
            "
          />
          <OButton
            variant="danger"
            size="2xs"
            iconLeft="close"
            :label="$t('action.cancel')"
            @click="hide"
          />
        </div>
      </div>
    </template>
  </VTooltip>
</template>
