import { createApp } from 'vue';
import { type BootModule } from './types/boot-module';
import { guessDefaultLocale, switchLanguage } from '@/services/translate-service';

import 'floating-vue/dist/style.css';
import './assets/styles/tailwind.css';
import './assets/themes/oruga-tailwind/index.css';

import App from './App.vue';

const app = createApp(App);

// Load modules || plugins
Object.values(
  import.meta.glob<BootModule>('./boot/*.ts', { eager: true, import: 'install' })
).forEach(install => install(app));

await switchLanguage(guessDefaultLocale());

app.mount('#app');
