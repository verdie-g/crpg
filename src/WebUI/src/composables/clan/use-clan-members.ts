import { getClanMembers } from '@/services/clan-service';

export const useClanMembers = () => {
  const { state: clanMembers, execute: loadClanMembers } = useAsyncState(
    ({ id }: { id: number }) => getClanMembers(id),
    [],
    {
      immediate: false,
    }
  );

  const clanMembersCount = computed(() => clanMembers.value.length);
  const isLastMember = computed(() => clanMembersCount.value <= 1);

  return {
    clanMembers,
    loadClanMembers,
    clanMembersCount,
    isLastMember,
  };
};
