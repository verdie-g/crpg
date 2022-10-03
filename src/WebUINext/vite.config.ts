/// <reference types="vitest" />

import { fileURLToPath } from 'node:url';
import { defineConfig } from 'vite';

import Vue from '@vitejs/plugin-vue';
import Analyzer from 'rollup-plugin-analyzer';
import AutoImport from 'unplugin-auto-import/vite';

// https://vitejs.dev/config/
export default defineConfig({
  server: {
    port: 3000,
    watch: {
      usePolling: true,
    },
  },

  plugins: [
    Vue({
      include: [/\.vue$/, /\.md$/],
    }),

    AutoImport({
      imports: ['vue', 'vue-router', 'pinia', '@vueuse/head'],
      dts: './src/types/auto-imports.d.ts',
    }),

    Analyzer({ summaryOnly: true }),
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
