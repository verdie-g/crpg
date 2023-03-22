<script setup lang="ts">
import { supportedLocales, currentLocale, switchLanguage } from '@/services/translate-service';

const locale = computed(() => currentLocale());
const locales = computed(() => supportedLocales());

const localeIcons = import.meta.glob('@/assets/themes/oruga-tailwind/img/locale/*.svg', {
  eager: true,
});
</script>

<template>
  <VDropdown :triggers="['click']" placement="bottom-end">
    <template #default="scope">
      <slot v-bind="{ ...scope, locale }"></slot>
    </template>

    <template #popper="{ hide }">
      <DropdownItem
        v-for="l in locales"
        :checked="l === locale"
        data-aq-switch-lang-item
        @click="
          () => {
            switchLanguage(l);
            hide();
          }
        "
      >
        <img
          class="w-5"
          :src="localeIcons[`/src/assets/themes/oruga-tailwind/img/locale/${l}.svg`].default"
        />
        {{ $t(`locale.${l}`) }}
      </DropdownItem>
    </template>
  </VDropdown>
</template>
