import { joinURL } from 'ufo';

// TODO: use import.meta.glob
export const getAssetUrl = (path: string, basePath = '/src/assets/') =>
  new URL(joinURL(basePath, path), import.meta.url).href;
