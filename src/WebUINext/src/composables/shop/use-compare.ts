export const useItemsCompare = () => {
  const route = useRoute();
  const router = useRouter();

  const compareMode = computed({
    set(val: boolean) {
      router.push({
        query: {
          ...route.query,
          compareMode: !val ? undefined : String(val),
        },
      });
    },

    get() {
      return route.query?.compareMode ? true : false;
    },
  });

  const toggleCompareMode = () => {
    compareMode.value = !compareMode.value;
  };

  const compareList = computed({
    set(val: string[]) {
      const needDisableCompareMode = compareMode.value && val.length <= 1;

      router.push({
        query: {
          ...route.query,
          compareList: val,
          ...(needDisableCompareMode && {
            compareMode: undefined,
          }),
        },
      });
    },

    get() {
      return (route.query?.compareList as string[]) || [];
    },
  });

  const toggleToCompareList = (id: string) => {
    if (compareList.value.includes(id)) {
      compareList.value = [...compareList.value.filter(i => i !== id)];
      return;
    }

    compareList.value = [...compareList.value, id];
  };

  const addAllToCompareList = (ids: string[]) => {
    compareList.value = ids;
  };

  const removeAllFromCompareList = () => {
    compareList.value = [];
  };

  return {
    compareMode,
    toggleCompareMode,
    compareList,
    toggleToCompareList,
    addAllToCompareList,
    removeAllFromCompareList,
  };
};
