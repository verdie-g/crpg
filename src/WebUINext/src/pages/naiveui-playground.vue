<script setup lang="ts">
import { NButton, NConfigProvider, GlobalThemeOverrides } from 'naive-ui';
import resolveConfig from 'tailwindcss/resolveConfig';

// @ts-ignore
import tailwindConfig from '@tailwindConfig';
const { theme = {} } = resolveConfig(tailwindConfig);

const themeOverrides: GlobalThemeOverrides = {
  ...(theme && {
    Button: {
      // @ts-ignore
      textColor: theme?.colors?.['black'] || '#000',
      paddingTiny: '0 6px',
      paddingSmall: '0 10px',
      // @ts-ignore
      paddingMedium: `0 ${theme.spacing[48]}`,
      paddingLarge: '0 18px',
      paddingRoundTiny: '0 10px',
      paddingRoundSmall: '0 14px',
      paddingRoundMedium: '0 18px',
      paddingRoundLarge: '0 22px',
      iconMarginTiny: '6px',
      iconMarginSmall: '6px',
      iconMarginMedium: '6px',
      iconMarginLarge: '6px',
      iconSizeTiny: '14px',
      iconSizeSmall: '18px',
      iconSizeMedium: '18px',
      iconSizeLarge: '20px',
      rippleDuration: '.6s',

      // textColorHover: theme?.colors.indigo[900],
    },
  }),
};
</script>

<route lang="yaml">
meta:
  layout: playground
</route>

<template>
  <n-config-provider :theme-overrides="themeOverrides">
    <div>
      <h1 class="text-slate-300">Naiveui playground</h1>

      <!--  -->
      <div>
        <NButton secondary type="warning">
          <template #icon>wdd</template>
          naive-ui
        </NButton>
      </div>
      <!--  -->
    </div>
  </n-config-provider>
</template>
