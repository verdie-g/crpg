import { useIntervalFn } from '@vueuse/core';
import { useAsyncCallback } from '@/utils/useAsyncCallback';
import {
  type Party,
  type PartyVisible,
  PartyStatus,
  type PartyStatusUpdateRequest,
} from '@/models/strategus/party';
import { updatePartyStatus, getUpdate } from '@/services/strategus-service';

// Shared state
const party = ref<Party | null>(null);

// const INTERVAL = 1000 * 60 ; // 1 min
const INTERVAL = 10000; // TODO:

export const useParty = () => {
  const isRegistered = ref<boolean>(true);
  const visibleParties = ref<PartyVisible[]>([]);

  const updateParty = async () => {
    const res = await getUpdate();

    // Not registered to Strategus.
    if (res.errors !== null) {
      isRegistered.value = false;
      return;
    }

    if (res.data === null) {
      return;
    }

    party.value = res.data.party;
    visibleParties.value = res.data.visibleParties;
  };

  const { resume: startUpdatePartyInterval } = useIntervalFn(() => updateParty(), INTERVAL, {
    immediate: false,
  });

  const moveParty = async (updateRequest: Partial<PartyStatusUpdateRequest>) => {
    if (party.value === null) return;

    party.value = await updatePartyStatus({
      status: PartyStatus.MovingToPoint,
      waypoints: { type: 'MultiPoint', coordinates: [] },
      targetedPartyId: 0,
      targetedSettlementId: 0,
      ...updateRequest,
    });
  };

  const { execute: toggleRecruitTroops, loading: isTogglingRecruitTroops } = useAsyncCallback(
    async () => {
      if (party.value === null || party.value.targetedSettlement === null) return;

      party.value = await updatePartyStatus({
        status:
          party.value.status !== PartyStatus.RecruitingInSettlement
            ? PartyStatus.RecruitingInSettlement
            : PartyStatus.IdleInSettlement,
        waypoints: { type: 'MultiPoint', coordinates: [] },
        targetedPartyId: 0,
        targetedSettlementId: party.value.targetedSettlement.id,
      });
    }
  );

  return {
    isRegistered,

    party,
    updateParty,
    startUpdatePartyInterval,
    moveParty,

    visibleParties,

    toggleRecruitTroops,
    isTogglingRecruitTroops,
  };
};
