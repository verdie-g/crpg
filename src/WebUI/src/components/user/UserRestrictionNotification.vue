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
        <div class="prose prose-invert">
          <div class="border-b border-border-200 px-12 pt-11 pb-8 text-center text-status-danger">
            <i18n-t
              scope="global"
              keypath="user.restriction.notification"
              tag="div"
              class="text-lg"
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
          </div>

          <div class="mb-2.5 border-b border-border-200 px-12 pt-6 pb-8">
            <p class="text-content-400">
              {{ $t('user.restriction.guide.intro') }}
            </p>
            <ol>
              <i18n-t scope="global" keypath="user.restriction.guide.step.join" tag="li">
                <template #discordLink>
                  <a
                    class="text-content-link hover:text-content-link-hover"
                    target="_blank"
                    href="https://discord.gg/c-rpg"
                  >
                    Discord
                  </a>
                </template>
              </i18n-t>
              <i18n-t scope="global" keypath="user.restriction.guide.step.navigate" tag="li">
                <template #modMailLink>
                  <!-- prettier-ignore -->
                  <a
                    class="text-content-link hover:text-content-link-hover"
    target="_blank"

                    href="https://discord.com/channels/279063743839862805/1034895358435799070"
                  >ModMail</a>
                </template>
              </i18n-t>
              <li>{{ $t('user.restriction.guide.step.follow') }}</li>
            </ol>
          </div>

          <div class="px-12 pt-4 pb-4">
            <p class="text-content-400">
              {{ $t('user.restriction.guide.outro') }}
            </p>
          </div>
        </div>
      </template>
    </Modal>
  </div>
</template>
