<script setup lang="ts">
import { type RestrictionWithActive } from '@/models/restriction';
import { parseTimestamp, computeLeftMs } from '@/utils/date';

const props = defineProps<{
  restriction: RestrictionWithActive;
}>();

const joinRestrictionRemainingDuration = computed(() =>
  parseTimestamp(computeLeftMs(props.restriction.createdAt, props.restriction.duration))
);
</script>

<template>
  <div
    class="flex items-center justify-center gap-3 bg-status-danger px-8 py-1.5 text-center text-content-100"
  >
    {{
      $t('user.restriction.notification', {
        duration: $t('dateTimeFormat.dd:hh:mm', {
          ...joinRestrictionRemainingDuration,
        }),
      })
    }}

    <div class="h-4 w-px select-none bg-base-600 bg-opacity-30" />

    <Modal closable>
      <span class="cursor-pointer">{{ $t('action.readMore') }}</span>

      <template #popper>
        <div class="space-y-10 py-10">
          <i18n-t
            scope="global"
            keypath="user.restriction.notification"
            tag="div"
            class="text-center text-lg text-status-danger"
          >
            <template #duration>
              <div class="mt-2 text-xl">
                {{
                  $t('dateTimeFormat.dd:hh:mm', {
                    ...joinRestrictionRemainingDuration,
                  })
                }}
              </div>
            </template>
          </i18n-t>

          <UserRestrictionGuide />
        </div>
      </template>
    </Modal>
  </div>
</template>
