export enum Sort {
  ASC = 'asc',
  DESC = 'desc',
}

const encodeValue = (key: string, value: Sort) => `${key}_${value}`;
const decodeValue = (key: string, value: string) => value.replace(`${key}_`, '') as Sort;

export const useSort = (key: string) => {
  const route = useRoute();
  const router = useRouter();

  const sort = computed({
    set(val: Sort) {
      router.push({
        query: {
          ...route.query,
          sort: val === Sort.ASC ? undefined : encodeValue(key, val),
        },
      });
    },

    get() {
      return route.query?.sort !== undefined
        ? decodeValue(key, route.query?.sort as string)
        : Sort.ASC;
    },
  });

  const toggleSort = () => {
    sort.value = sort.value === Sort.ASC ? Sort.DESC : Sort.ASC;
  };

  return {
    sort: readonly(sort),
    toggleSort,
  };
};
