import { useAsyncState } from '@vueuse/core';
import { ClanInvitationStatus, ClanInvitationType } from '@/models/clan';
import { getClanInvitations } from '@/services/clan-service';

export const useClanApplications = (id: number) => {
  const { state: applications, execute: loadClanApplications } = useAsyncState(
    () => getClanInvitations(id, [ClanInvitationType.Request], [ClanInvitationStatus.Pending]),
    [],
    {
      immediate: false,
    }
  );

  const applicationsCount = computed(() => applications.value.length);

  return {
    applications,
    applicationsCount,
    loadClanApplications,
  };
};
