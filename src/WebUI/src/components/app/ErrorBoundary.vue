<script setup lang="ts">
const props = withDefaults(defineProps<{ stopPropagation?: boolean }>(), {
  stopPropagation: true,
});

const router = useRouter();

const err = ref<Error | null>(null);
const info = ref<string | null>(null);

onErrorCaptured((_err, _vm, _info) => {
  console.error(_err); // TODO: send to datadog
  err.value = _err;
  info.value = _info;
  return !props.stopPropagation;
});

const goToRootPage = () => {
  const charactersRoute = router.resolve({ name: 'Root' });
  globalThis.location.href = charactersRoute.href;
};
</script>

<template>
  <div>
    <slot v-if="err !== null" name="error" v-bind="{ err, info }">
      <div class="flex h-screen w-full items-center justify-center">
        <div class="mx-auto max-w-lg rounded-xl border border-border-200 p-6">
          <div class="prose prose-invert px-12 pb-6 text-center">
            <OIcon size="5x" icon="error" />
            <h4 class="text-sm text-content-200">{{ $t('error.title') }}</h4>
          </div>

          <Divider />

          <div class="prose prose-invert px-12 py-6">
            <p>{{ $t('error.intro') }}</p>

            <Divider />

            <i18n-t scope="global" keypath="error.discord" tag="p">
              <template #discordLink>
                <a
                  class="text-content-link hover:text-content-link-hover"
                  href="https://discord.gg/c-rpg"
                  target="_blank"
                >
                  Discord
                </a>
              </template>
            </i18n-t>

            <i18n-t scope="global" keypath="error.modMail" tag="p">
              <template #modMailGuideLink>
                <a
                  class="text-content-link hover:text-content-link-hover"
                  href="https://discord.com/channels/279063743839862805/1034895358435799070"
                  target="_blank"
                >
                  {{ $t('here') }}
                </a>
              </template>
            </i18n-t>

            <Divider />

            <p>{{ $t('error.report.title') }}</p>
            <ol>
              <li>{{ $t('error.report.step.reproduce') }}</li>
              <li>{{ $t('error.report.step.repeat') }}</li>
            </ol>
          </div>

          <Divider />

          <div class="flex items-center justify-center px-12 pt-6">
            <OButton
              variant="primary"
              outlined
              size="xl"
              iconLeft="region"
              :label="$t('error.goToHomePage')"
              @click="goToRootPage"
            />
          </div>
        </div>
      </div>
    </slot>
    <slot v-else />
  </div>
</template>
