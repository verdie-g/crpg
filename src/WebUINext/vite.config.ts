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
import { createSvgIconsPlugin } from 'vite-plugin-svg-icons';

// https://vitejs.dev/config/
export default defineConfig({
  server: {
    port: 8080,
    watch: {
      usePolling: true,
    },
  },

  plugins: [
    Vue(),

    // https://github.com/hannoeru/vite-plugin-pages
    Pages({ exclude: ['**/*.spec*'] }),

    // https://github.com/JohnCampionJr/vite-plugin-vue-layouts
    Layouts(),

    // https://github.com/antfu/unplugin-auto-import
    AutoImport({
      imports: ['vue', 'vue-router', 'vue-i18n', 'pinia', 'vitest', '@vueuse/head'],
      dts: 'src/types/vite-auto-imports.d.ts',
      vueTemplate: true,
    }),

    // https://github.com/antfu/unplugin-vue-components
    Components({
      extensions: ['vue'],
      dts: 'src/types/vite-components.d.ts',
    }),

    // https://github.com/btd/rollup-plugin-visualizerhttps://github.com/btd/rollup-plugin-visualizer
    Visualizer({
      template: 'sunburst',
      gzipSize: true,
      open: true,
      brotliSize: true,
    }),

    // https://github.com/intlify/bundle-tools/tree/main/packages/vite-plugin-vue-i18n
    VueI18n({
      runtimeOnly: true,
      compositionOnly: true,
      include: [fileURLToPath(new URL('./locales/**', import.meta.url))],
    }),

    // https://github.com/vbenjs/vite-plugin-svg-icons
    createSvgIconsPlugin({
      // Specify the icon folder to be cached
      iconDirs: [fileURLToPath(new URL('./src/assets/icons-sprite', import.meta.url))],
      // Specify symbolId format
      symbolId: 'icon-[dir]-[name]',
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
      exclude: ['node_modules/', './src/__test__/unit/index.ts'],
    },
  },

  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
      '@@': fileURLToPath(new URL('./', import.meta.url)),
    },
  },

  build: {
    target: 'esnext',
  },
});
