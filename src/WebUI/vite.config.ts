/// <reference types="vitest" />

import { fileURLToPath } from 'node:url';
import { defineConfig, Plugin } from 'vite';
import Vue from '@vitejs/plugin-vue';
import Layouts from 'vite-plugin-vue-layouts';
import Components from 'unplugin-vue-components/vite';
import VueI18n from '@intlify/unplugin-vue-i18n/vite';
import AutoImport from 'unplugin-auto-import/vite';
import { VueRouterAutoImports, getPascalCaseRouteName } from 'unplugin-vue-router';
import VueRouter from 'unplugin-vue-router/vite';
import Visualizer from 'rollup-plugin-visualizer';
import { createSvgIconsPlugin } from 'vite-plugin-svg-icons';
import viteCompression from 'vite-plugin-compression';
import topLevelAwait from 'vite-plugin-top-level-await';
import json5 from 'json5';

// TODO: to libs
function JSON5(): Plugin {
  const fileRegex = /\.(json)$/;

  return {
    name: 'vite-plugin-json5',
    enforce: 'pre', // before vite-json
    transform(src, id) {
      if (fileRegex.test(id)) {
        let value;

        try {
          value = json5.parse(src);
        } catch (error) {
          console.error(error);
        }

        return {
          code: value ? JSON.stringify(value) : src,
          map: null,
        };
      }
    },
  };
}

const watchIgnored: string[] = [];

if (process.env.NODE_ENV !== 'test') {
  watchIgnored.push('**/*.spec.**');
}

// https://vitejs.dev/config/
export default defineConfig({
  server: {
    port: 8080,
    watch: {
      usePolling: true,
      ignored: watchIgnored,
    },
  },

  plugins: [
    // https://github.com/JohnCampionJr/vite-plugin-vue-layouts
    Layouts(),

    // https://github.com/posva/unplugin-vue-router
    VueRouter({
      extensions: ['.vue'],
      exclude: ['**/*.spec*'],
      dts: 'src/types/typed-router.d.ts',
      routeBlockLang: 'yaml',
      getRouteName: getPascalCaseRouteName,
    }),

    Vue({
      reactivityTransform: true,
      script: {
        defineModel: true,
      },
    }),

    // https://github.com/antfu/unplugin-auto-import
    AutoImport({
      imports: [
        'vue',
        VueRouterAutoImports,
        'vue-i18n',
        'pinia',
        'vitest',
        '@vueuse/head',
        {
          '@vueuse/core': ['useAsyncState'],
        },
      ],
      dirs: ['src/utils/inject-strict'],
      // cache: false,
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

    // https://github.com/intlify/bundle-tools/tree/main/packages/unplugin-vue-i18n
    VueI18n({
      runtimeOnly: true,
      compositionOnly: true,
      strictMessage: false,
      include: [fileURLToPath(new URL('./locales/**', import.meta.url))],
    }),

    JSON5(),

    createSvgIconsPlugin({
      iconDirs: [fileURLToPath(new URL('./src/assets/themes/oruga-tailwind/img', import.meta.url))],
    }),

    viteCompression({
      algorithm: 'gzip',
      filter: /\.(js|css|woff2|html)$/i,
    }),

    // this is to avoid using build.target = esnext,
    // which makes the app not work in Steam (<87 Chromium)
    topLevelAwait(),
  ],

  // https://vitest.dev/api/
  test: {
    globals: true,
    environment: 'jsdom',
    clearMocks: true,
    include: ['./src/**/*.spec.ts'],
    setupFiles: ['./src/__test__/unit/setup.ts'],
    coverage: {
      provider: 'c8',
      reporter: ['json', 'text', 'html'],
      exclude: ['node_modules/', './src/__test__/unit/index.ts', '**/*.spec.ts'],
    },
  },

  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
      '@@': fileURLToPath(new URL('./', import.meta.url)),
      '@root': fileURLToPath(new URL('../../', import.meta.url)),
    },
  },
});
