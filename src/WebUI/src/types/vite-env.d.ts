declare module '*.vue' {
  import { type DefineComponent } from 'vue';
  const component: DefineComponent<{}, {}, any>;
  export default component;
}

interface ImportMetaEnv {
  readonly VITE_API_BASE_URL: string;
  readonly VITE_LOCALE_DEFAULT: string;
  readonly VITE_LOCALE_FALLBACK: string;
  readonly VITE_LOCALE_SUPPORTED: string;
  readonly VITE_HH: string;
  // more env variables...
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
