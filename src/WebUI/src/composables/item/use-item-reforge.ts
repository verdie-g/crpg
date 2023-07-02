import { type ItemFlat } from '@/models/item';
import { useUserStore } from '@/stores/user';
import { reforgeCostByRank } from '@/services/item-service';

export const useItemReforge = (item: ItemFlat) => {
  const userStore = useUserStore();

  const reforgeCost = computed(() => reforgeCostByRank[item.rank]);

  const reforgeCostTable = computed(() => Object.entries(reforgeCostByRank).slice(1));

  const validation = computed(() => ({
    rank: item.rank !== 0,
    gold: userStore.user!.gold > reforgeCost.value,
    exist: !userStore.userItems.some(ui => ui.item.baseId === item.baseId && ui.item.rank === 0),
  }));

  const canReforge = computed(
    () => validation.value.rank && validation.value.gold && validation.value.exist
  );

  return {
    reforgeCost,
    reforgeCostTable,
    validation,
    canReforge,
  };
};
