<script setup lang="ts">
import { supportedLocales, currentLocale, switchLanguage } from '@/services/translate-service';

const locale = computed(() => currentLocale());
const locales = computed(() => supportedLocales());
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
        <SvgSpriteImg :name="`locale-${l}`" viewBox="0 0 18 18" class="w-5" />
        {{ $t(`locale.${l}`) }}
      </DropdownItem>
    </template>
  </VDropdown>
</template>
