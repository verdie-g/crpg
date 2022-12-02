import { useDebounceFn } from '@vueuse/core';

export const useSearchDebounced = () => {
  const route = useRoute();
  const router = useRouter();

  const searchModel = computed({
    get() {
      return (route.query?.search as string) || '';
    },

    set: useDebounceFn((val: string) => {
      router.push({
        query: {
          ...route.query,
          search: val === '' ? undefined : val,
        },
      });
    }, 300),
  });

  return {
    searchModel,
  };
};
