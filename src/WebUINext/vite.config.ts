/// <reference types="vitest" />

import { fileURLToPath } from 'node:url';
import { defineConfig } from 'vite';

import Vue from '@vitejs/plugin-vue';
import Pages from 'vite-plugin-pages';
import Layouts from 'vite-plugin-vue-layouts';
import VueI18n from '@intlify/vite-plugin-vue-i18n';
import AutoImport from 'unplugin-auto-import/vite';
import Analyzer from 'rollup-plugin-analyzer';

// https://vitejs.dev/config/
export default defineConfig({
  server: {
    port: 3000,
    watch: {
      usePolling: true,
    },
  },

  plugins: [
    Vue(),

    Pages({ exclude: ['**/*.spec*'] }),

    Layouts(),

    AutoImport({
      imports: ['vue', 'vue-router', 'vue-i18n', 'pinia', 'vitest', '@vueuse/head'],
      dts: 'src/auto-imports.d.ts',
      vueTemplate: true,
    }),

    Analyzer({ summaryOnly: true }),

    // https://github.com/intlify/bundle-tools/tree/main/packages/vite-plugin-vue-i18n
    VueI18n({
      runtimeOnly: true,
      compositionOnly: true,
      include: [fileURLToPath(new URL('./locales/**', import.meta.url))],
    }),
  ],

  test: {
    globals: true,
    environment: 'jsdom',
    clearMocks: true,
    include: ['./src/**/*.spec.ts'],
    setupFiles: [],
  },

  resolve: {
    alias: {
      '~': fileURLToPath(new URL('./src', import.meta.url)),
      '@': fileURLToPath(new URL('./src', import.meta.url)),
      '~~': fileURLToPath(new URL('./', import.meta.url)),
      '@@': fileURLToPath(new URL('./', import.meta.url)),
    },
  },

  build: {
    target: 'esnext',
  },
});
