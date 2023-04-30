import { useStorage } from '@vueuse/core';
import { Platform } from '@/models/platform';

export const usePlatform = () => {
  const platform = useStorage<Platform>('user-platform', Platform.Steam); // Steam by default

  const changePlatform = (p: Platform) => {
    platform.value = p;
  };

  return {
    platform: readonly(platform),
    changePlatform,
  };
};
