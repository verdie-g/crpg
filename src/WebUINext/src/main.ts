import { createApp } from 'vue';
import 'virtual:svg-icons-register';
import { type BootModule } from './types/boot-module';

import './assets/styles/tailwind.css';
import './assets/styles/themes/oruga-tailwind-favoras/index.css';

import App from './App.vue';

const app = createApp(App);

// Load plugins
Object.values(import.meta.glob<{ install: BootModule }>('./boot/*.ts', { eager: true })).forEach(
  module => module.install?.(app)
);

app.mount('#app');
