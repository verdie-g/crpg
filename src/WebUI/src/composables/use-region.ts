import { Region } from '@/models/region';
import { useUserStore } from '@/stores/user';

export const useRegion = () => {
  const route = useRoute();
  const router = useRouter();
  const userStore = useUserStore();

  const regionModel = computed({
    get() {
      return (route.query?.region as Region) || userStore.user!.region || Region.Eu;
    },

    set(region: Region) {
      router.replace({
        query: {
          ...route.query,
          region,
        },
      });
    },
  });

  const regions = Object.keys(Region);

  return {
    regionModel,
    regions,
  };
};
