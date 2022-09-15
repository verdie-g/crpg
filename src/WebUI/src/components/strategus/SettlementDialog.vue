<template>
  <div>
    <div class="content is-medium">
      <h2>{{ settlement.name }}</h2>
      <p>
        {{
          ($t('strategusSettlementDialogSettlementCulture'),
          { settlementCulture: settlement.culture })
        }}
        <br />
        {{ $t('strategusSettlementDialogGarrison') }}
      </p>
      <div class="buttons">
        <b-button
          v-if="party.status !== 'RecruitingInSettlement'"
          type="is-primary"
          size="is-medium"
          @click="recruitTroops(true)"
          :loading="recruitTroopsLoading"
        >
          {{ $t('strategusSettlementDialogStartRecruitingTroops') }}
        </b-button>
        <b-button
          v-else
          type="is-primary"
          size="is-medium"
          @click="recruitTroops(false)"
          :loading="recruitTroopsLoading"
        >
          {{ $t('strategusSettlementDialogStopRecruitingTroops') }}
        </b-button>
        <b-button type="is-link" size="is-medium">Shop</b-button>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { Vue, Component } from 'vue-property-decorator';
import strategusModule from '@/store/strategus-module';
import SettlementPublic from '@/models/settlement-public';
import Party from '@/models/party';
import PartyStatus from '@/models/party-status';

@Component
export default class SettlementDialog extends Vue {
  recruitTroopsLoading = false;

  get settlement(): SettlementPublic {
    return strategusModule.party!.targetedSettlement;
  }

  get party(): Party {
    return strategusModule.party!;
  }

  recruitTroops(enable: boolean) {
    this.recruitTroopsLoading = true;
    const status = enable ? PartyStatus.RecruitingInSettlement : PartyStatus.IdleInSettlement;
    strategusModule
      .updatePartyStatus({
        status,
        waypoints: { type: 'MultiPoint', coordinates: [] },
        targetedPartyId: 0,
        targetedSettlementId: this.settlement.id,
      })
      .finally(() => (this.recruitTroopsLoading = false));
  }
}
</script>

<style scoped lang="scss"></style>
