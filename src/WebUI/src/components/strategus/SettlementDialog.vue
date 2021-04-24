<template>
  <div>
    <div class="content is-medium">
      <h2>{{ settlement.name }}</h2>
      <p>
        Culture: {{ settlement.culture }}
        <br />
        Garrison: 2515
      </p>
      <div class="buttons">
        <b-button
          v-if="hero.status !== 'RecruitingInSettlement'"
          type="is-primary"
          size="is-medium"
          @click="recruitTroops(true)"
          :loading="recruitTroopsLoading"
        >
          Start recruiting troops
        </b-button>
        <b-button
          v-else
          type="is-primary"
          size="is-medium"
          @click="recruitTroops(false)"
          :loading="recruitTroopsLoading"
        >
          Stop recruiting troops
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
import Hero from '@/models/hero';
import HeroStatus from '@/models/hero-status';

@Component
export default class SettlementDialog extends Vue {
  recruitTroopsLoading = false;

  get settlement(): SettlementPublic {
    return strategusModule.hero!.targetedSettlement;
  }

  get hero(): Hero {
    return strategusModule.hero!;
  }

  recruitTroops(enable: boolean) {
    this.recruitTroopsLoading = true;
    const status = enable ? HeroStatus.RecruitingInSettlement : HeroStatus.IdleInSettlement;
    strategusModule
      .updateHeroStatus({
        status,
        waypoints: { type: 'MultiPoint', coordinates: [] },
        targetedHeroId: 0,
        targetedSettlementId: this.settlement.id,
      })
      .finally(() => (this.recruitTroopsLoading = false));
  }
}
</script>

<style scoped lang="scss"></style>
