<template>
  <section class="section">
    <b-field label="Region">
      <b-select
        v-model="selectedRegion"
        @input="getBattles(selectedRegion, ['Hiring'])"
        placeholder="Select a region"
        required
      >
        <option v-for="region in regionToStr" :key="region" :value="region">
          {{ region }}
        </option>
      </b-select>
    </b-field>
    <b-table
      :data="battles"
      ref="tableBattles"
      paginated
      per-page="10"
      detail-key="id"
      aria-next-label="Next page"
      aria-previous-label="Previous page"
      aria-page-label="Page"
      aria-current-label="Current page"
    >
      <b-table-column field="date" label="Schedule date" sortable centered v-slot="props">
        <a href="#" @click="showLongDate = !showLongDate">
          <span v-if="showLongDate">
            {{ formatDateLong(getBattleSchedulingDate(props.row)) }}
          </span>
          <span v-else>
            {{ formatDateShort(getBattleSchedulingDate(props.row)) }}
          </span>
        </a>
      </b-table-column>

      <b-table-column label="Attacker" v-slot="props">
        <div>
          <i class="fas fa-user"></i>
          {{ props.row.attacker.hero.name }} ({{ props.row.attackerTotalTroops }})
        </div>
      </b-table-column>

      <b-table-column label="Defender" v-slot="props">
        <div v-if="props.row.defender.hero">
          <i class="fas fa-user"></i>
          {{ props.row.defender.hero.name }} ({{ props.row.defenderTotalTroops }})
        </div>
        <div v-else>
          <i class="fab fa-fort-awesome"></i>
          {{ props.row.defender.settlement.name }} ({{ props.row.defenderTotalTroops }})
        </div>
      </b-table-column>

      <b-table-column label="Position" v-slot="props">
        <router-link
          :to="{
            name: 'strategus',
            params: {
              lat: props.row.position.coordinates[1],
              lng: props.row.position.coordinates[0],
            },
          }"
        >
          {{ props.row.position.coordinates[1].toFixed(2) }},
          {{ props.row.position.coordinates[0].toFixed(2) }}
        </router-link>
      </b-table-column>
    </b-table>
  </section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import strategusModule from '@/store/strategus-module';
import { regionToStr, getBattleSchedulingDate } from '@/services/strategus-service';
import { formatDateShort, formatDateLong } from '@/utils/date';
import BattleDetailed from '@/models/battle-detailed';
import Hero from '@/models/hero';
import BattlePhase from '@/models/battle-phase';
import Region from '@/models/region';

@Component
export default class Battles extends Vue {
  regionToStr = regionToStr;
  getBattleSchedulingDate = getBattleSchedulingDate;
  formatDateShort = formatDateShort;
  formatDateLong = formatDateLong;
  selectedRegion: Region = Region.Europe;
  showLongDate = false;

  get battles(): BattleDetailed[] {
    return strategusModule.battles;
  }

  get hero(): Hero | null {
    return strategusModule.hero;
  }

  async created() {
    await strategusModule.getUpdate();
    // Set default region or hero region
    this.selectedRegion = this.hero === null ? Region.Europe : this.hero.region;
    this.getBattles(this.selectedRegion, [BattlePhase.Hiring]);
  }

  getBattles(region: Region, phases: BattlePhase[]) {
    strategusModule.getBattles({ region, phases });
  }
}
</script>

<style scoped lang="scss"></style>
