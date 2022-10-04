import { createApp } from 'vue';
import { type BootModule } from './types/boot';
import './index.css';
import App from './App.vue';

const app = createApp(App);

// AUTOLOAD
Object.values(import.meta.glob<{ install: BootModule }>('./boot/*.ts', { eager: true })).forEach(
  module => module.install?.(app)
);

app.mount('#app');
