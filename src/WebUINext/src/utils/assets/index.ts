import { joinURL } from 'ufo';

export const getAssetUrl = (path: string, basePath = '/src/assets/') =>
  new URL(joinURL(basePath, path), import.meta.url).href;
