/// <reference types="vitest" />

import { fileURLToPath } from 'node:url';
import { defineConfig } from 'vite';

import Vue from '@vitejs/plugin-vue';
import Pages from 'vite-plugin-pages';
import Layouts from 'vite-plugin-vue-layouts';
import Components from 'unplugin-vue-components/vite';
import VueI18n from '@intlify/vite-plugin-vue-i18n';
import AutoImport from 'unplugin-auto-import/vite';
import Visualizer from 'rollup-plugin-visualizer';

// https://vitejs.dev/config/
export default defineConfig({
  server: {
    port: 8080,
    watch: {
      usePolling: true,
    },
  },

  plugins: [
    Vue({ reactivityTransform: true }),

    // https://github.com/JohnCampionJr/vite-plugin-vue-layouts
    Layouts(),

    // https://github.com/hannoeru/vite-plugin-pages
    Pages({ exclude: ['**/*.spec*'] }),

    // https://github.com/antfu/unplugin-auto-import
    AutoImport({
      imports: ['vue', 'vue-router', 'vue-i18n', 'pinia', 'vitest', '@vueuse/head'],
      dts: 'src/types/vite-auto-imports.d.ts',
      vueTemplate: true,
    }),

    // TODO: maybe uninstall
    // https://github.com/antfu/unplugin-vue-components
    Components({
      dts: 'src/types/vite-components.d.ts',
    }),

    // https://github.com/btd/rollup-plugin-visualizerhttps://github.com/btd/rollup-plugin-visualizer
    Visualizer({
      template: 'sunburst',
      gzipSize: true,
      brotliSize: true,
    }),

    // https://github.com/intlify/bundle-tools/tree/main/packages/vite-plugin-vue-i18n
    VueI18n({
      runtimeOnly: true,
      compositionOnly: true,
      include: [fileURLToPath(new URL('./locales/**', import.meta.url))],
    }),
  ],

  // https://vitest.dev/api/
  test: {
    globals: true,
    environment: 'jsdom',
    clearMocks: true,
    include: ['./src/**/*.spec.ts'],
    setupFiles: ['./src/__test__/unit/index.ts'],
    coverage: {
      reporter: ['json', 'text', 'html'],
      exclude: ['node_modules/', './src/__test__/unit/index.ts', '**/*.spec.ts'],
    },
  },

  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
      '@@': fileURLToPath(new URL('./', import.meta.url)),
    },
  },
});
