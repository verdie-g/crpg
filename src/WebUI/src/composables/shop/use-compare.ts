export const useItemsCompare = () => {
  const route = useRoute();
  const router = useRouter();

  const isCompare = computed({
    set(val: boolean) {
      router.push({
        query: {
          ...route.query,
          isCompare: !val ? undefined : String(val),
        },
      });
    },

    get() {
      return route.query?.isCompare ? true : false;
    },
  });

  const toggleCompare = () => {
    isCompare.value = !isCompare.value;
  };

  const compareList = computed({
    set(val: string[]) {
      const needDisableCompare = isCompare.value && val.length <= 1;

      router.push({
        query: {
          ...route.query,
          compareList: val,
          ...(needDisableCompare && {
            isCompare: undefined,
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
    isCompare,
    toggleCompare,
    compareList,
    toggleToCompareList,
    addAllToCompareList,
    removeAllFromCompareList,
  };
};
