<script setup lang="ts">
const router = useRouter();

const refreshPage = () => {
  const charactersRoute = router.resolve({ name: 'Characters' });
  globalThis.location.href = charactersRoute.href;
};
</script>

<template>
  <Modal closable>
    <template #popper>
      <div class="border-b border-border-200 px-12 pt-11 pb-8 text-center">
        <h3 class="text-xl text-content-100">{{ $t('character.create.guide.title') }}</h3>
      </div>

      <div class="prose prose-invert border-b border-border-200 px-12 py-8">
        <p class="text-content-400">
          {{ $t('character.create.guide.intro') }}
        </p>
        <ol>
          <li>{{ $t('character.create.guide.step.install') }}</li>
          <li>{{ $t('character.create.guide.step.join') }}</li>
          <li>{{ $t('character.create.guide.step.joined') }}</li>
          <i18n-t scope="global" keypath="character.create.guide.step.refresh" tag="li">
            <template #refresh>
              <span
                class="cursor-pointer text-content-link underline hover:text-content-link-hover hover:no-underline"
                @click="refreshPage"
              >
                {{ $t('action.refresh', 1) }}
              </span>
            </template>
          </i18n-t>
        </ol>
      </div>

      <div class="px-12 pt-4 pb-4">
        <p class="text-center text-status-warning">
          {{ $t('character.create.guide.outro') }}
        </p>
      </div>
    </template>
  </Modal>
</template>
